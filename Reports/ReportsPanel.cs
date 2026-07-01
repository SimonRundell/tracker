/**
 * ReportsPanel — hosts all reports, rendered via the built-in WebBrowser control.
 *
 * Each report either fetches cohort data from /results/index.php and builds HTML
 * locally, or calls a dedicated audit endpoint (grade-audit.php / student-audit.php).
 * The user can print or save to PDF via the browser's own print dialog.
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

namespace AtRiskTracker.Reports
{
    public partial class ReportsPanel : UserControl
    {
        // ─── Toolbar controls ─────────────────────────────────────────────
        private Panel    _toolbar;
        private ComboBox _cboCourse;
        private ComboBox _cboGroup;
        private ComboBox _cboReport;
        private Button   _btnRun;
        private Button   _btnPrint;

        // ─── Extras row (audit-specific filters) ──────────────────────────
        private Panel          _extrasPanel;
        private Label          _lblFrom, _lblTo, _lblStudent;
        private DateTimePicker _dtpFrom, _dtpTo;
        private ComboBox       _cboStudent;

        // ─── Status + browser ─────────────────────────────────────────────
        private Label      _lblStatus;
        private WebBrowser _browser;

        // ─── Data ─────────────────────────────────────────────────────────
        private List<CourseDto>     _courses  = new List<CourseDto>();
        private List<GroupDto>      _groups   = new List<GroupDto>();
        private List<EnrollmentDto> _students = new List<EnrollmentDto>();

        private static readonly string[] ReportNames =
        {
            "At-Risk Summary",
            "Grade Distribution",
            "Unit Performance",
            "Assessment Progress",
            "Outstanding Work",
            "Grade Audit Trail",
            "Student Grade History",
        };

        public ReportsPanel()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            BuildUi();
            Load += async (s, e) => await LoadCoursesAsync();
        }

        // ----------------------------------------------------------------
        // UI construction
        // ----------------------------------------------------------------

        private void BuildUi()
        {
            // ── Main toolbar ──────────────────────────────────────────────
            _toolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 50,
                Padding   = new Padding(8, 8, 8, 4),
                BackColor = Color.FromArgb(240, 244, 248),
            };

            int x = 8;
            _toolbar.Controls.Add(Lbl("Course:", x, 15)); x += 52;
            _cboCourse = Combo(x, 10, 250);
            _cboCourse.SelectedIndexChanged += OnCourseSelected;
            _toolbar.Controls.Add(_cboCourse); x += 258;

            _toolbar.Controls.Add(Lbl("Group:", x, 15)); x += 46;
            _cboGroup = Combo(x, 10, 160);
            _cboGroup.SelectedIndexChanged += OnGroupSelected;
            _toolbar.Controls.Add(_cboGroup); x += 168;

            _toolbar.Controls.Add(Lbl("Report:", x, 15)); x += 52;
            _cboReport = Combo(x, 10, 185);
            foreach (string r in ReportNames) _cboReport.Items.Add(r);
            _cboReport.SelectedIndex = 0;
            _cboReport.SelectedIndexChanged += OnReportTypeChanged;
            _toolbar.Controls.Add(_cboReport); x += 193;

            _btnRun = Btn("Run", x, 10, Color.FromArgb(0, 110, 0));
            _btnRun.Click += OnRun;
            _toolbar.Controls.Add(_btnRun); x += 88;

            _btnPrint = Btn("Print", x, 10, Color.FromArgb(80, 80, 80));
            _btnPrint.Click += (s, e) => _browser.Print();
            _btnPrint.Enabled = false;
            _toolbar.Controls.Add(_btnPrint);

            // ── Extras panel (hidden by default) ──────────────────────────
            _extrasPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 40,
                Padding   = new Padding(8, 6, 8, 4),
                BackColor = Color.FromArgb(230, 238, 248),
                Visible   = false,
            };

            // Date range controls (Grade Audit Trail)
            _lblFrom = Lbl("From:", 8, 12);
            _dtpFrom = new DateTimePicker
            {
                Left = 50, Top = 8, Width = 120,
                Format       = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked      = false,
                Value        = DateTime.Today.AddDays(-30),
                Font         = new Font("Trebuchet MS", 9f),
            };
            _lblTo = Lbl("To:", 182, 12);
            _dtpTo = new DateTimePicker
            {
                Left = 202, Top = 8, Width = 120,
                Format       = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked      = false,
                Value        = DateTime.Today,
                Font         = new Font("Trebuchet MS", 9f),
            };
            _lblFrom.Visible = _dtpFrom.Visible = false;
            _lblTo.Visible   = _dtpTo.Visible   = false;

            // Student selector (Student Grade History)
            _lblStudent = Lbl("Student:", 8, 12);
            _cboStudent = Combo(62, 8, 300);
            _cboStudent.Items.Add("-- select student --");
            _cboStudent.SelectedIndex = 0;
            _lblStudent.Visible = _cboStudent.Visible = false;

            _extrasPanel.Controls.AddRange(new Control[]
                { _lblFrom, _dtpFrom, _lblTo, _dtpTo, _lblStudent, _cboStudent });

            // ── Status label ──────────────────────────────────────────────
            _lblStatus = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 22,
                ForeColor = Color.Gray,
                Padding   = new Padding(8, 2, 0, 0),
            };

            // ── Browser ───────────────────────────────────────────────────
            _browser = new WebBrowser
            {
                Dock                           = DockStyle.Fill,
                IsWebBrowserContextMenuEnabled = false,
                WebBrowserShortcutsEnabled     = false,
            };
            _browser.DocumentCompleted += (s, e) => _btnPrint.Enabled = true;
            _browser.DocumentText = "<html><body style='font-family:Trebuchet MS;padding:20px;'>"
                + "<p style='color:#666'>Select a course, group and report type, then click Run.</p></body></html>";

            Controls.Add(_browser);
            Controls.Add(_lblStatus);
            Controls.Add(_extrasPanel);
            Controls.Add(_toolbar);
        }

        // ----------------------------------------------------------------
        // Data loading
        // ----------------------------------------------------------------

        private async Task LoadCoursesAsync()
        {
            try
            {
                var r = await ApiService.Instance.GetAsync<CoursesResponse>("/courses/index.php");
                _courses = r?.Data ?? new List<CourseDto>();
                _cboCourse.Items.Clear();
                _cboCourse.Items.Add("-- select course --");
                foreach (var c in _courses) _cboCourse.Items.Add(c);
                _cboCourse.SelectedIndex = 0;
            }
            catch (Exception ex) { Status(ex.Message, true); }
        }

        private async void OnCourseSelected(object sender, EventArgs e)
        {
            _cboGroup.Items.Clear();
            _cboGroup.Items.Add("-- select group --");
            _cboGroup.SelectedIndex = 0;
            ClearStudents();

            if (!(_cboCourse.SelectedItem is CourseDto course)) return;
            try
            {
                var r = await ApiService.Instance.GetAsync<GroupsResponse>(
                    "/groups/index.php", $"course_id={course.Id}");
                _groups = r?.Data ?? new List<GroupDto>();
                foreach (var g in _groups) _cboGroup.Items.Add(g);
                if (_groups.Count == 1) _cboGroup.SelectedIndex = 1;
            }
            catch (Exception ex) { Status(ex.Message, true); }
        }

        private async void OnGroupSelected(object sender, EventArgs e)
        {
            ClearStudents();
            if (!(_cboGroup.SelectedItem is GroupDto group)) return;

            if ((_cboReport.SelectedItem?.ToString() ?? "") == "Student Grade History")
                await LoadStudentsAsync(group.Id);
        }

        private async Task LoadStudentsAsync(int groupId)
        {
            try
            {
                var resp = await ApiService.Instance.GetAsync<ListResponse<EnrollmentDto>>(
                    "/enrollments/index.php", $"group_id={groupId}");
                _students = resp?.Data ?? new List<EnrollmentDto>();

                _cboStudent.Items.Clear();
                _cboStudent.Items.Add("-- select student --");
                foreach (var s in _students)
                    _cboStudent.Items.Add(s);
                _cboStudent.SelectedIndex = 0;
            }
            catch (Exception ex) { Status(ex.Message, true); }
        }

        private void ClearStudents()
        {
            _students = new List<EnrollmentDto>();
            _cboStudent.Items.Clear();
            _cboStudent.Items.Add("-- select student --");
            _cboStudent.SelectedIndex = 0;
        }

        // ----------------------------------------------------------------
        // Report type selection
        // ----------------------------------------------------------------

        private void OnReportTypeChanged(object sender, EventArgs e)
        {
            string name          = _cboReport.SelectedItem?.ToString() ?? "";
            bool   isGradeAudit  = name == "Grade Audit Trail";
            bool   isStudentHist = name == "Student Grade History";

            _lblFrom.Visible    = _dtpFrom.Visible    = isGradeAudit;
            _lblTo.Visible      = _dtpTo.Visible      = isGradeAudit;
            _lblStudent.Visible = _cboStudent.Visible = isStudentHist;
            _extrasPanel.Visible = isGradeAudit || isStudentHist;

            // If switching to Student History with a group already selected, load students now
            if (isStudentHist && _cboGroup.SelectedItem is GroupDto g && _students.Count == 0)
                _ = LoadStudentsAsync(g.Id);
        }

        // ----------------------------------------------------------------
        // Run
        // ----------------------------------------------------------------

        private async void OnRun(object sender, EventArgs e)
        {
            string reportName = _cboReport.SelectedItem?.ToString() ?? "";

            if (reportName == "Grade Audit Trail")   { await RunGradeAuditAsync();   return; }
            if (reportName == "Student Grade History"){ await RunStudentAuditAsync(); return; }

            // Cohort-based reports require course + group
            if (!(_cboCourse.SelectedItem is CourseDto course)) { Status("Select a course."); return; }
            if (!(_cboGroup.SelectedItem   is GroupDto   group)) { Status("Select a group.");  return; }

            Status("Loading data...");
            _btnRun.Enabled   = false;
            _btnPrint.Enabled = false;

            try
            {
                var resp = await ApiService.Instance.GetAsync<GridResponse>(
                    "/results/index.php", $"group_id={group.Id}");
                var data = resp?.Data;
                if (data == null) { Status("No data returned."); return; }

                _browser.DocumentText = ReportBuilder.Build(reportName, course, group, data);
                Status("");
            }
            catch (Exception ex) { Status("Error: " + ex.Message, true); }
            finally { _btnRun.Enabled = true; }
        }

        private async Task RunGradeAuditAsync()
        {
            Status("Loading audit data...");
            _btnRun.Enabled   = false;
            _btnPrint.Enabled = false;

            try
            {
                var qs = new List<string>();
                if (_cboCourse.SelectedItem is CourseDto c) qs.Add($"course_id={c.Id}");
                if (_cboGroup.SelectedItem   is GroupDto  g) qs.Add($"group_id={g.Id}");
                if (_dtpFrom.Checked) qs.Add($"date_from={_dtpFrom.Value:yyyy-MM-dd}");
                if (_dtpTo.Checked)   qs.Add($"date_to={_dtpTo.Value:yyyy-MM-dd}");

                var resp = await ApiService.Instance.GetAsync<GradeAuditResponse>(
                    "/reports/grade-audit.php", string.Join("&", qs));

                if (resp == null) { Status("No response from server.", true); return; }

                _browser.DocumentText = ReportBuilder.BuildGradeAudit(
                    resp.Data ?? new List<GradeAuditRowDto>(), resp.Count);
                Status($"{resp.Count} records");
            }
            catch (Exception ex) { Status("Error: " + ex.Message, true); }
            finally { _btnRun.Enabled = true; }
        }

        private async Task RunStudentAuditAsync()
        {
            if (!(_cboStudent.SelectedItem is EnrollmentDto enr))
            { Status("Select a student."); return; }

            Status("Loading student history...");
            _btnRun.Enabled   = false;
            _btnPrint.Enabled = false;

            try
            {
                var resp = await ApiService.Instance.GetAsync<StudentAuditResponse>(
                    "/reports/student-audit.php", $"student_id={enr.StudentId}");

                if (resp == null) { Status("No response from server.", true); return; }

                _browser.DocumentText = ReportBuilder.BuildStudentAudit(
                    resp.Student, resp.Data ?? new List<StudentAuditRowDto>(), resp.Count);
                Status($"{resp.Count} records");
            }
            catch (Exception ex) { Status("Error: " + ex.Message, true); }
            finally { _btnRun.Enabled = true; }
        }

        // ----------------------------------------------------------------
        // UI helpers
        // ----------------------------------------------------------------

        private void Status(string msg, bool error = false)
        {
            _lblStatus.Text      = msg;
            _lblStatus.ForeColor = error ? Color.DarkRed : Color.Gray;
        }

        private static Label Lbl(string text, int x, int y) =>
            new Label { Text = text, AutoSize = true, Left = x, Top = y };

        private static ComboBox Combo(int x, int y, int w) =>
            new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Left = x, Top = y, Width = w,
                Font = new Font("Trebuchet MS", 9f),
            };

        private static Button Btn(string text, int x, int y, Color back)
        {
            var b = new Button
            {
                Text      = text, Left = x, Top = y, Width = 80, Height = 28,
                BackColor = back, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Trebuchet MS", 9f),
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}
