/**
 * ObjectivesPanel — two-pane editor for unit sections and criteria.
 *
 * Left pane: course/unit tree with filter textbox.
 * Right pane: section_type selector, sections DataGridView, criteria DataGridView.
 * Each grid has Add / Edit / Delete / Up / Down toolbar buttons.
 *
 * API calls:
 *   GET  /units/index.php                        → list of all units (with section_type, course_ids)
 *   GET  /courses/index.php                      → list of courses (for tree grouping)
 *   GET  /criteria/index.php?unit_id=X           → sections with nested criteria
 *   PUT  /units/update.php                       → save section_type on the unit
 *   POST /unitsections/create.php                → new section
 *   PUT  /unitsections/update.php                → edit / reorder section
 *   DELETE /unitsections/delete.php              → delete section (and cascades criteria)
 *   POST /criteria/create.php                    → new criterion
 *   PUT  /criteria/update.php                    → edit / reorder criterion
 *   DELETE /criteria/delete.php                  → delete criterion
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Services;

namespace AtRiskTracker.Admin
{
    public partial class ObjectivesPanel : UserControl
    {
        // ── main layout ───────────────────────────────────────────────────────
        private SplitContainer _mainSplit;

        // ── left pane controls ────────────────────────────────────────────────
        private TextBox  _txtFilter;
        private TreeView _treeUnits;

        // ── right pane: unit header ────────────────────────────────────────────
        private Label    _lblUnitName;
        private ComboBox _cboSectionType;
        private Button   _btnSaveType;

        // ── sections grid ──────────────────────────────────────────────────────
        private DataGridView _gridSections;
        private Button _btnAddSection, _btnEditSection, _btnDeleteSection;
        private Button _btnSectionUp, _btnSectionDown;

        // ── criteria grid ──────────────────────────────────────────────────────
        private Label        _lblCriteriaHeader;
        private DataGridView _gridCriteria;
        private Button _btnAddCrit, _btnEditCrit, _btnDeleteCrit;
        private Button _btnCritUp, _btnCritDown;

        // ── status bar ─────────────────────────────────────────────────────────
        private Label _lblStatus;

        // ── state ──────────────────────────────────────────────────────────────
        private List<UnitForObjectivesDto> _allUnits   = new List<UnitForObjectivesDto>();
        private List<CourseDto>            _allCourses = new List<CourseDto>();
        private List<SectionDto>           _sections   = new List<SectionDto>();
        private UnitForObjectivesDto       _selectedUnit;
        private SectionDto                 _selectedSection;

        // ── section type options ───────────────────────────────────────────────
        private static readonly (string Slug, string Display)[] SectionTypes =
        {
            ("",                    "None — direct grade entry"),
            ("grade_bands",         "Grade Bands (BTec P/M/D)"),
            ("learning_objectives", "Learning Objectives (NCFE)"),
        };

        // ─────────────────────────────────────────────────────────────────────

        public ObjectivesPanel()
        {
            Dock = DockStyle.Fill;
            Font = new Font("Trebuchet MS", 9f);
            BuildUi();

            // Set left pane to ~33 % once the panel has a real width.
            // Use a flag so we only do this once; try-catch because intermediate
            // sizing events can produce a width that briefly violates min-size constraints.
            bool splitSet = false;
            _mainSplit.SizeChanged += (s, e) =>
            {
                if (splitSet) return;
                int dist = _mainSplit.Width / 3;
                int min  = _mainSplit.Panel1MinSize;
                int max  = _mainSplit.Width - _mainSplit.Panel2MinSize - _mainSplit.SplitterWidth;
                if (dist < 120 || dist > _mainSplit.Width - 120 - _mainSplit.SplitterWidth) return;
                _mainSplit.Panel1MinSize = 120;
                _mainSplit.Panel2MinSize = 120;
                _mainSplit.SplitterDistance = dist;
                splitSet = true;
            };

            Load += async (s, e) => await LoadAllAsync();
        }

        // ── UI construction ───────────────────────────────────────────────────

        private void BuildUi()
        {
            // Main horizontal split: unit tree (~33%) on left, detail (~67%) on right
            _mainSplit = new SplitContainer
            {
                Dock        = DockStyle.Fill,
                Orientation = Orientation.Vertical,
            };

            BuildLeftPane(_mainSplit.Panel1);
            BuildRightPane(_mainSplit.Panel2);

            Controls.Add(_mainSplit);
        }

        private void BuildLeftPane(SplitterPanel panel)
        {
            // Filter textbox with placeholder-style behaviour
            var filterPanel = new Panel { Dock = DockStyle.Top, Height = 30, Padding = new Padding(4, 4, 4, 0) };
            _txtFilter = new TextBox { Dock = DockStyle.Fill, Text = "Filter units..." };
            _txtFilter.ForeColor = Color.Gray;
            _txtFilter.Enter    += (s, e) => { if (_txtFilter.Text == "Filter units...") { _txtFilter.Text = ""; _txtFilter.ForeColor = SystemColors.WindowText; } };
            _txtFilter.Leave    += (s, e) => { if (string.IsNullOrEmpty(_txtFilter.Text)) { _txtFilter.Text = "Filter units..."; _txtFilter.ForeColor = Color.Gray; } };
            _txtFilter.TextChanged += (s, e) => { if (_txtFilter.ForeColor != Color.Gray) FilterTree(); };
            filterPanel.Controls.Add(_txtFilter);

            _treeUnits = new TreeView
            {
                Dock          = DockStyle.Fill,
                HideSelection = false,
                ShowLines     = true,
                ShowPlusMinus = true,
                BorderStyle   = BorderStyle.None,
            };
            _treeUnits.AfterSelect += async (s, e) =>
            {
                if (e.Node?.Tag is UnitForObjectivesDto u)
                    await SelectUnitAsync(u);
            };

            panel.Controls.Add(_treeUnits);
            panel.Controls.Add(filterPanel);
        }

        private void BuildRightPane(SplitterPanel panel)
        {
            // Status bar
            _lblStatus = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 22,
                ForeColor = Color.DimGray,
                Padding   = new Padding(6, 2, 0, 0),
            };

            // Unit header: name + section type selector
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 68 };

            _lblUnitName = new Label
            {
                Text     = "(no unit selected)",
                Font     = new Font("Trebuchet MS", 10f, FontStyle.Bold),
                Bounds   = new Rectangle(8, 6, 620, 20),
                AutoSize = false,
            };

            var lblSectionType = new Label { Text = "Section type:", Bounds = new Rectangle(8, 34, 92, 18) };

            _cboSectionType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Bounds        = new Rectangle(102, 30, 270, 24),
            };
            foreach (var (_, display) in SectionTypes)
                _cboSectionType.Items.Add(display);
            _cboSectionType.SelectedIndex = 0;

            _btnSaveType = new Button
            {
                Text      = "Save Type",
                Bounds    = new Rectangle(378, 29, 100, 26),
                BackColor = Color.FromArgb(0, 70, 127),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled   = false,
            };
            _btnSaveType.FlatAppearance.BorderSize = 0;
            _btnSaveType.Click += async (s, e) => await SaveSectionTypeAsync();

            pnlHeader.Controls.AddRange(new Control[]
                { _lblUnitName, lblSectionType, _cboSectionType, _btnSaveType });

            // Detail split: sections on top, criteria on bottom
            var detailSplit = new SplitContainer
            {
                Dock        = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
            };
            // Defer SplitterDistance until the container has real height
            bool detailSplitSet = false;
            detailSplit.SizeChanged += (s, e) =>
            {
                if (detailSplitSet) return;
                int dist = 180;
                int min  = detailSplit.Panel1MinSize;
                int max  = detailSplit.Height - detailSplit.Panel2MinSize - detailSplit.SplitterWidth;
                if (max < min || dist < min || dist > max) return;
                detailSplit.SplitterDistance = dist;
                detailSplitSet = true;
            };

            BuildSectionsPane(detailSplit.Panel1);
            BuildCriteriaPane(detailSplit.Panel2);

            panel.Controls.Add(detailSplit);
            panel.Controls.Add(pnlHeader);
            panel.Controls.Add(_lblStatus);

            SetRightPaneEnabled(false);
        }

        private void BuildSectionsPane(SplitterPanel panel)
        {
            // Toolbar
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(4, 4, 0, 0) };
            toolbar.Controls.Add(new Label
            {
                Text     = "Sections",
                Font     = new Font("Trebuchet MS", 9f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(4, 7),
            });

            _btnAddSection    = ToolBtn("+ Add",  Color.FromArgb(0, 120, 60));
            _btnEditSection   = ToolBtn("Edit",   Color.FromArgb(0, 70, 127));
            _btnDeleteSection = ToolBtn("Delete", Color.FromArgb(160, 30, 30));
            _btnSectionUp     = ToolBtn("↑ Up",   Color.FromArgb(80, 80, 80));
            _btnSectionDown   = ToolBtn("↓ Down", Color.FromArgb(80, 80, 80));

            int bx = 78;
            foreach (var b in new[] { _btnAddSection, _btnEditSection, _btnDeleteSection, _btnSectionUp, _btnSectionDown })
            { b.Location = new Point(bx, 4); bx += 78; }

            _btnAddSection.Click    += async (s, e) => await AddSectionAsync();
            _btnEditSection.Click   += async (s, e) => await EditSectionAsync();
            _btnDeleteSection.Click += async (s, e) => await DeleteSectionAsync();
            _btnSectionUp.Click     += async (s, e) => await MoveSectionAsync(-1);
            _btnSectionDown.Click   += async (s, e) => await MoveSectionAsync(+1);

            toolbar.Controls.AddRange(new Control[]
                { _btnAddSection, _btnEditSection, _btnDeleteSection, _btnSectionUp, _btnSectionDown });

            _gridSections = BuildGrid(new[]
            {
                ("Label",  "label",      0.6f),
                ("Title",  "title",      3.5f),
                ("Order",  "sort_order", 0.5f),
            });
            _gridSections.SelectionChanged += OnSectionSelectedHandler;

            panel.Controls.Add(_gridSections);
            panel.Controls.Add(toolbar);
        }

        private void BuildCriteriaPane(SplitterPanel panel)
        {
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(4, 4, 0, 0) };

            _lblCriteriaHeader = new Label
            {
                Text     = "Criteria  (select a section above)",
                Font     = new Font("Trebuchet MS", 9f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(4, 7),
            };

            _btnAddCrit    = ToolBtn("+ Add",  Color.FromArgb(0, 120, 60));
            _btnEditCrit   = ToolBtn("Edit",   Color.FromArgb(0, 70, 127));
            _btnDeleteCrit = ToolBtn("Delete", Color.FromArgb(160, 30, 30));
            _btnCritUp     = ToolBtn("↑ Up",   Color.FromArgb(80, 80, 80));
            _btnCritDown   = ToolBtn("↓ Down", Color.FromArgb(80, 80, 80));

            int bx = 250;
            foreach (var b in new[] { _btnAddCrit, _btnEditCrit, _btnDeleteCrit, _btnCritUp, _btnCritDown })
            { b.Location = new Point(bx, 4); bx += 78; }

            _btnAddCrit.Click    += async (s, e) => await AddCriterionAsync();
            _btnEditCrit.Click   += async (s, e) => await EditCriterionAsync();
            _btnDeleteCrit.Click += async (s, e) => await DeleteCriterionAsync();
            _btnCritUp.Click     += async (s, e) => await MoveCriterionAsync(-1);
            _btnCritDown.Click   += async (s, e) => await MoveCriterionAsync(+1);

            toolbar.Controls.Add(_lblCriteriaHeader);
            toolbar.Controls.AddRange(new Control[]
                { _btnAddCrit, _btnEditCrit, _btnDeleteCrit, _btnCritUp, _btnCritDown });

            _gridCriteria = BuildGrid(new[]
            {
                ("Code",        "code",        0.7f),
                ("Description", "description", 5.5f),
                ("Order",       "sort_order",  0.4f),
            });
            _gridCriteria.SelectionChanged += (s, e) =>
            {
                bool has = _gridCriteria.SelectedRows.Count > 0;
                _btnEditCrit.Enabled   = has;
                _btnDeleteCrit.Enabled = has;
                _btnCritUp.Enabled     = has;
                _btnCritDown.Enabled   = has;
            };

            panel.Controls.Add(_gridCriteria);
            panel.Controls.Add(toolbar);
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private static Button ToolBtn(string text, Color back)
        {
            var b = new Button
            {
                Text      = text,
                Size      = new Size(74, 26),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static DataGridView BuildGrid((string header, string name, float weight)[] cols)
        {
            var gv = new DataGridView
            {
                Dock                        = DockStyle.Fill,
                ReadOnly                    = true,
                AllowUserToAddRows          = false,
                AllowUserToDeleteRows       = false,
                SelectionMode               = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect                 = false,
                AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                RowHeadersVisible           = false,
                BorderStyle                 = BorderStyle.None,
                BackgroundColor             = Color.White,
            };
            foreach (var (header, name, weight) in cols)
                gv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = header,
                    Name       = name,
                    FillWeight = weight * 100,
                    ReadOnly   = true,
                    SortMode   = DataGridViewColumnSortMode.NotSortable,
                });
            return gv;
        }

        private void SetRightPaneEnabled(bool unitSelected)
        {
            _cboSectionType.Enabled   = unitSelected;
            _btnSaveType.Enabled      = unitSelected;
            _btnAddSection.Enabled    = unitSelected;
            _btnEditSection.Enabled   = false;
            _btnDeleteSection.Enabled = false;
            _btnSectionUp.Enabled     = false;
            _btnSectionDown.Enabled   = false;
            _btnAddCrit.Enabled       = false;
            _btnEditCrit.Enabled      = false;
            _btnDeleteCrit.Enabled    = false;
            _btnCritUp.Enabled        = false;
            _btnCritDown.Enabled      = false;
        }

        private void Status(string msg) => _lblStatus.Text = msg;

        // ── data loading ──────────────────────────────────────────────────────

        private async Task LoadAllAsync()
        {
            Status("Loading units...");
            try
            {
                var cr = await ApiService.Instance.GetAsync<CoursesResponse>("/courses/index.php");
                var ur = await ApiService.Instance.GetAsync<ListResponse<UnitForObjectivesDto>>("/units/index.php");
                _allCourses = cr?.Data ?? new List<CourseDto>();
                _allUnits   = ur?.Data ?? new List<UnitForObjectivesDto>();
                BuildTree(_allUnits);
                Status($"{_allUnits.Count} units loaded.");
            }
            catch (Exception ex)
            {
                Status($"Error loading units: {ex.Message}");
            }
        }

        private void BuildTree(List<UnitForObjectivesDto> units)
        {
            _treeUnits.BeginUpdate();
            _treeUnits.Nodes.Clear();

            var headerFont = new Font("Trebuchet MS", 9f, FontStyle.Bold);

            // Group units under each course they belong to
            foreach (var course in _allCourses)
            {
                var courseUnits = units
                    .Where(u => !string.IsNullOrEmpty(u.CourseIds) &&
                                u.CourseIds.Split(',')
                                           .Select(id => id.Trim())
                                           .Contains(course.Id.ToString()))
                    .OrderBy(u => u.YearTaken ?? 0)
                    .ThenBy(u => u.Unitcode)
                    .ToList();

                if (!courseUnits.Any()) continue;

                var courseNode = new TreeNode(course.Coursename)
                {
                    ForeColor = Color.FromArgb(0, 70, 127),
                    NodeFont  = headerFont,
                };
                foreach (var u in courseUnits)
                    courseNode.Nodes.Add(UnitNode(u));

                _treeUnits.Nodes.Add(courseNode);
            }

            // Units not linked to any course
            var unassigned = units
                .Where(u => string.IsNullOrEmpty(u.CourseIds))
                .OrderBy(u => u.Unitcode)
                .ToList();

            if (unassigned.Any())
            {
                var node = new TreeNode("(Unassigned)")
                {
                    ForeColor = Color.Gray,
                    NodeFont  = new Font("Trebuchet MS", 9f, FontStyle.Italic),
                };
                foreach (var u in unassigned) node.Nodes.Add(UnitNode(u));
                _treeUnits.Nodes.Add(node);
            }

            _treeUnits.ExpandAll();
            _treeUnits.EndUpdate();
        }

        private static TreeNode UnitNode(UnitForObjectivesDto u) =>
            new TreeNode($"{u.Unitcode} — {u.Unitname}") { Tag = u };

        private void FilterTree()
        {
            string filter = _txtFilter.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(filter)) { BuildTree(_allUnits); return; }

            var filtered = _allUnits
                .Where(u => u.Unitcode.ToLowerInvariant().Contains(filter) ||
                            u.Unitname.ToLowerInvariant().Contains(filter))
                .ToList();
            BuildTree(filtered);
        }

        private async Task SelectUnitAsync(UnitForObjectivesDto unit)
        {
            _selectedUnit    = unit;
            _selectedSection = null;
            _lblUnitName.Text = $"{unit.Unitcode} — {unit.Unitname}";

            int idx = Array.FindIndex(SectionTypes, t => t.Slug == (unit.SectionType ?? ""));
            _cboSectionType.SelectedIndex = Math.Max(0, idx);

            SetRightPaneEnabled(true);
            await LoadSectionsAsync();
        }

        private async Task LoadSectionsAsync()
        {
            if (_selectedUnit == null) return;
            Status("Loading sections...");
            try
            {
                _gridSections.Rows.Clear();
                _gridCriteria.Rows.Clear();
                _lblCriteriaHeader.Text = "Criteria  (select a section above)";

                var resp = await ApiService.Instance.GetAsync<ListResponse<SectionDto>>(
                    $"/criteria/index.php?unit_id={_selectedUnit.Id}");
                _sections = resp?.Data ?? new List<SectionDto>();

                PopulateSectionsGrid();
                Status($"{_sections.Count} section(s) for this unit.");
            }
            catch (Exception ex)
            {
                Status($"Error loading sections: {ex.Message}");
            }
        }

        private void PopulateSectionsGrid()
        {
            _gridSections.SelectionChanged -= OnSectionSelectedHandler;
            _gridSections.Rows.Clear();
            foreach (var sec in _sections.OrderBy(s => s.SortOrder))
            {
                var row = new DataGridViewRow();
                row.CreateCells(_gridSections, sec.SectionLabel, sec.SectionTitle ?? "", sec.SortOrder);
                row.Tag = sec;
                _gridSections.Rows.Add(row);
            }
            if (_gridSections.Rows.Count > 0)
                _gridSections.Rows[0].Selected = true;
            _gridSections.SelectionChanged += OnSectionSelectedHandler;

            // Drive criteria population directly — SelectionChanged timing is unreliable here.
            OnSectionSelected();
        }

        private void OnSectionSelectedHandler(object sender, EventArgs e) => OnSectionSelected();

        private void OnSectionSelected()
        {
            _selectedSection = null;
            _gridCriteria.Rows.Clear();
            _lblCriteriaHeader.Text = "Criteria  (select a section above)";

            bool hasSel = _gridSections.SelectedRows.Count > 0;
            _btnEditSection.Enabled   = hasSel;
            _btnDeleteSection.Enabled = hasSel;
            _btnSectionUp.Enabled     = hasSel;
            _btnSectionDown.Enabled   = hasSel;
            _btnAddCrit.Enabled       = hasSel;

            if (!hasSel) return;

            if (!(_gridSections.SelectedRows[0].Tag is SectionDto sec)) return;
            _selectedSection = sec;

            _lblCriteriaHeader.Text = $"Criteria for: {sec.SectionLabel} — {sec.SectionTitle}";

            foreach (var c in (sec.Criteria ?? new List<CriterionDto>()).OrderBy(c => c.SortOrder))
            {
                int ri = _gridCriteria.Rows.Add(c.Code, c.Description, c.SortOrder);
                _gridCriteria.Rows[ri].Tag = c;
            }
        }

        // ── section type ──────────────────────────────────────────────────────

        private async Task SaveSectionTypeAsync()
        {
            if (_selectedUnit == null) return;
            string slug = SectionTypes[_cboSectionType.SelectedIndex].Slug;
            Status("Saving section type...");
            try
            {
                await ApiService.Instance.PutAsync<object>("/units/update.php", new
                {
                    id           = _selectedUnit.Id,
                    unitcode     = _selectedUnit.Unitcode,
                    unitname     = _selectedUnit.Unitname,
                    unitref      = _selectedUnit.Unitref     ?? "",
                    credits      = _selectedUnit.Credits,
                    glh          = _selectedUnit.Glh,
                    is_external  = _selectedUnit.IsExternal,
                    section_type = slug,
                });
                _selectedUnit.SectionType = slug;
                Status("Section type saved.");
            }
            catch (Exception ex)
            {
                Status($"Error saving section type: {ex.Message}");
            }
        }

        // ── section CRUD ──────────────────────────────────────────────────────

        private async Task AddSectionAsync()
        {
            if (_selectedUnit == null) return;
            int nextOrder = _sections.Any() ? _sections.Max(s => s.SortOrder) + 10 : 10;
            using var dlg = new SectionEditDialog(null, nextOrder);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            Status("Adding section...");
            try
            {
                await ApiService.Instance.PostAsync<object>(
                    "/unitsections/create.php", dlg.ToPayload(_selectedUnit.Id));
                await LoadSectionsAsync();
            }
            catch (Exception ex) { Status($"Error: {ex.Message}"); }
        }

        private async Task EditSectionAsync()
        {
            if (_selectedSection == null) return;
            using var dlg = new SectionEditDialog(_selectedSection, _selectedSection.SortOrder);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            Status("Updating section...");
            try
            {
                await ApiService.Instance.PutAsync<object>(
                    "/unitsections/update.php", dlg.ToPayload(_selectedUnit.Id, _selectedSection.SectionId));
                await LoadSectionsAsync();
            }
            catch (Exception ex) { Status($"Error: {ex.Message}"); }
        }

        private async Task DeleteSectionAsync()
        {
            if (_selectedSection == null) return;
            if (MessageBox.Show(
                    $"Delete section \"{_selectedSection.SectionLabel}\" and all its criteria?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;
            Status("Deleting section...");
            try
            {
                await ApiService.Instance.DeleteAsync(
                    "/unitsections/delete.php", new { id = _selectedSection.SectionId });
                await LoadSectionsAsync();
            }
            catch (Exception ex) { Status($"Error: {ex.Message}"); }
        }

        private async Task MoveSectionAsync(int direction)
        {
            if (_selectedSection == null) return;
            var ordered = _sections.OrderBy(s => s.SortOrder).ToList();
            int idx   = ordered.FindIndex(s => s.SectionId == _selectedSection.SectionId);
            int other = idx + direction;
            if (other < 0 || other >= ordered.Count) return;

            int tmp = ordered[idx].SortOrder;
            ordered[idx].SortOrder   = ordered[other].SortOrder;
            ordered[other].SortOrder = tmp;

            Status("Reordering sections...");
            try
            {
                await ApiService.Instance.PutAsync<object>("/unitsections/update.php", new
                {
                    id         = ordered[idx].SectionId,
                    unit_id    = _selectedUnit.Id,
                    label      = ordered[idx].SectionLabel,
                    title      = ordered[idx].SectionTitle,
                    sort_order = ordered[idx].SortOrder,
                });
                await ApiService.Instance.PutAsync<object>("/unitsections/update.php", new
                {
                    id         = ordered[other].SectionId,
                    unit_id    = _selectedUnit.Id,
                    label      = ordered[other].SectionLabel,
                    title      = ordered[other].SectionTitle,
                    sort_order = ordered[other].SortOrder,
                });

                int movedId = _selectedSection.SectionId;
                await LoadSectionsAsync();
                foreach (DataGridViewRow row in _gridSections.Rows)
                {
                    if (row.Tag is SectionDto s && s.SectionId == movedId)
                    { row.Selected = true; break; }
                }
            }
            catch (Exception ex) { Status($"Error: {ex.Message}"); }
        }

        // ── criteria CRUD ─────────────────────────────────────────────────────

        private SectionDto SelectedSection() =>
            _gridSections.SelectedRows.Count > 0
                ? _gridSections.SelectedRows[0].Tag as SectionDto
                : null;

        private CriterionDto SelectedCriterion() =>
            _gridCriteria.SelectedRows.Count > 0
                ? _gridCriteria.SelectedRows[0].Tag as CriterionDto
                : null;

        private async Task AddCriterionAsync()
        {
            var sec = SelectedSection();
            if (sec == null) return;
            var existing  = sec.Criteria ?? new List<CriterionDto>();
            int nextOrder = existing.Any() ? existing.Max(c => c.SortOrder) + 10 : 10;
            using var dlg = new CriterionEditDialog(null, nextOrder);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            Status("Adding criterion...");
            try
            {
                await ApiService.Instance.PostAsync<object>(
                    "/criteria/create.php", dlg.ToPayload(sec.SectionId));
                int secId = sec.SectionId;
                await LoadSectionsAsync();
                ReselectSection(secId);
            }
            catch (Exception ex) { Status($"Error: {ex.Message}"); }
        }

        private async Task EditCriterionAsync()
        {
            var crit = SelectedCriterion();
            if (crit == null) return;
            using var dlg = new CriterionEditDialog(crit, crit.SortOrder);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            Status("Updating criterion...");
            try
            {
                await ApiService.Instance.PutAsync<object>(
                    "/criteria/update.php", dlg.ToPayload(crit.SectionId, crit.Id));
                int secId = crit.SectionId;
                await LoadSectionsAsync();
                ReselectSection(secId);
            }
            catch (Exception ex) { Status($"Error: {ex.Message}"); }
        }

        private async Task DeleteCriterionAsync()
        {
            var crit = SelectedCriterion();
            if (crit == null) return;
            if (MessageBox.Show($"Delete criterion \"{crit.Code}\"?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;
            Status("Deleting criterion...");
            try
            {
                int secId = crit.SectionId;
                await ApiService.Instance.DeleteAsync("/criteria/delete.php", new { id = crit.Id });
                await LoadSectionsAsync();
                ReselectSection(secId);
            }
            catch (Exception ex) { Status($"Error: {ex.Message}"); }
        }

        private async Task MoveCriterionAsync(int direction)
        {
            var crit = SelectedCriterion();
            var sec  = SelectedSection();
            if (crit == null || sec == null) return;

            var ordered = (sec.Criteria ?? new List<CriterionDto>())
                          .OrderBy(c => c.SortOrder).ToList();
            int idx   = ordered.FindIndex(c => c.Id == crit.Id);
            int other = idx + direction;
            if (other < 0 || other >= ordered.Count) return;

            int tmp = ordered[idx].SortOrder;
            ordered[idx].SortOrder   = ordered[other].SortOrder;
            ordered[other].SortOrder = tmp;

            Status("Reordering criteria...");
            try
            {
                await ApiService.Instance.PutAsync<object>("/criteria/update.php", new
                {
                    id          = ordered[idx].Id,
                    section_id  = sec.SectionId,
                    code        = ordered[idx].Code,
                    description = ordered[idx].Description,
                    sort_order  = ordered[idx].SortOrder,
                });
                await ApiService.Instance.PutAsync<object>("/criteria/update.php", new
                {
                    id          = ordered[other].Id,
                    section_id  = sec.SectionId,
                    code        = ordered[other].Code,
                    description = ordered[other].Description,
                    sort_order  = ordered[other].SortOrder,
                });

                int movedCritId = crit.Id;
                int secId       = sec.SectionId;
                await LoadSectionsAsync();
                ReselectSection(secId);

                // Re-select the moved criterion row
                foreach (DataGridViewRow row in _gridCriteria.Rows)
                {
                    if (row.Tag is CriterionDto c && c.Id == movedCritId)
                    { row.Selected = true; break; }
                }
            }
            catch (Exception ex) { Status($"Error: {ex.Message}"); }
        }

        /// <summary>After a sections reload, re-selects the section with the given id.</summary>
        private void ReselectSection(int sectionId)
        {
            foreach (DataGridViewRow row in _gridSections.Rows)
            {
                if (row.Tag is SectionDto s && s.SectionId == sectionId)
                { row.Selected = true; break; }
            }
        }
    }

}
