/**
 * GradeGrid — DataGridView-based grade table with frozen student columns.
 *
 * Columns: CIS No | Forename | Surname | Concern | NPs | M/D | At Risk | Overall | [units...] | [Predict]
 * Each unit cell is colour-coded by grade and opens a context menu on click.
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

namespace AtRiskTracker.Forms.Dashboard
{
    // Event args
    public class GradeUpdatedArgs : EventArgs
    {
        public int     StudentId { get; set; }
        public int     UnitId    { get; set; }
        public string  Grade     { get; set; }
        public int?    RawMark   { get; set; }
    }

    public class NotesUpdatedArgs : EventArgs
    {
        public int    StudentId { get; set; }
        public string Notes     { get; set; }
    }

    public class AssessmentUpdatedArgs : EventArgs
    {
        public int                  StudentId { get; set; }
        public int                  DefId     { get; set; }
        public AssessmentRecordDto  Record    { get; set; }
    }

    public class GradeGrid : UserControl
    {
        public event EventHandler<GradeUpdatedArgs>      GradeUpdated;
        public event EventHandler<NotesUpdatedArgs>      NotesUpdated;
        public event EventHandler<AssessmentUpdatedArgs> AssessmentUpdated;

        private DataGridView        _grid;
        private List<UnitDto>       _units;
        private List<UnitDto>       _allUnits;
        private List<StudentDto>    _students;
        private CourseDto           _course;
        private QualTypeDto         _qualType;
        private int                 _groupId;

        // Fixed column count before unit columns start
        private const int FixedCols = 8; // CIS, Fore, Sur, Concern, NPs, M/D, AtRisk, Overall

        // Grade colours
        private static readonly Dictionary<string, Color> GradeBackColors =
            new Dictionary<string, Color>
        {
            { "D",  Color.FromArgb(180, 230, 180) },
            { "D*", Color.FromArgb(160, 220, 160) },
            { "M",  Color.FromArgb(200, 220, 255) },
            { "P",  Color.FromArgb(255, 255, 200) },
            { "NP", Color.FromArgb(255, 210, 140) },
            { "U",  Color.FromArgb(255, 160, 160) },
            { "OU", Color.FromArgb(220, 220, 220) },
            { "NS", Color.White },
        };

        public GradeGrid()
        {
            Dock = DockStyle.Fill;
            _grid = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.None,
                RowHeadersVisible     = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly              = true,
                MultiSelect           = false,
                SelectionMode         = DataGridViewSelectionMode.CellSelect,
                EnableHeadersVisualStyles = false,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 70, 127),
                    ForeColor = Color.White,
                    Font      = new Font("Trebuchet MS", 8.5f, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font      = new Font("Courier New", 8.5f),
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    SelectionBackColor = Color.FromArgb(210, 230, 255),
                    SelectionForeColor = Color.Black,
                },
                RowTemplate = { Height = 26 },
            };

            _grid.CellClick  += OnCellClick;
            _grid.CellPainting += OnCellPainting;
            Controls.Add(_grid);
        }

        // ----------------------------------------------------------------
        // Public: populate grid from data
        // ----------------------------------------------------------------

        public void LoadData(List<UnitDto> units, List<UnitDto> allUnits,
            List<StudentDto> students, CourseDto course, QualTypeDto qualType, int groupId)
        {
            _units    = units;
            _allUnits = allUnits;
            _students = students;
            _course   = course;
            _qualType = qualType;
            _groupId  = groupId;

            bool isNcfe      = qualType?.IsNcfe != 0;
            bool showPredict = qualType?.ShowPredict != 0;

            _grid.SuspendLayout();
            _grid.Columns.Clear();
            _grid.Rows.Clear();

            // Fixed columns
            AddCol("CIS No",    60);
            AddCol("Forename",  90);
            AddCol("Surname",   100);
            AddCol("Concern",   90);

            if (!isNcfe)
            {
                AddCol("NPs",    40);
                AddCol("M/D",    40);
                AddCol("At Risk", 55);
            }
            else
            {
                // Still add dummy cols so FixedCols constant stays correct; hide them
                AddCol("NPs",    0, hidden: true);
                AddCol("M/D",    0, hidden: true);
                AddCol("At Risk", 0, hidden: true);
            }

            AddCol("Overall", 95);

            // Unit columns
            foreach (var u in units)
            {
                var col = AddCol(u.Unitcode, 60);
                col.Tag = u; // store unit for click handling
            }

            // Predict column
            if (showPredict)
                AddCol("Predict", 65);

            // Freeze first 4 columns (or 3 if NCFE)
            _grid.Columns[0].Frozen = true;
            _grid.Columns[1].Frozen = true;
            _grid.Columns[2].Frozen = true;

            // Populate rows
            foreach (var student in students)
            {
                PopulateRow(student, units, isNcfe, showPredict);
            }

            _grid.ResumeLayout();
        }

        private DataGridViewTextBoxColumn AddCol(string header, int width, bool hidden = false)
        {
            var col = new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                Width      = width,
                Visible    = !hidden,
                SortMode   = DataGridViewColumnSortMode.NotSortable,
            };
            _grid.Columns.Add(col);
            return col;
        }

        private void PopulateRow(StudentDto s, List<UnitDto> units, bool isNcfe, bool showPredict)
        {
            var ar = AtRiskCalc.Calc(units, s.Results, _course?.QualType ?? "other");

            string overall    = CalcOverall(s, isNcfe);
            var    row        = new DataGridViewRow();
            row.Tag           = s;

            // Build cell values
            var cells = new List<string>
            {
                s.Cisnumber ?? "-",
                s.Firstname ?? "",
                s.Lastname  ?? "",
                s.Concern   ?? "None",
                ar.NpsToCompensate.ToString(),
                ar.MeritsOrDist.ToString(),
                ar.AtRiskUnits.ToString(),
                overall,
            };

            foreach (var u in units)
            {
                string grade = GradeCalc.GetGrade(s.Results, u.Id);
                cells.Add(grade);
            }

            if (showPredict) cells.Add("Predict");

            foreach (string v in cells)
            {
                row.Cells.Add(new DataGridViewTextBoxCell { Value = v });
            }

            _grid.Rows.Add(row);

            // Colour at-risk rows
            if (ar.IsAtRisk)
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 240);
        }

        private string CalcOverall(StudentDto s, bool isNcfe)
        {
            if (_allUnits == null) return "-";
            bool isBtec = _qualType?.BtecOverallGrades != 0;

            if (isNcfe)
                return GradeCalc.CalcNcfeGrade(_allUnits, s.Results);

            if (isBtec)
            {
                int pts = GradeCalc.CalcBtecTotalPoints(_allUnits, s.Results, s.RawMarks);
                string gs = GradeCalc.CalcBtecGradeString(pts);
                string simp = GradeCalc.BtecSimplified.TryGetValue(gs, out string sv) ? sv : "U";
                return $"{simp} ({gs})";
            }

            return GradeCalc.CalcOverallGrade(_allUnits, s.Results);
        }

        // ----------------------------------------------------------------
        // Cell painting — colour unit cells by grade
        // ----------------------------------------------------------------

        private void OnCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            int unitStartCol = FixedCols;
            int unitEndCol   = unitStartCol + (_units?.Count ?? 0) - 1;

            if (e.ColumnIndex < unitStartCol || e.ColumnIndex > unitEndCol) return;

            string grade = e.Value?.ToString() ?? "NS";
            Color bg = GradeBackColors.TryGetValue(grade, out Color c) ? c : Color.White;

            e.Graphics.FillRectangle(new SolidBrush(bg), e.CellBounds);
            TextRenderer.DrawText(e.Graphics, grade, e.CellStyle.Font,
                e.CellBounds, Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
            e.Handled = true;
        }

        // ----------------------------------------------------------------
        // Cell click — open appropriate dialog
        // ----------------------------------------------------------------

        private void OnCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row     = _grid.Rows[e.RowIndex];
            var student = row.Tag as StudentDto;
            if (student == null) return;

            int col          = e.ColumnIndex;
            int unitStartCol = FixedCols;
            int unitEndCol   = unitStartCol + (_units?.Count ?? 0) - 1;
            bool showPredict = _qualType?.ShowPredict != 0;
            bool isNcfe      = _qualType?.IsNcfe != 0;

            // CIS/Name/concern columns → open notes
            if (col <= 2)
            {
                OpenNotes(student);
                return;
            }

            // Overall column (7)
            if (col == 7) return;

            // Unit columns
            if (col >= unitStartCol && col <= unitEndCol)
            {
                var unit = _units[col - unitStartCol];
                if (unit.AssessmentDefs != null && unit.AssessmentDefs.Count > 0)
                    OpenAssessments(student, unit);
                else
                    OpenObjectives(student, unit);
                return;
            }

            // Predict column
            if (showPredict && col == unitEndCol + 1)
            {
                OpenPrediction(student);
                return;
            }
        }

        // ----------------------------------------------------------------
        // Dialog openers
        // ----------------------------------------------------------------

        private void OpenNotes(StudentDto student)
        {
            var dlg = new Dialogs.NotesDialog(student);
            if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
            {
                student.Notes = dlg.Notes;
                RefreshStudentRow(student);
                NotesUpdated?.Invoke(this, new NotesUpdatedArgs
                {
                    StudentId = student.Id,
                    Notes     = dlg.Notes,
                });
            }
        }

        private void OpenObjectives(StudentDto student, UnitDto unit)
        {
            string currentGrade = GradeCalc.GetGrade(student.Results, unit.Id);
            int?   rawMark      = null;
            if (student.RawMarks != null && student.RawMarks.TryGetValue(unit.Id.ToString(), out int? rm))
                rawMark = rm;

            var dlg = new Dialogs.ObjectivesDialog(student, unit, currentGrade, rawMark, _course?.QualType);
            if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
            {
                if (student.Results == null) student.Results = new Dictionary<string, string>();
                student.Results[unit.Id.ToString()] = dlg.SelectedGrade;
                if (dlg.RawMark.HasValue)
                {
                    if (student.RawMarks == null) student.RawMarks = new Dictionary<string, int?>();
                    student.RawMarks[unit.Id.ToString()] = dlg.RawMark;
                }
                RefreshStudentRow(student);
                GradeUpdated?.Invoke(this, new GradeUpdatedArgs
                {
                    StudentId = student.Id,
                    UnitId    = unit.Id,
                    Grade     = dlg.SelectedGrade,
                    RawMark   = dlg.RawMark,
                });
            }
        }

        private void OpenAssessments(StudentDto student, UnitDto unit)
        {
            var dlg = new Dialogs.AssessmentDialog(student, unit);
            dlg.ShowDialog(FindForm());
            // Merge any saved assessments back
            foreach (var kvp in dlg.SavedAssessments)
            {
                if (student.Assessments == null)
                    student.Assessments = new Dictionary<string, AssessmentRecordDto>();
                student.Assessments[kvp.Key] = kvp.Value;
                AssessmentUpdated?.Invoke(this, new AssessmentUpdatedArgs
                {
                    StudentId = student.Id,
                    DefId     = int.Parse(kvp.Key),
                    Record    = kvp.Value,
                });
            }
        }

        private void OpenPrediction(StudentDto student)
        {
            var dlg = new Dialogs.PredictionDialog(student, _units, _course);
            dlg.ShowDialog(FindForm());
        }

        // ----------------------------------------------------------------
        // Re-render one student row after data change
        // ----------------------------------------------------------------

        public void RefreshStudentRow(StudentDto student)
        {
            bool isNcfe      = _qualType?.IsNcfe != 0;
            bool showPredict = _qualType?.ShowPredict != 0;
            var  ar          = AtRiskCalc.Calc(_units, student.Results, _course?.QualType ?? "other");
            string overall   = CalcOverall(student, isNcfe);

            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (!(row.Tag is StudentDto s) || s.Id != student.Id) continue;

                row.Cells[0].Value = student.Cisnumber ?? "-";
                row.Cells[1].Value = student.Firstname ?? "";
                row.Cells[2].Value = student.Lastname  ?? "";
                row.Cells[3].Value = student.Concern   ?? "None";
                row.Cells[4].Value = ar.NpsToCompensate.ToString();
                row.Cells[5].Value = ar.MeritsOrDist.ToString();
                row.Cells[6].Value = ar.AtRiskUnits.ToString();
                row.Cells[7].Value = overall;

                for (int i = 0; i < (_units?.Count ?? 0); i++)
                    row.Cells[FixedCols + i].Value = GradeCalc.GetGrade(student.Results, _units[i].Id);

                row.DefaultCellStyle.BackColor = ar.IsAtRisk ? Color.FromArgb(255, 240, 240) : Color.White;
                break;
            }

            _grid.Invalidate();
        }
    }
}
