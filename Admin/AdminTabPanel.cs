/**
 * AdminTabPanel — tabbed admin interface.
 *
 * Tabs: Students | Courses | Qual Types | Groups | Units | Course Units |
 *       Objectives | Concerns | Users
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Drawing;
using System.Windows.Forms;

namespace AtRiskTracker.Admin
{
    public class AdminTabPanel : UserControl
    {
        public AdminTabPanel()
        {
            Dock = DockStyle.Fill;

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Trebuchet MS", 9f),
            };

            void AddTab(string title, UserControl panel)
            {
                var pg = new TabPage(title);
                panel.Dock = DockStyle.Fill;
                pg.Controls.Add(panel);
                tabs.TabPages.Add(pg);
            }

            AddTab("Students",     new StudentsPanel());
            AddTab("Courses",      new CoursesPanel());
            AddTab("Qual Types",   new QualTypesPanel());
            AddTab("Groups",       new GroupsPanel());
            AddTab("Units",        new UnitsPanel());
            AddTab("Course Units", new CourseUnitsPanel());
            AddTab("Objectives",   new ObjectivesPanel());
            AddTab("Concerns",     new ConcernsPanel());
            AddTab("Users",        new UsersPanel());

            Controls.Add(tabs);
        }
    }
}
