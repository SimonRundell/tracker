/**
 * AssessmentDefsPanel — admin panel for managing tblassessment_def rows.
 *
 * Left pane:  course/unit tree with filter textbox.
 * Right pane: unit name banner + assessment definitions grid with
 *             Add / Edit / Delete / Up / Down toolbar.
 *
 * API calls:
 *   GET    /assessmentdefs/index.php?unit_id=X  → list defs for a unit
 *   POST   /assessmentdefs/create.php           → create new def
 *   PUT    /assessmentdefs/update.php           → update def / reorder
 *   DELETE /assessmentdefs/delete.php           → delete def (refused if student records exist)
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
    public partial class AssessmentDefsPanel : UserControl
    {
        // ── layout ──────────────────────────────────────────────────────────────
        private SplitContainer  _mainSplit;

        // ── left pane ───────────────────────────────────────────────────────────
        private TextBox  _txtFilter;
        private TreeView _treeUnits;

        // ── right pane ──────────────────────────────────────────────────────────
        private Label        _lblUnitName;
        private DataGridView _grid;
        private Button       _btnAdd, _btnEdit, _btnDelete, _btnUp, _btnDown;
        private Label        _lblStatus;

        // ── state ────────────────────────────────────────────────────────────────
        private List<UnitForObjectivesDto>   _allUnits   = new List<UnitForObjectivesDto>();
        private List<CourseDto>              _allCourses = new List<CourseDto>();
        private List<AssessmentDefAdminDto>  _defs       = new List<AssessmentDefAdminDto>();
        private UnitForObjectivesDto         _selectedUnit;

        // ─────────────────────────────────────────────────────────────────────────

        public AssessmentDefsPanel()
        {
            Dock = DockStyle.Fill;
            Font = new Font("Trebuchet MS", 9f);
            BuildUi();

            bool splitSet = false;
            _mainSplit.SizeChanged += (s, e) =>
            {
                if (splitSet) return;
                int dist = _mainSplit.Width / 3;
                int min  = _mainSplit.Panel1MinSize;
                int max  = _mainSplit.Width - _mainSplit.Panel2MinSize - _mainSplit.SplitterWidth;
                if (dist < 120 || dist > _mainSplit.Width - 200 - _mainSplit.SplitterWidth) return;
                _mainSplit.Panel1MinSize = 120;
                _mainSplit.Panel2MinSize = 200;
                _mainSplit.SplitterDistance = dist;
                splitSet = true;
            };

            Load += async (s, e) => await LoadAllAsync();
        }

        // ── UI construction ───────────────────────────────────────────────────────

        private void BuildUi()
        {
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
            var filterPanel = new Panel { Dock = DockStyle.Top, Height = 30, Padding = new Padding(4, 4, 4, 0) };
            _txtFilter = new TextBox { Dock = DockStyle.Fill, Text = "Filter units..." };
            _txtFilter.ForeColor  = Color.Gray;
            _txtFilter.Enter     += (s, e) => { if (_txtFilter.Text == "Filter units...") { _txtFilter.Text = ""; _txtFilter.ForeColor = SystemColors.WindowText; } };
            _txtFilter.Leave     += (s, e) => { if (string.IsNullOrEmpty(_txtFilter.Text)) { _txtFilter.Text = "Filter units..."; _txtFilter.ForeColor = Color.Gray; } };
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

            // Fill first so DockStyle layout evaluates it last
            panel.Controls.Add(_treeUnits);
            panel.Controls.Add(filterPanel);
        }

        private void BuildRightPane(SplitterPanel panel)
        {
            // ── grid (Fill — added first so DockStyle processes it last) ──────────
            _grid = new DataGridView
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
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Part Name", Name = "part_name", FillWeight = 500,
                ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable,
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Order", Name = "sort_order", FillWeight = 80,
                ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable,
            });
            _grid.SelectionChanged += OnGridSelectionChanged;
            panel.Controls.Add(_grid);

            // ── status bar (Bottom) ───────────────────────────────────────────────
            _lblStatus = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 22,
                ForeColor = Color.DimGray,
                Padding   = new Padding(6, 2, 0, 0),
            };
            panel.Controls.Add(_lblStatus);

            // ── toolbar (Top) ─────────────────────────────────────────────────────
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(4, 4, 0, 0) };

            _btnAdd    = ToolBtn("+ Add",  Color.FromArgb(0, 120, 60));
            _btnEdit   = ToolBtn("Edit",   Color.FromArgb(0, 70, 127));
            _btnDelete = ToolBtn("Delete", Color.FromArgb(160, 30, 30));
            _btnUp     = ToolBtn("↑ Up",   Color.FromArgb(80, 80, 80));
            _btnDown   = ToolBtn("↓ Down", Color.FromArgb(80, 80, 80));

            int bx = 4;
            foreach (var b in new[] { _btnAdd, _btnEdit, _btnDelete, _btnUp, _btnDown })
            { b.Location = new Point(bx, 4); bx += 78; }

            _btnAdd.Click    += async (s, e) => await AddDefAsync();
            _btnEdit.Click   += async (s, e) => await EditDefAsync();
            _btnDelete.Click += async (s, e) => await DeleteDefAsync();
            _btnUp.Click     += async (s, e) => await MoveDefAsync(-1);
            _btnDown.Click   += async (s, e) => await MoveDefAsync(+1);

            toolbar.Controls.AddRange(new System.Windows.Forms.Control[]
                { _btnAdd, _btnEdit, _btnDelete, _btnUp, _btnDown });
            panel.Controls.Add(toolbar);

            // ── unit name banner (Top — added last, highest Controls[] index, processed first) ─
            var banner = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = Color.FromArgb(0, 70, 127) };
            _lblUnitName = new Label
            {
                AutoSize  = false,
                Text      = "(no unit selected)",
                Dock      = DockStyle.Fill,
                ForeColor = Color.White,
                Font      = new Font("Trebuchet MS", 9f, FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding   = new Padding(8, 0, 0, 0),
                BackColor = Color.Transparent,
            };
            banner.Controls.Add(_lblUnitName);
            panel.Controls.Add(banner);

            SetButtonsEnabled(false, false);
        }

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

        private void SetButtonsEnabled(bool unitSelected, bool rowSelected)
        {
            _btnAdd.Enabled    = unitSelected;
            _btnEdit.Enabled   = rowSelected;
            _btnDelete.Enabled = rowSelected;
            _btnUp.Enabled     = rowSelected;
            _btnDown.Enabled   = rowSelected;
        }

        private void Status(string msg) => _lblStatus.Text = msg;

        // ── data loading ──────────────────────────────────────────────────────────

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

            foreach (var course in _allCourses)
            {
                var courseUnits = units
                    .Where(u => !string.IsNullOrEmpty(u.CourseIds) &&
                                u.CourseIds.Split(',').Select(id => id.Trim()).Contains(course.Id.ToString()))
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
                    courseNode.Nodes.Add(new TreeNode($"{u.Unitcode} — {u.Unitname}") { Tag = u });

                _treeUnits.Nodes.Add(courseNode);
            }

            var unassigned = units.Where(u => string.IsNullOrEmpty(u.CourseIds)).OrderBy(u => u.Unitcode).ToList();
            if (unassigned.Any())
            {
                var node = new TreeNode("(Unassigned)")
                {
                    ForeColor = Color.Gray,
                    NodeFont  = new Font("Trebuchet MS", 9f, FontStyle.Italic),
                };
                foreach (var u in unassigned)
                    node.Nodes.Add(new TreeNode($"{u.Unitcode} — {u.Unitname}") { Tag = u });
                _treeUnits.Nodes.Add(node);
            }

            _treeUnits.ExpandAll();
            _treeUnits.EndUpdate();
        }

        private void FilterTree()
        {
            string f = _txtFilter.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(f)) { BuildTree(_allUnits); return; }
            BuildTree(_allUnits.Where(u =>
                u.Unitcode.ToLowerInvariant().Contains(f) ||
                u.Unitname.ToLowerInvariant().Contains(f)).ToList());
        }

        private async Task SelectUnitAsync(UnitForObjectivesDto unit)
        {
            _selectedUnit = unit;
            _lblUnitName.Text = $"{unit.Unitcode} — {unit.Unitname}";
            SetButtonsEnabled(true, false);
            await LoadDefsAsync();
        }

        private async Task LoadDefsAsync()
        {
            if (_selectedUnit == null) return;
            Status("Loading assessment definitions...");
            try
            {
                var resp = await ApiService.Instance.GetAsync<ListResponse<AssessmentDefAdminDto>>(
                    $"/assessmentdefs/index.php?unit_id={_selectedUnit.Id}");
                _defs = resp?.Data ?? new List<AssessmentDefAdminDto>();
                PopulateGrid();
                Status($"{_defs.Count} definition(s) for this unit.");
            }
            catch (Exception ex)
            {
                Status($"Error: {ex.Message}");
            }
        }

        private void PopulateGrid()
        {
            _grid.SelectionChanged -= OnGridSelectionChanged;
            _grid.Rows.Clear();
            foreach (var d in _defs.OrderBy(d => d.SortOrder).ThenBy(d => d.PartName))
            {
                var row = new DataGridViewRow();
                row.CreateCells(_grid, d.PartName, d.SortOrder);
                row.Tag = d;
                _grid.Rows.Add(row);
            }
            if (_grid.Rows.Count > 0)
                _grid.Rows[0].Selected = true;
            _grid.SelectionChanged += OnGridSelectionChanged;
            OnGridSelectionChanged(null, EventArgs.Empty);
        }

        private void OnGridSelectionChanged(object sender, EventArgs e)
        {
            bool has = _grid.SelectedRows.Count > 0 && _selectedUnit != null;
            SetButtonsEnabled(_selectedUnit != null, has);
        }

        private AssessmentDefAdminDto SelectedDef() =>
            _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].Tag as AssessmentDefAdminDto : null;

        // ── CRUD ──────────────────────────────────────────────────────────────────

        private async Task AddDefAsync()
        {
            if (_selectedUnit == null) return;
            int nextOrder = _defs.Any() ? _defs.Max(d => d.SortOrder) + 10 : 10;
            using var dlg = new AssessmentDefEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            Status("Adding definition...");
            try
            {
                await ApiService.Instance.PostAsync<object>("/assessmentdefs/create.php", new
                {
                    unit_id    = _selectedUnit.Id,
                    part_name  = dlg.PartName,
                    sort_order = nextOrder,
                });
                await LoadDefsAsync();
            }
            catch (Exception ex) { Status($"Error: {ex.Message}"); }
        }

        private async Task EditDefAsync()
        {
            var def = SelectedDef();
            if (def == null) return;
            using var dlg = new AssessmentDefEditDialog(def);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            Status("Updating definition...");
            try
            {
                await ApiService.Instance.PutAsync<object>("/assessmentdefs/update.php", new
                {
                    id         = def.Id,
                    part_name  = dlg.PartName,
                    sort_order = dlg.SortOrder,
                });
                await LoadDefsAsync();
            }
            catch (Exception ex) { Status($"Error: {ex.Message}"); }
        }

        private async Task DeleteDefAsync()
        {
            var def = SelectedDef();
            if (def == null) return;
            if (MessageBox.Show(
                    $"Delete definition \"{def.PartName}\"?\n\nThis cannot be undone.",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;
            Status("Deleting definition...");
            try
            {
                await ApiService.Instance.DeleteAsync("/assessmentdefs/delete.php", new { id = def.Id });
                await LoadDefsAsync();
            }
            catch (Exception ex)
            {
                Status($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Delete failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task MoveDefAsync(int direction)
        {
            var def = SelectedDef();
            if (def == null) return;
            var ordered = _defs.OrderBy(d => d.SortOrder).ThenBy(d => d.PartName).ToList();
            int idx   = ordered.FindIndex(d => d.Id == def.Id);
            int other = idx + direction;
            if (other < 0 || other >= ordered.Count) return;

            int tmp = ordered[idx].SortOrder;
            ordered[idx].SortOrder   = ordered[other].SortOrder;
            ordered[other].SortOrder = tmp;

            Status("Reordering...");
            try
            {
                await ApiService.Instance.PutAsync<object>("/assessmentdefs/update.php", new
                {
                    id         = ordered[idx].Id,
                    part_name  = ordered[idx].PartName,
                    sort_order = ordered[idx].SortOrder,
                });
                await ApiService.Instance.PutAsync<object>("/assessmentdefs/update.php", new
                {
                    id         = ordered[other].Id,
                    part_name  = ordered[other].PartName,
                    sort_order = ordered[other].SortOrder,
                });

                int movedId = def.Id;
                await LoadDefsAsync();
                foreach (DataGridViewRow row in _grid.Rows)
                {
                    if (row.Tag is AssessmentDefAdminDto d && d.Id == movedId)
                    { row.Selected = true; break; }
                }
            }
            catch (Exception ex) { Status($"Error: {ex.Message}"); }
        }
    }
}
