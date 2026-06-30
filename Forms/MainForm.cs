/**
 * MainForm — application shell with tab pages for Dashboard, Admin and Reports.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Windows.Forms;
using AtRiskTracker.Services;

namespace AtRiskTracker.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            _lblUser.Text = $"Logged in as: {ApiService.Instance.CurrentUser?.Fullname}";
        }

        private void OnDashboard(object sender, EventArgs e)  => _tabs.SelectedIndex = 0;
        private void OnAdmin(object sender, EventArgs e)      => _tabs.SelectedIndex = 1;
        private void OnReports(object sender, EventArgs e)    => _tabs.SelectedIndex = 2;
        private void OnChangePassword(object sender, EventArgs e) => new ChangePasswordForm().ShowDialog(this);
        private void OnExit(object sender, EventArgs e) => Close();

        private void OnLogout(object sender, EventArgs e)
        {
            ApiService.Instance.Logout();
            var login = new LoginForm();
            login.Show();
            Close();
        }
    }
}
