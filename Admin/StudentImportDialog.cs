/**
 * StudentImportDialog — bulk-import students from a UTF-8 CSV file.
 *
 * The user picks a CSV (as exported by Excel's "CSV UTF-8" format), the file
 * is parsed and previewed in a grid before anything is sent to the server.
 * Rows missing a first or last name are flagged and excluded from the import.
 * Optionally enrols every imported student into a chosen teaching group.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Services;
using AtRiskTracker.Utils;

namespace AtRiskTracker.Admin
{
    public partial class StudentImportDialog : Form
    {
        private static readonly string[] CisAliases   = { "cisnumber", "cisno", "cis", "cisid", "studentid" };
        private static readonly string[] FirstAliases = { "firstname", "forename", "givenname", "fname" };
        private static readonly string[] LastAliases  = { "lastname", "surname", "familyname", "lname" };

        private class ImportRow
        {
            public int    RowNum;
            public string Cis;
            public string First;
            public string Last;
            public bool   Valid;
            public string Reason;
        }

        private List<ImportRow> _rows = new List<ImportRow>();

        private Label    _lblFile;
        private ComboBox _cboCourse, _cboGroup;
        private Button   _btnChooseFile, _btnImport;
        private DataGridView _grid;
        private Label    _lblSummary, _lblError;

        public StudentImportDialog()
        {
            InitializeComponent();
            BuildUi();
            Load += async (s, e) => await LoadCoursesAsync();
        }

        private void BuildUi()
        {
            Text            = "Import Students from CSV";
            Size            = new Size(760, 600);
            MinimumSize     = new Size(600, 420);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            MinimizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);
            BackColor       = Color.White;

            // ── Top: instructions + file/group pickers ────────────────────
            var topBar = new Panel { Dock = DockStyle.Top, Height = 158, Padding = new Padding(10) };

            topBar.Controls.Add(new Label
            {
                Text   = "Import Students from CSV",
                Bounds = new Rectangle(10, 6, 500, 22),
                Font   = new Font("Trebuchet MS", 10f, FontStyle.Bold),
            });

            topBar.Controls.Add(new Label
            {
                Text      = "The file must be a UTF-8 CSV (in Excel: Save As → \"CSV UTF-8 (Comma delimited)\") " +
                            "with a header row. Recognised headers (any order, any case):\n" +
                            "CIS Number → \"CIS No\" / \"CIS Number\" / \"CIS\"     " +
                            "First Name → \"First Name\" / \"Forename\"     " +
                            "Last Name → \"Last Name\" / \"Surname\"",
                Bounds    = new Rectangle(10, 30, 720, 48),
                ForeColor = Color.FromArgb(70, 70, 90),
                Font      = new Font("Trebuchet MS", 8.5f),
            });

            _btnChooseFile = new Button
            {
                Text      = "Choose CSV File...",
                Bounds    = new Rectangle(10, 84, 160, 30),
                BackColor = Color.FromArgb(0, 70, 127),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnChooseFile.FlatAppearance.BorderSize = 0;
            _btnChooseFile.Click += OnChooseFile;

            _lblFile = new Label
            {
                Text     = "No file selected",
                Bounds   = new Rectangle(180, 90, 550, 20),
                ForeColor= Color.FromArgb(90, 90, 90),
            };

            topBar.Controls.Add(new Label
            {
                Text   = "Enrol into course (optional):",
                Bounds = new Rectangle(10, 126, 150, 22),
            });
            _cboCourse = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Bounds        = new Rectangle(160, 123, 190, 24),
            };
            _cboCourse.SelectedIndexChanged += async (s, e) => await OnCourseChangedAsync();

            topBar.Controls.Add(new Label
            {
                Text   = "Group:",
                Bounds = new Rectangle(360, 126, 50, 22),
            });
            _cboGroup = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Bounds        = new Rectangle(410, 123, 330, 24),
                Enabled       = false,
            };

            topBar.Controls.AddRange(new Control[] { _btnChooseFile, _lblFile, _cboCourse, _cboGroup });

            // ── Middle: preview grid ───────────────────────────────────────
            _grid = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 70, 127),
                    ForeColor = Color.White,
                    Font      = new Font("Trebuchet MS", 8.5f, FontStyle.Bold),
                },
                DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Trebuchet MS", 9f) },
                RowTemplate = { Height = 24 },
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Row",     Name = "row",    FillWeight = 0.5f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CIS No",  Name = "cis",    FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Forename",Name = "first",  FillWeight = 1.5f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Surname", Name = "last",   FillWeight = 1.5f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status",  Name = "status", FillWeight = 2f });

            // ── Bottom: summary + error + buttons ──────────────────────────
            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 78, Padding = new Padding(10) };

            _lblSummary = new Label
            {
                Text     = "Choose a CSV file to preview its rows.",
                Bounds   = new Rectangle(10, 4, 720, 20),
                Font     = new Font("Trebuchet MS", 9f, FontStyle.Bold),
            };
            _lblError = new Label
            {
                Text      = "",
                Bounds    = new Rectangle(10, 24, 720, 18),
                ForeColor = Color.DarkRed,
                Font      = new Font("Trebuchet MS", 8.5f),
            };

            _btnImport = new Button
            {
                Text      = "Import",
                Bounds    = new Rectangle(10, 44, 130, 30),
                BackColor = Color.FromArgb(0, 127, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled   = false,
            };
            _btnImport.FlatAppearance.BorderSize = 0;
            _btnImport.Click += async (s, e) => await OnImportAsync();

            var btnClose = new Button
            {
                Text = "Close", Bounds = new Rectangle(148, 44, 100, 30),
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel,
            };
            CancelButton = btnClose;

            bottomBar.Controls.AddRange(new Control[] { _lblSummary, _lblError, _btnImport, btnClose });

            Controls.Add(_grid);
            Controls.Add(bottomBar);
            Controls.Add(topBar);
        }

        private async System.Threading.Tasks.Task LoadCoursesAsync()
        {
            _cboCourse.Items.Add("— None —");
            try
            {
                var resp = await ApiService.Instance.GetAsync<CoursesResponse>("/courses/index.php");
                foreach (var c in resp?.Data ?? new List<CourseDto>())
                    _cboCourse.Items.Add(c);
            }
            catch { /* course list is optional; leave just "— None —" on failure */ }
            _cboCourse.SelectedIndex = 0;
        }

        /// <summary>Reloads the group list scoped to the selected course, so identically-named groups in different courses aren't ambiguous.</summary>
        private async System.Threading.Tasks.Task OnCourseChangedAsync()
        {
            _cboGroup.Items.Clear();
            _cboGroup.Enabled = false;

            if (!(_cboCourse.SelectedItem is CourseDto course))
            {
                _cboGroup.Items.Add("— None —");
                _cboGroup.SelectedIndex = 0;
                return;
            }

            _cboGroup.Items.Add("Loading...");
            _cboGroup.SelectedIndex = 0;

            try
            {
                var resp   = await ApiService.Instance.GetAsync<GroupsResponse>(
                    "/groups/index.php", $"course_id={course.Id}");
                var groups = resp?.Data ?? new List<GroupDto>();

                _cboGroup.Items.Clear();
                if (groups.Count == 0)
                {
                    _cboGroup.Items.Add("(no groups in this course)");
                }
                else
                {
                    foreach (var g in groups) _cboGroup.Items.Add(g);
                    _cboGroup.Enabled = true;
                }
                _cboGroup.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
        }

        private void OnChooseFile(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title  = "Select Student CSV File",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            SetError("");
            string text;
            try
            {
                text = File.ReadAllText(ofd.FileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                SetError($"Could not read file: {ex.Message}");
                return;
            }

            var rows = CsvParser.Parse(text);
            rows.RemoveAll(r => r.Length == 0 || (r.Length == 1 && string.IsNullOrWhiteSpace(r[0])));

            if (rows.Count < 2)
            {
                SetError("The file has no data rows below the header row.");
                return;
            }

            var header   = rows[0];
            int cisCol   = FindColumn(header, CisAliases);
            int firstCol = FindColumn(header, FirstAliases);
            int lastCol  = FindColumn(header, LastAliases);

            if (firstCol < 0 || lastCol < 0)
            {
                MessageBox.Show(this,
                    "Could not find First Name and/or Last Name columns in this file.\n\n" +
                    "Expected headers (any of):\n" +
                    "  CIS Number: CIS No, CIS Number, CIS\n" +
                    "  First Name: First Name, Forename\n" +
                    "  Last Name: Last Name, Surname\n\n" +
                    "Check the header row and try again.",
                    "Headers Not Recognised", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _lblFile.Text = Path.GetFileName(ofd.FileName);
            BuildPreview(rows, cisCol, firstCol, lastCol);
        }

        /// <summary>Finds the first header column whose normalised text matches one of the given aliases.</summary>
        private static int FindColumn(string[] header, string[] aliases)
        {
            for (int i = 0; i < header.Length; i++)
                if (aliases.Contains(Normalize(header[i])))
                    return i;
            return -1;
        }

        private static string Normalize(string s) =>
            new string((s ?? "").ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

        private void BuildPreview(List<string[]> rows, int cisCol, int firstCol, int lastCol)
        {
            _rows.Clear();
            _grid.Rows.Clear();

            for (int i = 1; i < rows.Count; i++)
            {
                var fields = rows[i];
                string cis   = cisCol   >= 0 && cisCol   < fields.Length ? fields[cisCol].Trim()   : "";
                string first = firstCol >= 0 && firstCol < fields.Length ? fields[firstCol].Trim() : "";
                string last  = lastCol  >= 0 && lastCol  < fields.Length ? fields[lastCol].Trim()  : "";

                if (cis == "" && first == "" && last == "") continue; // fully blank row

                bool   valid  = first != "" && last != "";
                string reason = valid ? "OK" : "Missing first and/or last name";

                var impRow = new ImportRow
                {
                    RowNum = i + 1, // +1: header was row 1
                    Cis    = cis,
                    First  = first,
                    Last   = last,
                    Valid  = valid,
                    Reason = reason,
                };
                _rows.Add(impRow);

                int idx = _grid.Rows.Add(impRow.RowNum, cis == "" ? "-" : cis, first, last, reason);
                if (!valid)
                    _grid.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
            }

            int validCount   = _rows.Count(r => r.Valid);
            int invalidCount = _rows.Count - validCount;

            _lblSummary.Text = $"{_rows.Count} row(s) read — {validCount} will be imported" +
                                (invalidCount > 0 ? $", {invalidCount} skipped (missing name)." : ".");
            _btnImport.Enabled = validCount > 0;
        }

        private async System.Threading.Tasks.Task OnImportAsync()
        {
            var validRows = _rows.Where(r => r.Valid).ToList();
            if (validRows.Count == 0) return;

            SetError("");
            _btnImport.Enabled      = false;
            _btnChooseFile.Enabled  = false;

            try
            {
                int? groupId = (_cboGroup.SelectedItem as GroupDto)?.Id;

                var payload = new
                {
                    students = validRows.Select(r => new
                    {
                        firstname = r.First,
                        lastname  = r.Last,
                        cisnumber = string.IsNullOrEmpty(r.Cis) ? null : r.Cis,
                    }).ToList(),
                    group_id = groupId,
                };

                var resp   = await ApiService.Instance.PostAsync<SingleResponse<StudentImportResultDto>>(
                    "/students/import.php", payload);
                var result = resp?.Data ?? new StudentImportResultDto();

                string msg = $"{result.Imported} student(s) added, {result.Updated} updated.";
                var errors = result.Errors ?? new List<StudentImportErrorDto>();
                if (errors.Count > 0)
                {
                    msg += $"\n\n{errors.Count} row(s) failed:\n" +
                           string.Join("\n", errors.Take(10).Select(er => $"Row {er.Row}: {er.Message}"));
                    if (errors.Count > 10) msg += $"\n… and {errors.Count - 10} more.";
                }

                MessageBox.Show(this, msg, "Import Complete", MessageBoxButtons.OK,
                    errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                _btnImport.Enabled     = true;
                _btnChooseFile.Enabled = true;
            }
        }

        private void SetError(string msg) => _lblError.Text = msg ?? "";
    }
}
