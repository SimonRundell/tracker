/**
 * UserEditDialog — add/edit a staff user account.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    internal partial class UserEditDialog : Form
    {
        private static readonly string[] Roles = { "staff", "admin" };

        public UserEditDialog() { InitializeComponent(); }

        public UserEditDialog(UserAdminDto u) : this()
        {
            foreach (string r in Roles) _cboRole.Items.Add(r);
            _cboRole.SelectedItem = "staff";

            if (u != null)
            {
                Text            = "Edit User";
                _txtName.Text   = u.Fullname;
                _txtEmail.Text  = u.Email;
                _cboRole.SelectedItem = u.Role ?? "staff";
                _lblPassword.Text = "Password (leave blank to keep)";
            }
        }

        public object ToPayload(int id = 0)
        {
            var p = new Dictionary<string, object>
            {
                { "fullname", _txtName.Text.Trim()  },
                { "email",    _txtEmail.Text.Trim() },
                { "role",     _cboRole.SelectedItem?.ToString() ?? "staff" },
            };
            if (id > 0) p["id"] = id;
            if (!string.IsNullOrWhiteSpace(_txtPassword.Text))
                p["password"] = _txtPassword.Text;
            return p;
        }
    }
}
