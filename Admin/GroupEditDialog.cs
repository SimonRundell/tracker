/**
 * GroupEditDialog — add/edit a teaching group.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    internal partial class GroupEditDialog : Form
    {
        public GroupEditDialog() { InitializeComponent(); }

        public GroupEditDialog(GroupDto g, List<CourseDto> courses) : this()
        {
            _cboCourse.Items.Add("-- select --");
            foreach (var c in courses) _cboCourse.Items.Add(c);
            _cboCourse.SelectedIndex = 0;

            if (g != null)
            {
                Text          = "Edit Group";
                _txtName.Text = g.Groupname;
                foreach (object item in _cboCourse.Items)
                    if (item is CourseDto cd && cd.Id == g.CourseId) { _cboCourse.SelectedItem = cd; break; }
            }
        }

        public object ToPayload(int id = 0) => new
        {
            id,
            groupname = _txtName.Text.Trim(),
            course_id = (_cboCourse.SelectedItem is CourseDto c) ? c.Id : 0,
        };
    }
}
