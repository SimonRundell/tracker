/**
 * UnitsPanel — admin CRUD for units.
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
    public partial class UnitsPanel : AdminPanelBase
    {
        private TextBox _filterCode, _filterName;

        public UnitsPanel() { InitializeComponent(); }

        protected override Control BuildFilterBar()
        {
            var bar = MakeFilterPanel();
            _filterCode = AddFilterBox(bar, "Code", 52, 110);
            _filterName = AddFilterBox(bar, "Name", 170, 240);
            AddClearButton(bar, 418, _filterCode, _filterName);
            return bar;
        }

        protected override bool RowMatchesFilter(DataGridViewRow row)
            => CellContains(row, 1, _filterCode?.Text)
            && CellContains(row, 2, _filterName?.Text);

        protected override void DefineColumns()
        {
            AddColText("ID",       "id",        0.4f);
            AddColText("Code",     "unitcode",  0.8f);
            AddColText("Name",     "unitname",  2.5f);
            AddColText("Credits",  "credits",   0.5f);
            AddColText("GLH",      "glh",       0.5f);
            AddColText("External", "external",  0.6f);
            HideIdColumn();
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<UnitAdminDto>>("/units/index.php");
            foreach (var u in resp?.Data ?? new List<UnitAdminDto>())
            {
                var row = AddRow(u.Id, u.Unitcode, u.Unitname, u.Credits, u.Glh,
                    u.IsExternal != 0 ? "Yes" : "No");
                row.Tag = u;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new UnitEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/units/create.php", dlg.ToPayload());
            await ReloadAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is UnitAdminDto u)) return;
            using var dlg = new UnitEditDialog(u);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>("/units/update.php", dlg.ToPayload(u.Id));
            await ReloadAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is UnitAdminDto u)) return;
            await ApiService.Instance.DeleteAsync("/units/delete.php", new { id = u.Id });
            await ReloadAsync();
        }
    }

}
