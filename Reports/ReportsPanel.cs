/**
 * ReportsPanel — hosts all reports, rendered via the built-in WebBrowser control.
 *
 * Each report fetches its data from the API, builds an HTML string locally
 * and loads it into the WebBrowser. The user can then print or save to PDF
 * using the browser's own print dialog.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Services;
using Newtonsoft.Json;

namespace AtRiskTracker.Reports
{
    public class ReportsPanel : UserControl
    {
        private WebBrowser  _browser;
        private ComboBox    _cboCourse;
        private ComboBox    _cboGroup;
        private ComboBox    _cboReport;
        private Button      _btnRun;
        private Button      _btnPrint;
        private Label       _lblStatus;
        private Panel       _toolbar;

        private List<CourseDto> _courses = new List<CourseDto>();
        private List<GroupDto>  _groups  = new List<GroupDto>();

        private static readonly string[] ReportNames =
        {
            "At-Risk Summary",
            "Grade Distribution",
            "Unit Performance",
            "Assessment Progress",
            "Outstanding Work",
        };

        public ReportsPanel()
        {
            Dock = DockStyle.Fill;
            BuildUi();
            Load += async (s, e) => await LoadCoursesAsync();
        }

        private void BuildUi()
        {
            _toolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 50,
                Padding   = new Padding(8, 8, 8, 4),
                BackColor = Color.FromArgb(240, 244, 248),
            };

            int x = 8;
            _toolbar.Controls.Add(Lbl("Course:", x, 15)); x += 52;
            _cboCourse = Combo(x, 10, 250); _cboCourse.SelectedIndexChanged += OnCourseSelected; _toolbar.Controls.Add(_cboCourse); x += 258;
            _toolbar.Controls.Add(Lbl("Group:", x, 15)); x += 46;
            _cboGroup = Combo(x, 10, 170); _toolbar.Controls.Add(_cboGroup); x += 178;
            _toolbar.Controls.Add(Lbl("Report:", x, 15)); x += 52;
            _cboReport = Combo(x, 10, 180);
            foreach (string r in ReportNames) _cboReport.Items.Add(r);
            _cboReport.SelectedIndex = 0;
            _toolbar.Controls.Add(_cboReport); x += 188;

            _btnRun = Btn("Run", x, 10, Color.FromArgb(0, 127, 0)); _btnRun.Click += OnRun; _toolbar.Controls.Add(_btnRun); x += 88;
            _btnPrint = Btn("Print", x, 10, Color.FromArgb(80, 80, 80)); _btnPrint.Click += (s, e) => _browser.Print(); _btnPrint.Enabled = false; _toolbar.Controls.Add(_btnPrint);

            _lblStatus = new Label
            {
                Dock = DockStyle.Top, Height = 22,
                ForeColor = Color.Gray, Padding = new Padding(8, 2, 0, 0),
            };

            _browser = new WebBrowser
            {
                Dock          = DockStyle.Fill,
                IsWebBrowserContextMenuEnabled = false,
                WebBrowserShortcutsEnabled     = false,
            };
            _browser.DocumentCompleted += (s, e) => _btnPrint.Enabled = true;

            Controls.Add(_browser);
            Controls.Add(_lblStatus);
            Controls.Add(_toolbar);

            // Initial content
            _browser.DocumentText = "<html><body style='font-family:Trebuchet MS;padding:20px;'>"
                + "<p style='color:#666'>Select a course, group, and report type then click Run.</p></body></html>";
        }

        private async System.Threading.Tasks.Task LoadCoursesAsync()
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
            catch (Exception ex) { _lblStatus.Text = ex.Message; }
        }

        private async void OnCourseSelected(object sender, EventArgs e)
        {
            _cboGroup.Items.Clear();
            _cboGroup.Items.Add("-- select group --");
            _cboGroup.SelectedIndex = 0;
            if (!(_cboCourse.SelectedItem is CourseDto course)) return;
            try
            {
                var r = await ApiService.Instance.GetAsync<GroupsResponse>("/groups/index.php", $"course_id={course.Id}");
                _groups = r?.Data ?? new List<GroupDto>();
                foreach (var g in _groups) _cboGroup.Items.Add(g);
                if (_groups.Count == 1) _cboGroup.SelectedIndex = 1;
            }
            catch (Exception ex) { _lblStatus.Text = ex.Message; }
        }

        private async void OnRun(object sender, EventArgs e)
        {
            if (!(_cboCourse.SelectedItem is CourseDto course)) { _lblStatus.Text = "Select a course."; return; }
            if (!(_cboGroup.SelectedItem   is GroupDto   group)) { _lblStatus.Text = "Select a group.";  return; }

            _lblStatus.Text   = "Loading data...";
            _btnRun.Enabled   = false;
            _btnPrint.Enabled = false;

            try
            {
                var resp = await ApiService.Instance.GetAsync<GridResponse>(
                    "/results/index.php", $"group_id={group.Id}");
                var data = resp?.Data;
                if (data == null) { _lblStatus.Text = "No data."; return; }

                string reportName = _cboReport.SelectedItem?.ToString() ?? "";
                string html = ReportBuilder.Build(reportName, course, group, data);
                _browser.DocumentText = html;
                _lblStatus.Text = "";
            }
            catch (Exception ex) { _lblStatus.Text = "Error: " + ex.Message; }
            finally { _btnRun.Enabled = true; }
        }

        // ----------------------------------------------------------------
        // UI helpers
        // ----------------------------------------------------------------

        private static Label Lbl(string text, int x, int y) =>
            new Label { Text = text, AutoSize = true, Left = x, Top = y };

        private static ComboBox Combo(int x, int y, int w) =>
            new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = x, Top = y, Width = w,
                Font = new Font("Trebuchet MS", 9f) };

        private static Button Btn(string text, int x, int y, Color back)
        {
            var b = new Button
            {
                Text = text, Left = x, Top = y, Width = 80, Height = 28,
                BackColor = back, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Trebuchet MS", 9f),
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}
