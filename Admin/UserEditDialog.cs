/**
 * UserEditDialog — add/edit a staff user account.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    internal partial class UserEditDialog : Form
    {
        private static readonly string[] Roles = { "staff", "admin" };

        public UserEditDialog()
        {
            InitializeComponent();
            // Populate role items here so every constructor path has them
            foreach (string r in Roles) _cboRole.Items.Add(r);
            _cboRole.SelectedIndex = 0;
        }

        public UserEditDialog(UserAdminDto u) : this()
        {
            if (u != null)
            {
                Text              = "Edit User";
                _txtName.Text     = u.Fullname;
                _txtEmail.Text    = u.Email;
                _lblPassword.Text = "Password (leave blank to keep)";
                int ri = Array.IndexOf(Roles, u.Role ?? "staff");
                _cboRole.SelectedIndex = ri >= 0 ? ri : 0;
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
