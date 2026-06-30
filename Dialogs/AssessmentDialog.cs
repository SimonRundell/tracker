/**
 * AssessmentDialog — per-student, per-unit assessment tracking form.
 *
 * Shows all assessment parts for a unit (status + dates) and saves on OK.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Services;

namespace AtRiskTracker.Dialogs
{
    public partial class AssessmentDialog : Form
    {
        private readonly StudentDto _student;
        private readonly UnitDto    _unit;

        private DataGridView _grid;
        private Button       _btnSave;

        // Output: defId (string) -> updated record
        public Dictionary<string, AssessmentRecordDto> SavedAssessments { get; }
            = new Dictionary<string, AssessmentRecordDto>();

        private static readonly string[] StatusOptions =
        {
            "NOT_SET", "SET", "HANDED_IN_1", "RETURNED",
            "HANDED_IN_2", "COMPLETE", "INCOMPLETE",
        };

        private static readonly string[] StatusLabels =
        {
            "Not Set", "Set", "Handed In", "Returned",
            "Handed In (Resubmission)", "Complete", "Incomplete",
        };

        public AssessmentDialog(StudentDto student, UnitDto unit)
        {
            InitializeComponent();
            _student = student;
            _unit    = unit;
            BuildUi();
        }

        private void BuildUi()
        {
            Text            = $"Assessments — {_student.Firstname} {_student.Lastname} — {_unit.Unitcode}";
            Size            = new Size(820, 420);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            Font            = new Font("Trebuchet MS", 9f);

            Controls.Add(new Label
            {
                Text   = _unit.Unitname,
                Dock   = DockStyle.Top,
                Height = 24,
                Font   = new Font("Trebuchet MS", 9f, FontStyle.Bold),
                Padding= new Padding(8, 4, 0, 0),
            });

            _grid = new DataGridView
            {
                Dock                    = DockStyle.Fill,
                AllowUserToAddRows      = false,
                AllowUserToDeleteRows   = false,
                RowHeadersVisible       = false,
                AutoSizeColumnsMode     = DataGridViewAutoSizeColumnsMode.Fill,
                EditMode                = DataGridViewEditMode.EditOnEnter,
                BackgroundColor         = Color.White,
                BorderStyle             = BorderStyle.None,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 70, 127),
                    ForeColor = Color.White,
                    Font      = new Font("Trebuchet MS", 8.5f, FontStyle.Bold),
                },
            };

            // Part column
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Part", Name = "part", ReadOnly = true,
                FillWeight = 25,
            });

            // Status column — ComboBox
            var statusCol = new DataGridViewComboBoxColumn
            {
                HeaderText = "Status", Name = "status",
                FillWeight = 25,
            };
            foreach (string s in StatusLabels) statusCol.Items.Add(s);
            _grid.Columns.Add(statusCol);

            // Date columns
            foreach (var (name, header) in new[]
            {
                ("date_set",          "Date Set"),
                ("date_deadline",     "Deadline"),
                ("date_resubmission", "Resubmission"),
                ("date_completed",    "Completed"),
            })
            {
                _grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = header, Name = name, FillWeight = 12.5f,
                });
            }

            // Populate rows
            if (_unit.AssessmentDefs != null)
            {
                foreach (var def in _unit.AssessmentDefs)
                {
                    var rec = _student.Assessments != null &&
                              _student.Assessments.TryGetValue(def.Id.ToString(), out var r) ? r : null;

                    string statusLabel = "Not Set";
                    if (rec?.Status != null)
                    {
                        int idx = Array.IndexOf(StatusOptions, rec.Status);
                        if (idx >= 0) statusLabel = StatusLabels[idx];
                    }

                    var row = new DataGridViewRow { Tag = def.Id };
                    row.CreateCells(_grid,
                        def.Title,
                        statusLabel,
                        rec?.Status == null ? "" : "",  // date_set placeholder
                        "", "", "");

                    // Pull actual date values
                    _grid.Rows.Add(row);
                    int ri = _grid.Rows.Count - 1;
                    _grid.Rows[ri].Cells["date_set"].Value          = rec != null ? "" : "";
                    _grid.Rows[ri].Cells["date_deadline"].Value     = "";
                    _grid.Rows[ri].Cells["date_resubmission"].Value = "";
                    _grid.Rows[ri].Cells["date_completed"].Value    = "";
                }
            }

            var btnBar = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };
            _btnSave = new Button
            {
                Text      = "Save",
                Width     = 100, Height = 30, Left = 8, Top = 7,
                BackColor = Color.FromArgb(0, 70, 127), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += OnSave;

            var btnCancel = new Button
            {
                Text = "Cancel", Width = 100, Height = 30, Left = 118, Top = 7,
                FlatStyle = FlatStyle.Flat,
            };
            btnCancel.Click += (s, e) => Close();
            btnBar.Controls.AddRange(new Control[] { _btnSave, btnCancel });

            Controls.Add(_grid);
            Controls.Add(btnBar);
        }

        private async void OnSave(object sender, EventArgs e)
        {
            _btnSave.Enabled = false;
            try
            {
                var updates = new List<object>();
                foreach (DataGridViewRow row in _grid.Rows)
                {
                    if (row.Tag == null) continue;
                    int defId = (int)row.Tag;

                    string statusLabel = row.Cells["status"].Value?.ToString() ?? "Not Set";
                    int idx = Array.IndexOf(StatusLabels, statusLabel);
                    string statusVal = idx >= 0 ? StatusOptions[idx] : "NOT_SET";

                    updates.Add(new
                    {
                        assessment_def_id = defId,
                        status            = statusVal,
                        date_set          = NullIfEmpty(row.Cells["date_set"].Value?.ToString()),
                        date_deadline     = NullIfEmpty(row.Cells["date_deadline"].Value?.ToString()),
                        date_resubmission = NullIfEmpty(row.Cells["date_resubmission"].Value?.ToString()),
                        date_completed    = NullIfEmpty(row.Cells["date_completed"].Value?.ToString()),
                    });

                    SavedAssessments[defId.ToString()] = new AssessmentRecordDto
                    {
                        Status = statusVal,
                    };
                }

                await ApiService.Instance.PutAsync<object>("/assessments/update.php", new
                {
                    student_id = _student.Id,
                    updates    = updates,
                });
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving assessments: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _btnSave.Enabled = true;
            }
        }

        private static string NullIfEmpty(string s) =>
            string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
