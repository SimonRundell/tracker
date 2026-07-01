/**
 * EnrollmentsPanel — admin CRUD for student group allocations.
 *
 * Each row represents one tblstudent_group record: a student enrolled in
 * a single teaching group. A student studying three NCFE courses will appear
 * three times, once per group.
 *
 * Add   → EnrollmentAddDialog (student search + course → group cascade)
 * Edit  → inline concern picker
 * Delete → removes the student from that group (results are kept)
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Services;

namespace AtRiskTracker.Admin
{
    public partial class EnrollmentsPanel : AdminPanelBase
    {
        private TextBox  _filterStudent;
        private TextBox  _filterCourse;

        public EnrollmentsPanel() { InitializeComponent(); }


        // ----------------------------------------------------------------
        // Columns + filter bar
        // ----------------------------------------------------------------

        protected override void DefineColumns()
        {
            AddColText("ID",       "id",         0.3f);
            AddColText("Student",  "student",    1.5f);
            AddColText("CIS No",   "cisnumber",  0.7f);
            AddColText("Course",   "course",     1.5f);
            AddColText("Group",    "group",      0.8f);
            AddColText("Concern",  "concern",    0.8f);
            HideIdColumn();
        }

        protected override Control BuildFilterBar()
        {
            var bar = MakeFilterPanel();
            _filterStudent = AddFilterBox(bar, "Student name",  52,  160);
            _filterCourse  = AddFilterBox(bar, "Course",       220,  200);
            AddClearButton(bar, 428, _filterStudent, _filterCourse);
            return bar;
        }

        protected override bool RowMatchesFilter(DataGridViewRow row)
            => CellContains(row, 1, _filterStudent?.Text)
            && CellContains(row, 3, _filterCourse?.Text);

        // ----------------------------------------------------------------
        // Data loading
        // ----------------------------------------------------------------

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<EnrollmentDto>>(
                "/enrollments/index.php", "all=1");

            foreach (var e in resp?.Data ?? new List<EnrollmentDto>())
            {
                var row = AddRow(
                    e.Id,
                    $"{e.Lastname}, {e.Firstname}",
                    e.Cisnumber,
                    e.Coursename,
                    e.Groupname,
                    e.Concern ?? "None");
                row.Tag = e;
            }
        }

        // ----------------------------------------------------------------
        // Add — open dialog, POST, reload
        // ----------------------------------------------------------------

        protected override async Task AddItemAsync()
        {
            using var dlg = new EnrollmentAddDialog();
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

            try
            {
                await ApiService.Instance.PostAsync<object>("/enrollments/create.php", new
                {
                    student_id = dlg.SelectedStudentId,
                    group_id   = dlg.SelectedGroupId,
                    concern_id = dlg.SelectedConcernId,
                });
            }
            catch (Exception ex)
            {
                // create.php returns 409 for duplicate enrollments
                SetError(ex.Message);
                return;
            }

            await ReloadAsync();
        }

        // ----------------------------------------------------------------
        // Edit — change concern only
        // ----------------------------------------------------------------

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is EnrollmentDto enr)) return;

            // Load concerns for the picker
            List<ConcernDto> concerns;
            try
            {
                var resp = await ApiService.Instance.GetAsync<ListResponse<ConcernDto>>("/concerns/index.php");
                concerns = resp?.Data ?? new List<ConcernDto>();
            }
            catch (Exception ex) { SetError(ex.Message); return; }

            int? newConcernId = ShowConcernPicker(enr, concerns);
            if (newConcernId == null && enr.ConcernId == null) return; // no change

            await ApiService.Instance.PutAsync<object>("/enrollments/update.php", new
            {
                id         = enr.Id,
                concern_id = newConcernId,
            });
            await ReloadAsync();
        }

        // ----------------------------------------------------------------
        // Delete — remove from group
        // ----------------------------------------------------------------

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is EnrollmentDto enr)) return;
            await ApiService.Instance.DeleteAsync("/enrollments/delete.php", new { id = enr.Id });
            await ReloadAsync();
        }

        // ----------------------------------------------------------------
        // Concern picker (inline form — no separate file needed)
        // ----------------------------------------------------------------

        private static int? ShowConcernPicker(EnrollmentDto enr, List<ConcernDto> concerns)
        {
            using var f = new Form
            {
                Text            = $"Concern — {enr.Firstname} {enr.Lastname} / {enr.Groupname}",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false, MinimizeBox = false,
                StartPosition   = FormStartPosition.CenterParent,
                Size            = new Size(340, 140),
                Font            = new Font("Trebuchet MS", 9f),
                BackColor       = Color.White,
            };

            var lbl = new Label { Text = "Concern:", Left = 12, Top = 16, AutoSize = true };
            var cbo = new ComboBox
            {
                Left = 12, Top = 34, Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Trebuchet MS", 9f),
            };
            cbo.Items.Add(new ConcernDto { Id = 0, Concern = "None" });
            foreach (var c in concerns) cbo.Items.Add(c);
            cbo.DisplayMember = "Concern";
            cbo.SelectedIndex = 0;
            if (enr.ConcernId.HasValue && enr.ConcernId.Value > 0)
                foreach (var item in cbo.Items)
                    if (item is ConcernDto c && c.Id == enr.ConcernId.Value)
                    { cbo.SelectedItem = c; break; }

            var btnOk = new Button
            {
                Text = "Save", Left = 148, Top = 72, Width = 80, Height = 28,
                BackColor = Color.FromArgb(0, 110, 0), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK,
            };
            btnOk.FlatAppearance.BorderSize = 0;
            var btnCancel = new Button
            {
                Text = "Cancel", Left = 234, Top = 72, Width = 80, Height = 28,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel,
            };

            f.Controls.AddRange(new Control[] { lbl, cbo, btnOk, btnCancel });
            f.AcceptButton = btnOk;
            f.CancelButton = btnCancel;

            if (f.ShowDialog() != DialogResult.OK) return enr.ConcernId; // unchanged sentinel

            if (cbo.SelectedItem is ConcernDto chosen && chosen.Id > 0) return chosen.Id;
            return null;
        }
    }
}
