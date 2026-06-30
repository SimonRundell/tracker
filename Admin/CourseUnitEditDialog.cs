/**
 * CourseUnitEditDialog — add/edit a course-unit assignment.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    internal partial class CourseUnitEditDialog : Form
    {
        public CourseUnitEditDialog() { InitializeComponent(); }

        public CourseUnitEditDialog(CourseUnitDto cu, List<CourseDto> courses, List<UnitAdminDto> units) : this()
        {
            _cboCourse.Items.Add("-- select --");
            foreach (var c in courses) _cboCourse.Items.Add(c);
            _cboCourse.SelectedIndex = 0;

            _cboUnit.Items.Add("-- select --");
            foreach (var u in units) _cboUnit.Items.Add(u);
            _cboUnit.SelectedIndex = 0;

            if (cu != null)
            {
                Text           = "Edit Course Unit";
                _numYear.Value = cu.YearTaken ?? 1;
                foreach (object item in _cboCourse.Items)
                    if (item is CourseDto cd && cd.Id == cu.CourseId) { _cboCourse.SelectedItem = cd; break; }
                foreach (object item in _cboUnit.Items)
                    if (item is UnitAdminDto ud && ud.Id == cu.UnitId) { _cboUnit.SelectedItem = ud; break; }
            }
        }

        public object ToPayload() => new
        {
            course_id  = (_cboCourse.SelectedItem is CourseDto c)   ? c.Id : 0,
            unit_id    = (_cboUnit.SelectedItem   is UnitAdminDto u) ? u.Id : 0,
            year_taken = (int)_numYear.Value,
        };
    }
}
