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
    public class GroupsPanel : AdminPanelBase
    {
        protected override void DefineColumns()
        {
            AddColText("ID",         "id",         0.4f);
            AddColText("Group Name", "groupname",  2f);
            AddColText("Course",     "course",     2f);
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
            await ApiService.Instance.PostAsync<object>("/groups/index.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is GroupDto g)) return;
            var courses = await LoadCoursesAsync();
            using var dlg = new GroupEditDialog(g, courses);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>($"/groups/index.php?id={g.Id}", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is GroupDto g)) return;
            await ApiService.Instance.DeleteAsync($"/groups/index.php?id={g.Id}");
            await LoadDataAsync();
        }

        private async Task<List<CourseDto>> LoadCoursesAsync()
        {
            try { var r = await ApiService.Instance.GetAsync<CoursesResponse>("/courses/index.php"); return r?.Data ?? new List<CourseDto>(); }
            catch { return new List<CourseDto>(); }
        }
    }

    internal class GroupEditDialog : Form
    {
        private TextBox  _txtName;
        private ComboBox _cboCourse;

        public GroupEditDialog(GroupDto g, List<CourseDto> courses)
        {
            Text            = g == null ? "Add Group" : "Edit Group";
            Size            = new Size(380, 200);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);

            Controls.Add(new Label { Text="Group Name", Bounds=new Rectangle(20,15,320,18) });
            _txtName = new TextBox { Text=g?.Groupname??"", Bounds=new Rectangle(20,33,320,24) }; Controls.Add(_txtName);

            Controls.Add(new Label { Text="Course", Bounds=new Rectangle(20,65,320,18) });
            _cboCourse = new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList, Bounds=new Rectangle(20,83,320,24) };
            _cboCourse.Items.Add("-- select --");
            foreach (var c in courses) _cboCourse.Items.Add(c);
            _cboCourse.SelectedIndex = 0;
            if (g != null)
                foreach (CourseDto c in _cboCourse.Items)
                    if (c is CourseDto cd && cd.Id == g.CourseId) { _cboCourse.SelectedItem=cd; break; }
            Controls.Add(_cboCourse);

            var ok = new Button { Text="OK",Bounds=new Rectangle(20,118,150,28),BackColor=Color.FromArgb(0,70,127),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.OK };
            ok.FlatAppearance.BorderSize=0;
            var cancel = new Button { Text="Cancel",Bounds=new Rectangle(185,118,150,28),FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.Cancel };
            Controls.AddRange(new Control[]{ok,cancel}); AcceptButton=ok; CancelButton=cancel;
        }

        public object ToPayload() => new
        {
            groupname = _txtName.Text.Trim(),
            course_id = (_cboCourse.SelectedItem is CourseDto c) ? c.Id : 0,
        };
    }
}
