/**
 * DashboardPanel — main panel containing the course/group selector and grade grid.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Reports;
using AtRiskTracker.Services;

namespace AtRiskTracker.Forms.Dashboard
{
    public partial class DashboardPanel : UserControl
    {
        private ComboBox   _cboCourse;
        private ComboBox   _cboGroup;
        private ComboBox   _cboYear;
        private Button     _btnPrint;
        private Panel      _selectorBar;
        private GradeGrid  _gradeGrid;
        private Label      _lblStatus;

        private List<CourseDto>   _courses   = new List<CourseDto>();
        private List<GroupDto>    _groups    = new List<GroupDto>();
        private GridDataDto       _gridData;
        private CourseDto         _selectedCourse;
        private int?              _selectedGroupId;

        public DashboardPanel()
        {
            InitializeComponent();
            BuildUi();
            Load += async (s, e) => await LoadCoursesAsync();
        }

        private void BuildUi()
        {
            Dock      = DockStyle.Fill;
            BackColor = Color.White;

            _selectorBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 92,
                Padding   = new Padding(8, 8, 8, 16),
                BackColor = Color.FromArgb(240, 244, 248),
            };

            // Row 1: course — given its own row and a lot of width, since many
            // course names ("Introduction to the principles of...") are long
            // and were getting truncated in a narrower shared-row combo.
            var lblCourse = new Label { Text = "Course:", AutoSize = true, Top = 12, Left = 10 };
            _cboCourse = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 650, Top = 8, Left = 70,
                Font  = new Font("Trebuchet MS", 9f),
            };
            _cboCourse.SelectedIndexChanged += OnCourseSelected;

            // Row 2: group / year / print
            var lblGroup = new Label { Text = "Group:", AutoSize = true, Top = 50, Left = 10 };
            _cboGroup = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 200, Top = 46, Left = 70,
                Font  = new Font("Trebuchet MS", 9f),
            };
            _cboGroup.SelectedIndexChanged += OnGroupSelected;

            var lblYear = new Label { Text = "Year:", AutoSize = true, Top = 50, Left = 284 };
            _cboYear = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 100, Top = 46, Left = 330,
                Font  = new Font("Trebuchet MS", 9f),
            };
            _cboYear.Items.Add("All Years");
            _cboYear.SelectedIndex = 0;
            _cboYear.SelectedIndexChanged += OnYearFilterChanged;

            _btnPrint = new Button
            {
                Text      = "Print / PDF",
                Top       = 44, Left = 444,
                Width     = 100, Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                Enabled   = false,
            };
            _btnPrint.FlatAppearance.BorderSize = 0;
            _btnPrint.Click += OnPrint;

            _selectorBar.Controls.AddRange(new Control[]
                { lblCourse, _cboCourse, lblGroup, _cboGroup, lblYear, _cboYear, _btnPrint });

            _lblStatus = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 32,
                Text      = "Select a course and group to view the grade grid.",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(8, 4, 0, 0),
                ForeColor = Color.Gray,
            };

            _gradeGrid = new GradeGrid
            {
                Dock    = DockStyle.Fill,
                Visible = false,
            };
            _gradeGrid.GradeUpdated      += OnGradeUpdated;
            _gradeGrid.NotesUpdated      += OnNotesUpdated;
            _gradeGrid.AssessmentUpdated += OnAssessmentUpdated;

            Controls.Add(_gradeGrid);
            Controls.Add(_lblStatus);
            Controls.Add(_selectorBar);
        }

        // ----------------------------------------------------------------
        // Public: called by MainForm when dashboard tab is activated
        // ----------------------------------------------------------------

        public async System.Threading.Tasks.Task RefreshAsync()
        {
            if (_selectedGroupId.HasValue)
                await LoadGridAsync(_selectedGroupId.Value);
        }

        // ----------------------------------------------------------------
        // Data loading
        // ----------------------------------------------------------------

        private async System.Threading.Tasks.Task LoadCoursesAsync()
        {
            try
            {
                var cr = await ApiService.Instance.GetAsync<CoursesResponse>("/courses/index.php");
                _courses = cr?.Data ?? new List<CourseDto>();

                _cboCourse.Items.Clear();
                _cboCourse.Items.Add("-- select course --");
                foreach (var c in _courses) _cboCourse.Items.Add(c);
                _cboCourse.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                _lblStatus.Text      = "Error loading courses: " + ex.Message;
                _lblStatus.ForeColor = Color.DarkRed;
            }
        }

        private async void OnCourseSelected(object sender, EventArgs e)
        {
            _cboGroup.Items.Clear();
            _cboGroup.Items.Add("-- select group --");
            _cboGroup.SelectedIndex = 0;
            _gradeGrid.Visible      = false;
            _btnPrint.Enabled       = false;

            if (!(_cboCourse.SelectedItem is CourseDto course)) return;
            _selectedCourse = course;

            try
            {
                var gr = await ApiService.Instance.GetAsync<GroupsResponse>(
                    "/groups/index.php", $"course_id={course.Id}");
                _groups = gr?.Data ?? new List<GroupDto>();

                foreach (var g in _groups) _cboGroup.Items.Add(g);
                if (_groups.Count == 1)
                {
                    _cboGroup.SelectedIndex = 1;
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text      = "Error loading groups: " + ex.Message;
                _lblStatus.ForeColor = Color.DarkRed;
            }
        }

        private async void OnGroupSelected(object sender, EventArgs e)
        {
            if (!(_cboGroup.SelectedItem is GroupDto group)) return;
            _selectedGroupId = group.Id;
            await LoadGridAsync(group.Id);
        }

        private async System.Threading.Tasks.Task LoadGridAsync(int groupId)
        {
            _lblStatus.Text      = "Loading...";
            _lblStatus.ForeColor = Color.Gray;
            _gradeGrid.Visible   = false;
            _btnPrint.Enabled    = false;

            try
            {
                var resp = await ApiService.Instance.GetAsync<GridResponse>(
                    "/results/index.php", $"group_id={groupId}");
                _gridData = resp?.Data;

                if (_gridData == null || _gridData.Students.Count == 0)
                {
                    _lblStatus.Text      = "No students found in this group.";
                    _lblStatus.ForeColor = Color.Gray;
                    return;
                }

                PopulateYearFilter();
                RefreshGrid();
                _lblStatus.Text   = "";
                _btnPrint.Enabled = true;
            }
            catch (Exception ex)
            {
                _lblStatus.Text      = "Error loading grid: " + ex.Message;
                _lblStatus.ForeColor = Color.DarkRed;
            }
        }

        private void PopulateYearFilter()
        {
            _cboYear.SelectedIndexChanged -= OnYearFilterChanged;
            _cboYear.Items.Clear();
            _cboYear.Items.Add("All Years");

            if (_gridData?.Units != null)
            {
                var years = _gridData.Units
                    .Select(u => u.YearTaken ?? 1)
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList();

                if (years.Count > 1)
                    foreach (int y in years) _cboYear.Items.Add($"Year {y}");
            }

            _cboYear.SelectedIndex = 0;
            _cboYear.SelectedIndexChanged += OnYearFilterChanged;
        }

        private void OnYearFilterChanged(object sender, EventArgs e) => RefreshGrid();

        private void RefreshGrid()
        {
            if (_gridData == null) return;

            List<UnitDto> visibleUnits;
            if (_cboYear.SelectedItem is string sel && sel.StartsWith("Year ") &&
                int.TryParse(sel.Substring(5), out int yr))
                visibleUnits = _gridData.Units.Where(u => (u.YearTaken ?? 1) == yr).ToList();
            else
                visibleUnits = _gridData.Units;

            var qt = QualTypeDto.FromSlug(_selectedCourse?.QualType);

            _gradeGrid.LoadData(
                visibleUnits,
                _gridData.Units,
                _gridData.Students,
                _selectedCourse,
                qt,
                _selectedGroupId ?? 0);

            _gradeGrid.Visible = true;
        }

        // ----------------------------------------------------------------
        // Event forwarders from GradeGrid
        // ----------------------------------------------------------------

        private void OnGradeUpdated(object sender, GradeUpdatedArgs e)
        {
            if (_gridData == null) return;
            var student = _gridData.Students.Find(s => s.Id == e.StudentId);
            if (student == null) return;
            if (student.Results == null) student.Results = new Dictionary<string, string>();
            student.Results[e.UnitId.ToString()] = e.Grade;
            if (e.RawMark.HasValue)
            {
                if (student.RawMarks == null) student.RawMarks = new Dictionary<string, int?>();
                student.RawMarks[e.UnitId.ToString()] = e.RawMark;
            }
        }

        private void OnNotesUpdated(object sender, NotesUpdatedArgs e)
        {
            if (_gridData == null) return;
            var student = _gridData.Students.Find(s => s.Id == e.StudentId);
            if (student != null) student.Notes = e.Notes;
        }

        private void OnAssessmentUpdated(object sender, AssessmentUpdatedArgs e)
        {
            if (_gridData == null) return;
            var student = _gridData.Students.Find(s => s.Id == e.StudentId);
            if (student == null) return;
            if (student.Assessments == null)
                student.Assessments = new Dictionary<string, AssessmentRecordDto>();
            student.Assessments[e.DefId.ToString()] = e.Record;
        }

        private async void OnPrint(object sender, EventArgs e)
        {
            if (_gridData == null || _selectedCourse == null) return;
            await PrintHelper.PrintReportAsync(
                _selectedCourse,
                _cboGroup.SelectedItem?.ToString() ?? "",
                _gridData);
        }
    }
}
