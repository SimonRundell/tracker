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
    public class UsersPanel : AdminPanelBase
    {
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
            await ApiService.Instance.PostAsync<object>("/users/index.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is UserAdminDto u)) return;
            using var dlg = new UserEditDialog(u);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>($"/users/index.php?id={u.Id}", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is UserAdminDto u)) return;
            await ApiService.Instance.DeleteAsync($"/users/index.php?id={u.Id}");
            await LoadDataAsync();
        }
    }

    internal class UserEditDialog : Form
    {
        private TextBox  _txtName, _txtEmail, _txtPassword;
        private ComboBox _cboRole;

        private static readonly string[] Roles = { "staff", "admin" };

        public UserEditDialog(UserAdminDto u)
        {
            Text            = u == null ? "Add User" : "Edit User";
            Size            = new Size(380, 300);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);

            int y = 15;
            Field("Full Name", ref _txtName,    ref y, u?.Fullname);
            Field("Email",     ref _txtEmail,   ref y, u?.Email);

            Controls.Add(new Label { Text="Password" + (u!=null?" (leave blank to keep)":""), Bounds=new Rectangle(20,y,320,18) }); y+=18;
            _txtPassword = new TextBox { Bounds=new Rectangle(20,y,320,24), UseSystemPasswordChar=true }; Controls.Add(_txtPassword); y+=32;

            Controls.Add(new Label { Text="Role", Bounds=new Rectangle(20,y,320,18) }); y+=18;
            _cboRole = new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList, Bounds=new Rectangle(20,y,320,24) };
            foreach (string r in Roles) _cboRole.Items.Add(r);
            _cboRole.SelectedItem = u?.Role ?? "staff";
            Controls.Add(_cboRole); y+=32;

            var ok=new Button{Text="OK",Bounds=new Rectangle(20,y,155,28),BackColor=Color.FromArgb(0,70,127),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.OK};ok.FlatAppearance.BorderSize=0;
            var cancel=new Button{Text="Cancel",Bounds=new Rectangle(190,y,155,28),FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.Cancel};
            Controls.AddRange(new Control[]{ok,cancel}); AcceptButton=ok; CancelButton=cancel;
        }

        private void Field(string lbl, ref TextBox txt, ref int y, string val)
        {
            Controls.Add(new Label { Text=lbl, Bounds=new Rectangle(20,y,320,18) }); y+=18;
            txt = new TextBox { Text=val??"", Bounds=new Rectangle(20,y,320,24) }; Controls.Add(txt); y+=32;
        }

        public object ToPayload()
        {
            var p = new System.Collections.Generic.Dictionary<string, object>
            {
                { "fullname", _txtName.Text.Trim()  },
                { "email",    _txtEmail.Text.Trim()  },
                { "role",     _cboRole.SelectedItem?.ToString() ?? "staff" },
            };
            if (!string.IsNullOrWhiteSpace(_txtPassword.Text))
                p["password"] = _txtPassword.Text;
            return p;
        }
    }
}
