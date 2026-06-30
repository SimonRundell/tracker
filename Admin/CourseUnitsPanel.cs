/**
 * CourseUnitsPanel — admin CRUD for course-unit assignments.
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
    public class CourseUnitsPanel : AdminPanelBase
    {
        protected override void DefineColumns()
        {
            AddColText("ID",         "id",         0.4f);
            AddColText("Course",     "course",     2f);
            AddColText("Unit Code",  "unitcode",   0.8f);
            AddColText("Unit Name",  "unitname",   2f);
            AddColText("Year Taken", "year_taken", 0.7f);
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<CourseUnitDto>>("/courseunits/index.php");
            foreach (var cu in resp?.Data ?? new List<CourseUnitDto>())
            {
                var row = AddRow(cu.Id, cu.Coursename, cu.Unitcode, cu.Unitname, cu.YearTaken ?? 1);
                row.Tag = cu;
            }
        }

        protected override async Task AddItemAsync()
        {
            var (courses, units) = await LoadCoursesAndUnitsAsync();
            using var dlg = new CourseUnitEditDialog(null, courses, units);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/courseunits/index.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is CourseUnitDto cu)) return;
            var (courses, units) = await LoadCoursesAndUnitsAsync();
            using var dlg = new CourseUnitEditDialog(cu, courses, units);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>($"/courseunits/index.php?id={cu.Id}", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is CourseUnitDto cu)) return;
            await ApiService.Instance.DeleteAsync($"/courseunits/index.php?id={cu.Id}");
            await LoadDataAsync();
        }

        private async Task<(List<CourseDto>, List<UnitAdminDto>)> LoadCoursesAndUnitsAsync()
        {
            List<CourseDto>     courses = new List<CourseDto>();
            List<UnitAdminDto>  units   = new List<UnitAdminDto>();
            try
            {
                var cr = await ApiService.Instance.GetAsync<CoursesResponse>("/courses/index.php");
                var ur = await ApiService.Instance.GetAsync<ListResponse<UnitAdminDto>>("/units/index.php");
                courses = cr?.Data ?? courses;
                units   = ur?.Data ?? units;
            }
            catch { }
            return (courses, units);
        }
    }

    internal class CourseUnitEditDialog : Form
    {
        private ComboBox      _cboCourse, _cboUnit;
        private NumericUpDown _numYear;

        public CourseUnitEditDialog(CourseUnitDto cu, List<CourseDto> courses, List<UnitAdminDto> units)
        {
            Text            = cu == null ? "Add Course Unit" : "Edit Course Unit";
            Size            = new Size(400, 240);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);

            Controls.Add(new Label { Text="Course", Bounds=new Rectangle(20,15,340,18) });
            _cboCourse = new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList, Bounds=new Rectangle(20,33,340,24) };
            _cboCourse.Items.Add("-- select --");
            foreach (var c in courses) _cboCourse.Items.Add(c);
            _cboCourse.SelectedIndex=0;
            if (cu!=null) foreach(CourseDto c in _cboCourse.Items) if(c is CourseDto cd&&cd.Id==cu.CourseId){_cboCourse.SelectedItem=cd;break;}
            Controls.Add(_cboCourse);

            Controls.Add(new Label { Text="Unit", Bounds=new Rectangle(20,63,340,18) });
            _cboUnit = new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList, Bounds=new Rectangle(20,81,340,24) };
            _cboUnit.Items.Add("-- select --");
            foreach (var u in units) _cboUnit.Items.Add(u);
            _cboUnit.SelectedIndex=0;
            if (cu!=null) foreach(UnitAdminDto u in _cboUnit.Items) if(u is UnitAdminDto ud&&ud.Id==cu.UnitId){_cboUnit.SelectedItem=ud;break;}
            Controls.Add(_cboUnit);

            Controls.Add(new Label { Text="Year Taken", Bounds=new Rectangle(20,111,340,18) });
            _numYear = new NumericUpDown { Bounds=new Rectangle(20,129,100,24),Minimum=1,Maximum=3,Value=cu?.YearTaken??1 }; Controls.Add(_numYear);

            var ok=new Button{Text="OK",Bounds=new Rectangle(20,163,160,28),BackColor=Color.FromArgb(0,70,127),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.OK};ok.FlatAppearance.BorderSize=0;
            var cancel=new Button{Text="Cancel",Bounds=new Rectangle(195,163,160,28),FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.Cancel};
            Controls.AddRange(new Control[]{ok,cancel}); AcceptButton=ok; CancelButton=cancel;
        }

        public object ToPayload() => new
        {
            course_id  = (_cboCourse.SelectedItem is CourseDto c)   ? c.Id  : 0,
            unit_id    = (_cboUnit.SelectedItem   is UnitAdminDto u) ? u.Id  : 0,
            year_taken = (int)_numYear.Value,
        };
    }
}
