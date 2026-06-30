/**
 * QualTypesPanel — admin CRUD for qualification types.
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
    public partial class QualTypesPanel : AdminPanelBase
    {
        public QualTypesPanel() { InitializeComponent(); }

        protected override void DefineColumns()
        {
            AddColText("ID",          "id",      0.4f);
            AddColText("Name",        "name",    2f);
            AddColText("Slug",        "slug",    1.5f);
            AddColText("Show Predict","predict", 0.7f);
            AddColText("Is NCFE",     "ncfe",    0.7f);
            AddColText("BTec Grades", "btec",    0.7f);
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<QualTypesResponse>("/qualtypes/index.php");
            foreach (var q in resp?.Data ?? new List<QualTypeDto>())
            {
                var row = AddRow(q.Id, q.Name, q.Slug,
                    q.ShowPredict != 0 ? "Yes" : "No",
                    q.IsNcfe != 0 ? "Yes" : "No",
                    q.BtecOverallGrades != 0 ? "Yes" : "No");
                row.Tag = q;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new QualTypeEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/qualtypes/index.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is QualTypeDto q)) return;
            using var dlg = new QualTypeEditDialog(q);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>($"/qualtypes/index.php?id={q.Id}", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is QualTypeDto q)) return;
            await ApiService.Instance.DeleteAsync($"/qualtypes/index.php?id={q.Id}");
            await LoadDataAsync();
        }
    }

}
