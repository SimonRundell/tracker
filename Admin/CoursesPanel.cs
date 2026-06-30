/**
 * CoursesPanel — admin CRUD for courses.
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
    public class CoursesPanel : AdminPanelBase
    {
        protected override void DefineColumns()
        {
            AddColText("ID",          "id",          0.4f);
            AddColText("Course Name", "coursename",  2f);
            AddColText("Qual Type",   "qual_type",   1f);
            AddColText("Pass Pts",    "pass_pts",    0.6f);
            AddColText("Merit Pts",   "merit_pts",   0.6f);
            AddColText("Dist Pts",    "dist_pts",    0.6f);
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<CoursesResponse>("/courses/index.php");
            foreach (var c in resp?.Data ?? new List<CourseDto>())
            {
                var row = AddRow(c.Id, c.Coursename, c.QualType,
                    c.PassPoints, c.MeritPoints, c.DistinctionPoints);
                row.Tag = c;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new CourseEditDialog(null, await LoadQualTypesAsync());
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/courses/index.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is CourseDto c)) return;
            using var dlg = new CourseEditDialog(c, await LoadQualTypesAsync());
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>($"/courses/index.php?id={c.Id}", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is CourseDto c)) return;
            await ApiService.Instance.DeleteAsync($"/courses/index.php?id={c.Id}");
            await LoadDataAsync();
        }

        private async Task<List<QualTypeDto>> LoadQualTypesAsync()
        {
            try
            {
                var r = await ApiService.Instance.GetAsync<QualTypesResponse>("/qualtypes/index.php");
                return r?.Data ?? new List<QualTypeDto>();
            }
            catch { return new List<QualTypeDto>(); }
        }
    }

    internal class CourseEditDialog : Form
    {
        private TextBox      _txtName;
        private ComboBox     _cbqQualType;
        private NumericUpDown _numPass, _numMerit, _numDist;
        private readonly List<QualTypeDto> _qualTypes;

        public CourseEditDialog(CourseDto c, List<QualTypeDto> qualTypes)
        {
            _qualTypes = qualTypes;
            Text            = c == null ? "Add Course" : "Edit Course";
            Size            = new Size(420, 360);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);

            int y = 15;
            Controls.Add(new Label { Text = "Course Name", Bounds = new Rectangle(20, y, 360, 18) }); y += 20;
            _txtName = new TextBox { Text = c?.Coursename ?? "", Bounds = new Rectangle(20, y, 360, 24) };
            Controls.Add(_txtName); y += 32;

            Controls.Add(new Label { Text = "Qual Type", Bounds = new Rectangle(20, y, 360, 18) }); y += 20;
            _cbqQualType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Bounds = new Rectangle(20, y, 360, 24),
            };
            _cbqQualType.Items.Add("-- select --");
            foreach (var qt in qualTypes) _cbqQualType.Items.Add(qt);
            _cbqQualType.SelectedIndex = 0;
            if (c?.QualType != null)
                foreach (QualTypeDto qt in _cbqQualType.Items)
                    if (qt is QualTypeDto q && q.Slug == c.QualType)
                    { _cbqQualType.SelectedItem = q; break; }
            Controls.Add(_cbqQualType); y += 32;

            void AddNum(string lbl, ref NumericUpDown num, int? val)
            {
                Controls.Add(new Label { Text = lbl, Bounds = new Rectangle(20, y, 160, 18) });
                num = new NumericUpDown
                {
                    Bounds = new Rectangle(20, y + 18, 160, 24),
                    Minimum = 0, Maximum = 9999, Value = val ?? 0,
                };
                Controls.Add(num);
            }
            AddNum("Pass Points",        ref _numPass,  c?.PassPoints);
            _numPass.Left  = 20; _numMerit = null; _numDist = null;
            // Reposition numerics side-by-side
            _numPass.Bounds = new Rectangle(20, y + 18, 110, 24);
            Controls.Add(new Label { Text = "Pass Pts", Bounds = new Rectangle(20, y, 110, 18) });

            var numMerit = new NumericUpDown { Bounds = new Rectangle(145, y+18, 110, 24), Minimum=0,Maximum=9999, Value= c?.MeritPoints??0 };
            Controls.Add(new Label { Text = "Merit Pts", Bounds = new Rectangle(145, y, 110, 18) });
            Controls.Add(numMerit);

            var numDist = new NumericUpDown { Bounds = new Rectangle(270, y+18, 110, 24), Minimum=0,Maximum=9999, Value= c?.DistinctionPoints??0 };
            Controls.Add(new Label { Text = "Dist Pts", Bounds = new Rectangle(270, y, 110, 18) });
            Controls.Add(numDist);

            _numMerit = numMerit; _numDist = numDist;
            y += 55;

            var btnOk = new Button { Text = "OK", Bounds = new Rectangle(20, y, 170, 30),
                BackColor = Color.FromArgb(0, 70, 127), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };
            btnOk.FlatAppearance.BorderSize = 0;
            var btnCancel = new Button { Text = "Cancel", Bounds = new Rectangle(205, y, 170, 30),
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };
            Controls.AddRange(new Control[] { btnOk, btnCancel });
            AcceptButton = btnOk; CancelButton = btnCancel;
        }

        public object ToPayload()
        {
            string qt = (_cbqQualType.SelectedItem is QualTypeDto q) ? q.Slug : "";
            return new
            {
                coursename        = _txtName.Text.Trim(),
                qual_type         = qt,
                pass_points       = (int)_numPass.Value,
                merit_points      = (int)_numMerit.Value,
                distinction_points= (int)_numDist.Value,
            };
        }
    }
}
