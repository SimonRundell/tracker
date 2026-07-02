/**
 * AssessmentDialog — per-student, per-unit assessment tracking form.
 *
 * Shows all assessment parts for a unit with status dropdowns and
 * DateTimePicker controls for each date field. Save writes back to the API.
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
        private Button _btnSave;
        private Panel  _host;         // kept for Shown-event trace

        // One entry per assessment def row
        private readonly List<(int DefId, ComboBox CbxStatus,
            DateTimePicker DtpSet, DateTimePicker DtpDeadline,
            DateTimePicker DtpResub, DateTimePicker DtpComplete)> _rows = new();

        /// <summary>Output: defId → updated record, populated on Save.</summary>
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
            "Resubmission Handed In", "Complete", "Incomplete",
        };

        // (display header, pixel width) for each column
        private static readonly (string Header, int W)[] ColDefs =
        {
            ("Part",           160),
            ("Status",         160),
            ("Date Set",       130),
            ("Deadline",       130),
            ("Resub Deadline", 130),
            ("Date Completed", 130),
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
            Size            = new Size(900, 420);
            MinimumSize     = new Size(700, 320);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            Font            = new Font("Trebuchet MS", 9f);
            BackColor       = Color.White;

            const int unitBannerH = 26;
            const int colHeaderH  = 30;
            const int padX        = 8;

            // WinForms evaluates DockStyle in REVERSE Controls[] order (Controls[Count-1] first).
            // DockStyle.Fill must therefore be Controls[0] so it is evaluated LAST, after all
            // Top/Bottom controls have already claimed their slices of the client area.
            SuspendLayout();

            // ── scrollable data rows — added FIRST so it ends up at Controls[0] ──
            _host = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(_host);

            // ── button bar ──────────────────────────────────────────────────────
            var btnBar = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.WhiteSmoke };
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
            Controls.Add(btnBar);

            // ── unit name banner ────────────────────────────────────────────────
            var bannerPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = unitBannerH,
                BackColor = Color.FromArgb(0, 70, 127),
            };
            bannerPanel.Controls.Add(new Label
            {
                AutoSize  = false,
                Text      = _unit.Unitname,
                Dock      = DockStyle.Fill,
                ForeColor = Color.White,
                Font      = new Font("Trebuchet MS", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(padX, 0, 0, 0),
                BackColor = Color.Transparent,
            });
            Controls.Add(bannerPanel);

            // ── column header bar — added LAST so it ends up at Controls[Count-1] ──
            // In reverse evaluation order this is processed first, taking Y=0.
            var headerPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = colHeaderH,
                BackColor = Color.FromArgb(0, 55, 110),
            };
            int hx = padX;
            foreach (var (hdr, hw) in ColDefs)
            {
                headerPanel.Controls.Add(new Label
                {
                    AutoSize  = false,
                    Text      = hdr,
                    Location  = new Point(hx, 0),
                    Size      = new Size(hw, colHeaderH),
                    ForeColor = Color.White,
                    Font      = new Font("Trebuchet MS", 9f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(4, 0, 0, 0),
                    BackColor = Color.Transparent,
                });
                hx += hw;
            }
            Controls.Add(headerPanel);

            ResumeLayout(true);

            try
            {
                BuildRows(_host, padX);
            }
            catch (Exception ex)
            {
                _host.Controls.Add(new Label
                {
                    Text      = "Error building rows: " + ex.Message,
                    Dock      = DockStyle.Fill,
                    ForeColor = Color.Red,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(8),
                });
            }
        }

        private void BuildRows(Panel host, int padX)
        {
            const int rowH = 40;

            if (_unit.AssessmentDefs == null || _unit.AssessmentDefs.Count == 0)
            {
                host.Controls.Add(new Label
                {
                    Text      = "No assessment parts are configured for this unit.\n" +
                                "Add assessment definitions via Admin → Units.",
                    Location  = new Point(padX, 12),
                    Size      = new Size(600, 50),
                    AutoSize  = false,
                    ForeColor = Color.Gray,
                });
                return;
            }

            int y = 6;
            foreach (var def in _unit.AssessmentDefs)
            {
                var rec = _student.Assessments != null &&
                          _student.Assessments.TryGetValue(def.Id.ToString(), out var r) ? r : null;

                int x = padX;

                // Part name label
                host.Controls.Add(new Label
                {
                    Text      = def.Title ?? $"Part {def.Id}",
                    Location  = new Point(x, y + 11),
                    Size      = new Size(ColDefs[0].W - 6, rowH - 12),
                    AutoSize  = false,
                });
                x += ColDefs[0].W;

                // Status ComboBox
                var cbx = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location      = new Point(x, y + 9),
                    Size          = new Size(ColDefs[1].W - 10, 24),
                };
                foreach (string lbl in StatusLabels) cbx.Items.Add(lbl);
                string curLabel = "Not Set";
                if (rec?.Status != null)
                {
                    int si = Array.IndexOf(StatusOptions, rec.Status);
                    if (si >= 0) curLabel = StatusLabels[si];
                }
                cbx.SelectedItem = curLabel;
                host.Controls.Add(cbx);
                x += ColDefs[1].W;

                // Date pickers
                var dtpSet      = MakeDtp(rec?.DateSet,          x, y, ColDefs[2].W); x += ColDefs[2].W;
                var dtpDeadline = MakeDtp(rec?.DateDeadline,     x, y, ColDefs[3].W); x += ColDefs[3].W;
                var dtpResub    = MakeDtp(rec?.DateResubmission, x, y, ColDefs[4].W); x += ColDefs[4].W;
                var dtpComplete = MakeDtp(rec?.DateCompleted,    x, y, ColDefs[5].W);
                host.Controls.AddRange(new Control[] { dtpSet, dtpDeadline, dtpResub, dtpComplete });

                _rows.Add((def.Id, cbx, dtpSet, dtpDeadline, dtpResub, dtpComplete));

                // Row divider
                y += rowH;
                host.Controls.Add(new Label
                {
                    Location  = new Point(padX, y),
                    Size      = new Size(840, 1),
                    AutoSize  = false,
                    BackColor = Color.FromArgb(220, 220, 220),
                });
                y += 2;
            }

            // Size the host to fit all rows (enables scroll if form shrinks below content)
            host.AutoScrollMinSize = new Size(0, y + 4);
        }

        /// <summary>
        /// Creates a DateTimePicker pre-populated from an API date string (yyyy-MM-dd).
        /// ShowCheckBox lets the user clear a date by unchecking.
        /// </summary>
        private static DateTimePicker MakeDtp(string apiDate, int x, int y, int colW)
        {
            var dtp = new DateTimePicker
            {
                Location     = new Point(x, y + 8),
                Size         = new Size(colW - 8, 26),
                Format       = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                ShowCheckBox = true,
            };

            if (!string.IsNullOrEmpty(apiDate) &&
                DateTime.TryParseExact(apiDate, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var dt))
            {
                dtp.Value   = dt;
                dtp.Checked = true;
            }
            else
            {
                dtp.Value        = DateTime.Today;
                dtp.Checked      = false;
                dtp.CustomFormat = " "; // blank display for an unset date — Value is a placeholder only
            }

            // Swap between a blank display and the real date format as the checkbox is (un)ticked.
            // DateTimePicker has no CheckedChanged event, but ValueChanged also fires on a
            // checkbox toggle (the underlying native control raises it for either change).
            dtp.ValueChanged += (s, e) => dtp.CustomFormat = dtp.Checked ? "dd/MM/yyyy" : " ";

            return dtp;
        }

        private async void OnSave(object sender, EventArgs e)
        {
            _btnSave.Enabled = false;
            try
            {
                var updates = new List<object>();
                foreach (var (defId, cbxStatus, dtpSet, dtpDeadline, dtpResub, dtpComplete) in _rows)
                {
                    int si = Array.IndexOf(StatusLabels, cbxStatus.SelectedItem?.ToString() ?? "Not Set");
                    string statusVal = si >= 0 ? StatusOptions[si] : "NOT_SET";

                    string apiSet      = dtpSet.Checked      ? dtpSet.Value.ToString("yyyy-MM-dd")      : null;
                    string apiDeadline = dtpDeadline.Checked ? dtpDeadline.Value.ToString("yyyy-MM-dd") : null;
                    string apiResub    = dtpResub.Checked    ? dtpResub.Value.ToString("yyyy-MM-dd")    : null;
                    string apiComplete = dtpComplete.Checked ? dtpComplete.Value.ToString("yyyy-MM-dd") : null;

                    updates.Add(new
                    {
                        assessment_def_id = defId,
                        status            = statusVal,
                        date_set          = apiSet,
                        date_deadline     = apiDeadline,
                        date_resubmission = apiResub,
                        date_completed    = apiComplete,
                    });

                    SavedAssessments[defId.ToString()] = new AssessmentRecordDto
                    {
                        Status           = statusVal,
                        DateSet          = apiSet,
                        DateDeadline     = apiDeadline,
                        DateResubmission = apiResub,
                        DateCompleted    = apiComplete,
                    };
                }

                await ApiService.Instance.PutAsync<object>("/assessments/update.php", new
                {
                    student_id = _student.Id,
                    updates,
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
    }
}
