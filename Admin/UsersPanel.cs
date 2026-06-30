/**
 * UsersPanel — admin CRUD for staff user accounts.
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
    public partial class UsersPanel : AdminPanelBase
    {
        public UsersPanel() { InitializeComponent(); }

        protected override void DefineColumns()
        {
            AddColText("ID",       "id",       0.4f);
            AddColText("Name",     "fullname", 2f);
            AddColText("Email",    "email",    2.5f);
            AddColText("Role",     "role",     0.8f);
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<UserAdminDto>>("/users/index.php");
            foreach (var u in resp?.Data ?? new List<UserAdminDto>())
            {
                var row = AddRow(u.Id, u.Fullname, u.Email, u.Role);
                row.Tag = u;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new UserEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/users/create.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is UserAdminDto u)) return;
            using var dlg = new UserEditDialog(u);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>("/users/update.php", dlg.ToPayload(u.Id));
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is UserAdminDto u)) return;
            await ApiService.Instance.DeleteAsync("/users/delete.php", new { id = u.Id });
            await LoadDataAsync();
        }
    }

}
