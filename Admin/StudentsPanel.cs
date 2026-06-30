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
    public partial class StudentsPanel : AdminPanelBase
    {
        public StudentsPanel() { InitializeComponent(); }

        protected override void DefineColumns()
        {
            AddColText("ID",       "id",        0.5f);
            AddColText("CIS No",   "cisnumber",  1f);
            AddColText("Forename", "firstname",  1.5f);
            AddColText("Surname",  "lastname",   1.5f);
            AddColText("Concern",  "concern",    1f);
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<StudentAdminDto>>(
                "/students/index.php");
            foreach (var s in resp?.Data ?? new List<StudentAdminDto>())
            {
                var row = AddRow(s.Id, s.Cisnumber, s.Firstname, s.Lastname, s.Concern ?? "None");
                row.Tag = s;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new StudentEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/students/create.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is StudentAdminDto s)) return;
            using var dlg = new StudentEditDialog(s);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>("/students/update.php", dlg.ToPayload(s.Id));
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is StudentAdminDto s)) return;
            await ApiService.Instance.DeleteAsync("/students/delete.php", new { id = s.Id });
            await LoadDataAsync();
        }
    }

}
