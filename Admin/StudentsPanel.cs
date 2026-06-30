/**
 * StudentsPanel — admin CRUD for students.
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
    public class StudentsPanel : AdminPanelBase
    {
        protected override void DefineColumns()
        {
            AddColText("ID",        "id",        0.5f);
            AddColText("CIS No",    "cisnumber",  1f);
            AddColText("Forename",  "firstname",  1.5f);
            AddColText("Surname",   "lastname",   1.5f);
            AddColText("Email",     "email",      2f);
            AddColText("Concern",   "concern",    1f);
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<StudentAdminDto>>(
                "/students/index.php");
            foreach (var s in resp?.Data ?? new List<StudentAdminDto>())
            {
                var row = AddRow(s.Id, s.Cisnumber, s.Firstname, s.Lastname, s.Email, s.Concern ?? "None");
                row.Tag = s;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new StudentEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/students/index.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is StudentAdminDto s)) return;
            using var dlg = new StudentEditDialog(s);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>($"/students/index.php?id={s.Id}", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is StudentAdminDto s)) return;
            await ApiService.Instance.DeleteAsync($"/students/index.php?id={s.Id}");
            await LoadDataAsync();
        }
    }

    internal class StudentEditDialog : Form
    {
        private TextBox _txtCis, _txtFirst, _txtLast, _txtEmail;

        public StudentEditDialog(StudentAdminDto s)
        {
            Text            = s == null ? "Add Student" : "Edit Student";
            Size            = new Size(400, 300);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);

            int y = 20;
            AddField("CIS Number",  ref _txtCis,   ref y, s?.Cisnumber);
            AddField("Forename",    ref _txtFirst,  ref y, s?.Firstname);
            AddField("Surname",     ref _txtLast,   ref y, s?.Lastname);
            AddField("Email",       ref _txtEmail,  ref y, s?.Email);

            var btnOk = new Button { Text = "OK", Bounds = new Rectangle(20, y, 150, 30),
                BackColor = Color.FromArgb(0, 70, 127), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };
            btnOk.FlatAppearance.BorderSize = 0;
            var btnCancel = new Button { Text = "Cancel", Bounds = new Rectangle(185, y, 150, 30),
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };
            Controls.AddRange(new Control[] { btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void AddField(string label, ref TextBox txt, ref int y, string value)
        {
            Controls.Add(new Label { Text = label, Bounds = new Rectangle(20, y, 340, 18) });
            y += 20;
            txt = new TextBox { Text = value ?? "", Bounds = new Rectangle(20, y, 340, 24) };
            Controls.Add(txt);
            y += 32;
        }

        public object ToPayload() => new
        {
            cisnumber = _txtCis.Text.Trim(),
            firstname = _txtFirst.Text.Trim(),
            lastname  = _txtLast.Text.Trim(),
            email     = _txtEmail.Text.Trim(),
        };
    }
}
