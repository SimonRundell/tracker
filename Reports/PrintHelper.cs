/**
 * PrintHelper — opens a temporary WebBrowser form pre-loaded with the grade
 * grid print report and triggers the system print dialog.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Reports
{
    public static class PrintHelper
    {
        /// <summary>
        /// Builds and opens a printable grade grid report for the given course/group.
        /// </summary>
        public static Task PrintReportAsync(CourseDto course, string groupName, GridDataDto data)
        {
            var group = new GroupDto { Groupname = groupName };
            string html = ReportBuilder.Build("At-Risk Summary", course, group, data);

            var frm = new Form
            {
                Text            = "Print Preview — " + groupName,
                Size            = new Size(900, 700),
                StartPosition   = FormStartPosition.CenterParent,
                Font            = new Font("Trebuchet MS", 9f),
            };

            var browser = new WebBrowser
            {
                Dock                  = DockStyle.Fill,
                IsWebBrowserContextMenuEnabled = true,
            };

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(240, 244, 248) };
            var btnPrint = new Button
            {
                Text = "Print / Save PDF", Left = 8, Top = 7, Width = 140, Height = 28,
                BackColor = Color.FromArgb(0, 70, 127), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (s, e) => browser.Print();
            toolbar.Controls.Add(btnPrint);

            frm.Controls.Add(browser);
            frm.Controls.Add(toolbar);
            frm.Show();

            browser.DocumentText = html;
            return Task.CompletedTask;
        }
    }
}
