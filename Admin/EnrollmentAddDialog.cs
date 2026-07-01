/**
 * EnrollmentAddDialog — enrol a student into a teaching group.
 *
 * The user searches for the student by typing part of their name or CIS number,
 * then selects the target course → group and optionally sets a concern.
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
    internal partial class EnrollmentAddDialog : Form
    {
        // ─── All loaded data ──────────────────────────────────────────────
        private List<StudentAdminDto> _allStudents = new List<StudentAdminDto>();
        private List<CourseDto>       _courses     = new List<CourseDto>();
        private List<GroupDto>        _groups      = new List<GroupDto>();
        private List<ConcernDto>      _concerns    = new List<ConcernDto>();

        // ─── UI ───────────────────────────────────────────────────────────
        private TextBox  _txtSearch;
        private ListBox  _lstStudents;
        private Label    _lblStudentHint;
        private ComboBox _cboCourse;
        private ComboBox _cboGroup;
        private ComboBox _cboConcern;
        private Button   _btnOk;
        private Button   _btnCancel;
        private Label    _lblStatus;

        // ─── Result properties ────────────────────────────────────────────
        public int  SelectedStudentId { get; private set; }
        public int  SelectedGroupId   { get; private set; }
        public int? SelectedConcernId { get; private set; }

        public EnrollmentAddDialog()
        {
            InitializeComponent();
            BuildUi();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void BuildUi()
        {
            Text                 = "Add Enrollment";
            FormBorderStyle      = FormBorderStyle.FixedDialog;
            MaximizeBox          = false;
            MinimizeBox          = false;
            StartPosition        = FormStartPosition.CenterParent;
            AutoScaleDimensions  = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode        = AutoScaleMode.Font;
            ClientSize           = new Size(428, 390);
            Font                 = new Font("Trebuchet MS", 9f);
            BackColor            = Color.White;
            SuspendLayout();

            int y = 12;

            Add(Lbl("Student — type to search:", 12, y)); y += 18;
            _txtSearch = new TextBox { Left = 12, Top = y, Width = 404, Font = new Font("Trebuchet MS", 9f) };
            _txtSearch.TextChanged += OnSearchChanged;
            Controls.Add(_txtSearch); y += 26;

            _lstStudents = new ListBox
            {
                Left = 12, Top = y, Width = 404, Height = 110,
                Font = new Font("Trebuchet MS", 9f),
                IntegralHeight = false,
            };
            _lstStudents.SelectedIndexChanged += OnStudentSelected;
            Controls.Add(_lstStudents); y += 116;

            _lblStudentHint = new Label
            {
                Left = 12, Top = y, AutoSize = true,
                Text = "No student selected",
                Font = new Font("Trebuchet MS", 8f, FontStyle.Italic),
                ForeColor = Color.Gray,
            };
            Controls.Add(_lblStudentHint); y += 22;

            Add(Lbl("Course:", 12, y)); y += 18;
            _cboCourse = Combo(12, y, 404);
            _cboCourse.SelectedIndexChanged += OnCourseSelected;
            Controls.Add(_cboCourse); y += 30;

            Add(Lbl("Group:", 12, y)); y += 18;
            _cboGroup = Combo(12, y, 404);
            Controls.Add(_cboGroup); y += 30;

            Add(Lbl("Concern (optional):", 12, y)); y += 18;
            _cboConcern = Combo(12, y, 404);
            Controls.Add(_cboConcern); y += 36;

            _lblStatus = new Label
            {
                Left = 12, Top = y, Width = 280, AutoSize = false, Height = 18,
                ForeColor = Color.DarkRed,
                Font = new Font("Trebuchet MS", 8f),
            };
            Controls.Add(_lblStatus);

            _btnOk = new Button
            {
                Text = "Enrol", Left = 236, Top = y - 2, Width = 88, Height = 28,
                BackColor = Color.FromArgb(0, 110, 0), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                DialogResult = DialogResult.None,
            };
            _btnOk.FlatAppearance.BorderSize  = 0;
            _btnOk.Paint += PaintColoredButton;
            _btnOk.Click += OnOk;
            Controls.Add(_btnOk);

            _btnCancel = new Button
            {
                Text = "Cancel", Left = 330, Top = y - 2, Width = 88, Height = 28,
                DialogResult = DialogResult.Cancel,
            };
            Controls.Add(_btnCancel);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;

            ResumeLayout(false);
            PerformLayout();
        }

        // Forces background + text paint — works around Win11 FlatStyle text suppression.
        private static void PaintColoredButton(object sender, PaintEventArgs e)
        {
            var btn  = (Button)sender;
            var rect = btn.ClientRectangle;
            using (var brush = new SolidBrush(btn.BackColor))
                e.Graphics.FillRectangle(brush, rect);
            TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, rect, btn.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        // ----------------------------------------------------------------
        // Data loading
        // ----------------------------------------------------------------

        private async Task LoadDataAsync()
        {
            try
            {
                var tasks = new Task[]
                {
                    LoadStudentsAsync(),
                    LoadCoursesAsync(),
                    LoadConcernsAsync(),
                };
                await Task.WhenAll(tasks);
            }
            catch (Exception ex) { _lblStatus.Text = ex.Message; }
        }

        private async Task LoadStudentsAsync()
        {
            var resp = await ApiService.Instance.GetAsync<ListResponse<StudentAdminDto>>("/students/index.php");
            _allStudents = (resp?.Data ?? new List<StudentAdminDto>())
                .OrderBy(s => s.Lastname).ThenBy(s => s.Firstname).ToList();
            RefreshStudentList();
        }

        private async Task LoadCoursesAsync()
        {
            var resp = await ApiService.Instance.GetAsync<CoursesResponse>("/courses/index.php");
            _courses = resp?.Data ?? new List<CourseDto>();
            _cboCourse.Items.Clear();
            _cboCourse.Items.Add("-- select course --");
            foreach (var c in _courses) _cboCourse.Items.Add(c);
            _cboCourse.SelectedIndex = 0;
        }

        private async Task LoadConcernsAsync()
        {
            var resp = await ApiService.Instance.GetAsync<ListResponse<ConcernDto>>("/concerns/index.php");
            _concerns = resp?.Data ?? new List<ConcernDto>();
            _cboConcern.Items.Clear();
            _cboConcern.Items.Add(new ConcernDto { Id = 0, Concern = "None" });
            foreach (var c in _concerns) _cboConcern.Items.Add(c);
            _cboConcern.DisplayMember = "Concern";
            _cboConcern.SelectedIndex = 0;
        }

        // ----------------------------------------------------------------
        // Student search
        // ----------------------------------------------------------------

        private void OnSearchChanged(object sender, EventArgs e) => RefreshStudentList();

        private void RefreshStudentList()
        {
            string q = _txtSearch.Text.Trim();
            _lstStudents.Items.Clear();

            var matches = string.IsNullOrEmpty(q)
                ? _allStudents
                : _allStudents.Where(s =>
                    (s.Lastname  ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (s.Firstname ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (s.Cisnumber ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                  .ToList();

            foreach (var s in matches)
                _lstStudents.Items.Add(s);   // uses StudentAdminDto.ToString() which shows Lastname, Firstname

            _lblStudentHint.Text      = _lstStudents.Items.Count == 0
                ? "No matching students" : "No student selected";
            _lblStudentHint.ForeColor = Color.Gray;

        }

        private void OnStudentSelected(object sender, EventArgs e)
        {
            if (_lstStudents.SelectedItem is StudentAdminDto s)
            {
                _lblStudentHint.Text      = $"Selected: {s.Lastname}, {s.Firstname} ({s.Cisnumber})";
                _lblStudentHint.ForeColor = Color.FromArgb(0, 80, 0);
            }
        }

        // ----------------------------------------------------------------
        // Course → Group cascade
        // ----------------------------------------------------------------

        private async void OnCourseSelected(object sender, EventArgs e)
        {
            _cboGroup.Items.Clear();
            _cboGroup.Items.Add("-- select group --");
            _cboGroup.SelectedIndex = 0;
            if (!(_cboCourse.SelectedItem is CourseDto course)) return;

            try
            {
                var resp = await ApiService.Instance.GetAsync<GroupsResponse>(
                    "/groups/index.php", $"course_id={course.Id}");
                _groups = resp?.Data ?? new List<GroupDto>();
                foreach (var g in _groups) _cboGroup.Items.Add(g);
                if (_groups.Count == 1) _cboGroup.SelectedIndex = 1;
            }
            catch (Exception ex) { _lblStatus.Text = ex.Message; }
        }

        // ----------------------------------------------------------------
        // OK / validate
        // ----------------------------------------------------------------

        private void OnOk(object sender, EventArgs e)
        {
            if (!(_lstStudents.SelectedItem is StudentAdminDto student))
            { _lblStatus.Text = "Select a student."; return; }

            if (!(_cboCourse.SelectedItem is CourseDto))
            { _lblStatus.Text = "Select a course."; return; }

            if (!(_cboGroup.SelectedItem is GroupDto group))
            { _lblStatus.Text = "Select a group."; return; }

            SelectedStudentId = student.Id;
            SelectedGroupId   = group.Id;
            SelectedConcernId = (_cboConcern.SelectedItem is ConcernDto c && c.Id > 0) ? c.Id : (int?)null;
            DialogResult      = DialogResult.OK;
        }

        // ----------------------------------------------------------------
        // UI helpers
        // ----------------------------------------------------------------

        private static Label Lbl(string text, int x, int y) =>
            new Label { Text = text, Left = x, Top = y, AutoSize = true };

        private static ComboBox Combo(int x, int y, int w) =>
            new ComboBox
            {
                Left = x, Top = y, Width = w,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Trebuchet MS", 9f),
            };

        private void Add(Control c) => Controls.Add(c);
    }
}
