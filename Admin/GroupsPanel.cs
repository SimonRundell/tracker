/**
 * GroupsPanel — admin CRUD for teaching groups.
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
    public partial class GroupsPanel : AdminPanelBase
    {
        private TextBox _filterGroup, _filterCourse;

        public GroupsPanel() { InitializeComponent(); }

        protected override Control BuildFilterBar()
        {
            var bar = MakeFilterPanel();
            _filterGroup  = AddFilterBox(bar, "Group Name", 52, 170);
            _filterCourse = AddFilterBox(bar, "Course",    230, 170);
            AddClearButton(bar, 408, _filterGroup, _filterCourse);
            return bar;
        }

        protected override bool RowMatchesFilter(DataGridViewRow row)
            => CellContains(row, 1, _filterGroup?.Text)
            && CellContains(row, 2, _filterCourse?.Text);

        protected override void DefineColumns()
        {
            AddColText("ID",         "id",         0.4f);
            AddColText("Group Name", "groupname",  2f);
            AddColText("Course",     "course",     2f);
            HideIdColumn();
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<GroupsResponse>("/groups/index.php");
            // Also fetch courses for display
            var cr = await ApiService.Instance.GetAsync<CoursesResponse>("/courses/index.php");
            var courseMap = new Dictionary<int, string>();
            foreach (var c in cr?.Data ?? new List<CourseDto>())
                courseMap[c.Id] = c.Coursename;

            foreach (var g in resp?.Data ?? new List<GroupDto>())
            {
                string cname = courseMap.TryGetValue(g.CourseId, out string cn) ? cn : "-";
                var row = AddRow(g.Id, g.Groupname, cname);
                row.Tag = g;
            }
        }

        protected override async Task AddItemAsync()
        {
            var courses = await LoadCoursesAsync();
            using var dlg = new GroupEditDialog(null, courses);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/groups/create.php", dlg.ToPayload());
            await ReloadAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is GroupDto g)) return;
            var courses = await LoadCoursesAsync();
            using var dlg = new GroupEditDialog(g, courses);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>("/groups/update.php", dlg.ToPayload(g.Id));
            await ReloadAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is GroupDto g)) return;
            await ApiService.Instance.DeleteAsync("/groups/delete.php", new { id = g.Id });
            await ReloadAsync();
        }

        private async Task<List<CourseDto>> LoadCoursesAsync()
        {
            try { var r = await ApiService.Instance.GetAsync<CoursesResponse>("/courses/index.php"); return r?.Data ?? new List<CourseDto>(); }
            catch { return new List<CourseDto>(); }
        }
    }

}
