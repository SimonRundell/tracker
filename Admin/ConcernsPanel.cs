/**
 * ConcernsPanel — admin CRUD for concern labels.
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
    public partial class ConcernsPanel : AdminPanelBase
    {
        public ConcernsPanel() { InitializeComponent(); }

        protected override void DefineColumns()
        {
            AddColText("ID",      "id",      0.4f);
            AddColText("Concern", "concern", 4f);
            HideIdColumn();
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<ConcernDto>>("/concerns/index.php");
            foreach (var c in resp?.Data ?? new List<ConcernDto>())
            {
                var row = AddRow(c.Id, c.Concern);
                row.Tag = c;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new ConcernEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/concerns/create.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is ConcernDto c)) return;
            using var dlg = new ConcernEditDialog(c);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>("/concerns/update.php", dlg.ToPayload(c.Id));
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is ConcernDto c)) return;
            await ApiService.Instance.DeleteAsync("/concerns/delete.php", new { id = c.Id });
            await LoadDataAsync();
        }
    }

}
