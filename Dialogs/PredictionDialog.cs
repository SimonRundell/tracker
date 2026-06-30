/**
 * PredictionDialog — shows what grades a student needs in remaining units
 * to reach a target overall grade (P / M / D).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Utils;

namespace AtRiskTracker.Dialogs
{
    public class PredictionDialog : Form
    {
        private readonly StudentDto    _student;
        private readonly List<UnitDto> _units;
        private readonly CourseDto     _course;

        private ComboBox     _cboTarget;
        private DataGridView _grid;

        private static readonly string[] Targets = { "PPP", "MMM", "DDD" };

        public PredictionDialog(StudentDto student, List<UnitDto> units, CourseDto course)
        {
            _student = student;
            _units   = units;
            _course  = course;
            BuildUi();
        }

        private void BuildUi()
        {
            Text            = $"Grade Prediction — {_student.Firstname} {_student.Lastname}";
            Size            = new Size(600, 500);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            Font            = new Font("Trebuchet MS", 9f);

            var topBar = new Panel
            {
                Dock    = DockStyle.Top,
                Height  = 44,
                Padding = new Padding(10, 8, 0, 0),
            };
            topBar.Controls.Add(new Label { Text = "Target grade:", Bounds = new Rectangle(10, 12, 100, 22) });

            _cboTarget = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Bounds        = new Rectangle(115, 10, 120, 24),
            };
            foreach (string t in Targets) _cboTarget.Items.Add(t);
            _cboTarget.SelectedIndex = 0;
            _cboTarget.SelectedIndexChanged += (s, e) => Refresh();
            topBar.Controls.Add(_cboTarget);

            // Current overall
            bool isBtec = _course?.QualType?.StartsWith("btec") ?? false;
            int  pts    = isBtec
                ? GradeCalc.CalcBtecTotalPoints(_units, _student.Results, _student.RawMarks)
                : 0;
            string currentGrade = isBtec
                ? GradeCalc.CalcBtecGradeString(pts)
                : GradeCalc.CalcOverallGrade(_units, _student.Results);

            topBar.Controls.Add(new Label
            {
                Text   = $"Current overall: {currentGrade}",
                Bounds = new Rectangle(250, 12, 300, 22),
                Font   = new Font("Trebuchet MS", 9f, FontStyle.Bold),
            });

            _grid = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                ReadOnly              = true,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 70, 127),
                    ForeColor = Color.White,
                    Font      = new Font("Trebuchet MS", 8.5f, FontStyle.Bold),
                },
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unit",     FillWeight = 45 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Current",  FillWeight = 20 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Need",     FillWeight = 20 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Achievable", FillWeight = 15 });

            var btnClose = new Button
            {
                Text = "Close", Width = 100, Height = 30,
                Dock = DockStyle.Bottom,
                FlatStyle = FlatStyle.Flat,
            };
            btnClose.Click += (s, e) => Close();

            Controls.Add(_grid);
            Controls.Add(topBar);
            Controls.Add(btnClose);

            RefreshGrid();
        }

        private new void Refresh() => RefreshGrid();

        private void RefreshGrid()
        {
            _grid.Rows.Clear();
            string target = _cboTarget.SelectedItem?.ToString() ?? "PPP";

            // Find the target minimum points
            int targetPts = 0;
            foreach (var (min, grade) in GradeCalc.BtecExtDipGrades)
                if (grade == target) { targetPts = min; break; }

            int currentPts = GradeCalc.CalcBtecTotalPoints(
                _units, _student.Results, _student.RawMarks);
            int deficit = Math.Max(0, targetPts - currentPts);

            var ungraded = _units
                .Where(u => { string g = GradeCalc.GetGrade(_student.Results, u.Id); return g == "NS" || g == "OU"; })
                .OrderByDescending(u => u.Glh)
                .ToList();

            // Show all graded units first
            foreach (var u in _units.Except(ungraded))
            {
                string g = GradeCalc.GetGrade(_student.Results, u.Id);
                int? rawMark = null;
                if (_student.RawMarks != null && _student.RawMarks.TryGetValue(u.Id.ToString(), out int? rm))
                    rawMark = rm;
                string disp = u.IsExternal != 0 && rawMark.HasValue ? $"{rawMark} ({g})" : g;
                _grid.Rows.Add(u.Unitcode, disp, "—", "—");
                _grid.Rows[_grid.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Gray;
            }

            // Ungraded units with predictions
            foreach (var u in ungraded)
            {
                if (deficit <= 0)
                {
                    _grid.Rows.Add(u.Unitcode, "NS", "Any", "Yes");
                    continue;
                }

                string reqGrade;
                if (u.IsExternal != 0)
                {
                    int maxMark = u.Glh == 120 ? 32 : 24;
                    int reqMark = Math.Min((int)Math.Ceiling((double)deficit), maxMark);
                    reqGrade    = GradeCalc.MarkToGrade(reqMark, u.Glh) + $" ({reqMark})";
                    deficit    -= reqMark;
                }
                else
                {
                    var pts = new Dictionary<string, int>
                    {
                        { "P", u.Glh == 60 ? 6 : u.Glh == 90 ? 9 : u.Glh == 120 ? 12 : 15 },
                        { "M", u.Glh == 60 ? 10 : u.Glh == 90 ? 15 : u.Glh == 120 ? 20 : 25 },
                        { "D", u.Glh == 60 ? 16 : u.Glh == 90 ? 24 : u.Glh == 120 ? 32 : 40 },
                    };
                    string chosen = null;
                    foreach (string g in new[] { "P", "M", "D" })
                        if (pts[g] >= deficit) { chosen = g; break; }
                    chosen = chosen ?? "D";
                    reqGrade = chosen;
                    deficit -= pts[chosen];
                }

                bool achievable = deficit <= 0;
                var row = _grid.Rows[_grid.Rows.Add(u.Unitcode, "NS", reqGrade,
                    achievable ? "Yes" : "No")];
                if (!achievable)
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                else
                    row.DefaultCellStyle.ForeColor = Color.DarkGreen;
            }
        }
    }
}
