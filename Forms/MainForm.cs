/**
 * MainForm — application shell with tab pages for Dashboard, Admin and Reports.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using AtRiskTracker.Services;
using AtRiskTracker.Forms.Dashboard;
using AtRiskTracker.Admin;
using AtRiskTracker.Reports;

namespace AtRiskTracker.Forms
{
    public class MainForm : Form
    {
        private TabControl  _tabs;
        private StatusStrip _status;
        private ToolStripStatusLabel _lblUser;
        private ToolStripStatusLabel _lblVersion;

        private DashboardPanel  _dashboardPanel;
        private AdminTabPanel   _adminPanel;
        private ReportsPanel    _reportsPanel;

        public MainForm()
        {
            BuildUi();
        }

        private void BuildUi()
        {
            Text            = "AtRisk Tracker";
            Size            = new Size(1280, 800);
            MinimumSize     = new Size(900, 600);
            StartPosition   = FormStartPosition.CenterScreen;
            Font            = new Font("Trebuchet MS", 9f);

            // Menu
            var menu = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            var changePwdItem = new ToolStripMenuItem("Change Password...");
            changePwdItem.Click += (s, e) => new ChangePasswordForm().ShowDialog(this);
            var logoutItem = new ToolStripMenuItem("Logout");
            logoutItem.Click += OnLogout;
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => Close();
            fileMenu.DropDownItems.AddRange(new ToolStripItem[]
                { changePwdItem, new ToolStripSeparator(), logoutItem, new ToolStripSeparator(), exitItem });
            menu.Items.Add(fileMenu);
            MainMenuStrip = menu;
            Controls.Add(menu);

            // Status bar
            _status   = new StatusStrip();
            _lblUser  = new ToolStripStatusLabel($"Logged in as: {ApiService.Instance.CurrentUser?.Fullname}");
            _lblVersion = new ToolStripStatusLabel("© 2026 Exeter College — CC NC-BY-SA 4.0")
            {
                Spring    = true,
                TextAlign = ContentAlignment.MiddleRight,
            };
            _status.Items.AddRange(new ToolStripItem[] { _lblUser, _lblVersion });
            Controls.Add(_status);

            // Tabs
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Trebuchet MS", 9f, FontStyle.Bold),
            };

            var pageDash    = new TabPage("Dashboard");
            var pageAdmin   = new TabPage("Admin");
            var pageReports = new TabPage("Reports");

            _dashboardPanel = new DashboardPanel { Dock = DockStyle.Fill };
            _adminPanel     = new AdminTabPanel  { Dock = DockStyle.Fill };
            _reportsPanel   = new ReportsPanel   { Dock = DockStyle.Fill };

            pageDash.Controls.Add(_dashboardPanel);
            pageAdmin.Controls.Add(_adminPanel);
            pageReports.Controls.Add(_reportsPanel);

            _tabs.TabPages.AddRange(new[] { pageDash, pageAdmin, pageReports });
            Controls.Add(_tabs);
        }

        private void OnLogout(object sender, EventArgs e)
        {
            ApiService.Instance.Logout();
            var login = new LoginForm();
            login.Show();
            Close();
        }
    }
}
