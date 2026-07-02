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
        private bool _loggingOut = false;

        public MainForm()
        {
            InitializeComponent();
            if (Program.AppIcon != null) Icon = Program.AppIcon;
            _lblUser.Text = $"Logged in as: {ApiService.Instance.CurrentUser?.Fullname}";
            _tabs.SelectedIndexChanged += async (s, e) =>
            {
                if (_tabs.SelectedIndex == 0)
                    await _dashboardPanel.RefreshAsync();
            };
            FormClosing += OnMainFormClosing;
        }

        private void OnDashboard(object sender, EventArgs e)  => _tabs.SelectedIndex = 0;
        private void OnAdmin(object sender, EventArgs e)      => _tabs.SelectedIndex = 1;
        private void OnReports(object sender, EventArgs e)    => _tabs.SelectedIndex = 2;
        private void OnChangePassword(object sender, EventArgs e) => new ChangePasswordForm().ShowDialog(this);
        private void OnExit(object sender, EventArgs e) => Close();

        private void OnLogout(object sender, EventArgs e)
        {
            _loggingOut = true;
            ApiService.Instance.Logout();
            var login = new LoginForm();
            login.Show();
            Close();
        }

        private void OnAbout(object sender, EventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            MessageBox.Show(this,
                $"AtRisk Tracker\nVersion {version}\n\n" +
                "© 2026 Exeter College\n" +
                "Released under the Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0) license.",
                "About AtRisk Tracker",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // Confirm before actually leaving the application; skip when the close
        // is part of a Logout (which re-shows the login form, not a real exit).
        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_loggingOut) return;
            if (e.CloseReason != CloseReason.UserClosing) return;

            var result = MessageBox.Show(this,
                "Do you really want to exit AtRisk Tracker?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                e.Cancel = true;
        }
    }
}
