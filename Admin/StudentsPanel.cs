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
        private TextBox _filterCis, _filterFirst, _filterLast;

        public StudentsPanel() { InitializeComponent(); }

        protected override Control BuildFilterBar()
        {
            var bar = MakeFilterPanel();
            _filterCis   = AddFilterBox(bar, "CIS No",   52,  90);
            _filterFirst = AddFilterBox(bar, "Forename", 150, 130);
            _filterLast  = AddFilterBox(bar, "Surname",  288, 130);
            AddClearButton(bar, 426, _filterCis, _filterFirst, _filterLast);
            return bar;
        }

        protected override bool RowMatchesFilter(DataGridViewRow row)
            => CellContains(row, 1, _filterCis?.Text)
            && CellContains(row, 2, _filterFirst?.Text)
            && CellContains(row, 3, _filterLast?.Text);

        protected override void DefineColumns()
        {
            AddColText("ID",       "id",        0.5f);
            AddColText("CIS No",   "cisnumber",  1f);
            AddColText("Forename", "firstname",  1.5f);
            AddColText("Surname",  "lastname",   1.5f);
            AddColText("Concern",  "concern",    1f);
            HideIdColumn();
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
            await ReloadAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is StudentAdminDto s)) return;
            using var dlg = new StudentEditDialog(s);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

            await ApiService.Instance.PutAsync<object>("/students/update.php", dlg.ToPayload(s.Id));

            // Update concern on the enrollment row if the student has one
            if (s.SgId.HasValue)
            {
                await ApiService.Instance.PutAsync<object>("/enrollments/update.php", new
                {
                    id         = s.SgId.Value,
                    concern_id = dlg.SelectedConcernId,
                });
            }

            await ReloadAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is StudentAdminDto s)) return;
            await ApiService.Instance.DeleteAsync("/students/delete.php", new { id = s.Id });
            await ReloadAsync();
        }
    }

}
