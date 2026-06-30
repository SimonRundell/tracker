/**
 * CourseEditDialog — add/edit a course.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    internal partial class CourseEditDialog : Form
    {
        public CourseEditDialog() { InitializeComponent(); }

        public CourseEditDialog(CourseDto c, List<QualTypeDto> qualTypes) : this()
        {
            _cbqQualType.Items.Add("-- select --");
            foreach (var qt in qualTypes) _cbqQualType.Items.Add(qt);
            _cbqQualType.SelectedIndex = 0;

            if (c != null)
            {
                Text          = "Edit Course";
                _txtName.Text = c.Coursename;
                _numPass.Value  = c.PassPoints        ?? 0;
                _numMerit.Value = c.MeritPoints       ?? 0;
                _numDist.Value  = c.DistinctionPoints ?? 0;
                foreach (object item in _cbqQualType.Items)
                    if (item is QualTypeDto q && q.Slug == c.QualType) { _cbqQualType.SelectedItem = q; break; }
            }
        }

        public object ToPayload(int id = 0)
        {
            string qt = (_cbqQualType.SelectedItem is QualTypeDto q) ? q.Slug : "";
            return new
            {
                id,
                coursename         = _txtName.Text.Trim(),
                qual_type          = qt,
                pass_points        = (int)_numPass.Value,
                merit_points       = (int)_numMerit.Value,
                distinction_points = (int)_numDist.Value,
            };
        }
    }
}
