/**
 * CourseUnitsPanel — admin CRUD for course-unit assignments.
 *
 * The /courseunits/index.php endpoint requires a course_id filter, so we load
 * all courses first and then build the combined view per course.
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
using Newtonsoft.Json;

namespace AtRiskTracker.Admin
{
    public partial class CourseUnitsPanel : AdminPanelBase
    {
        public CourseUnitsPanel() { InitializeComponent(); }

        protected override void DefineColumns()
        {
            AddColText("ID",         "id",         0.4f);
            AddColText("Course",     "course",     2f);
            AddColText("Unit Code",  "unitcode",   0.8f);
            AddColText("Unit Name",  "unitname",   2f);
            AddColText("Year Taken", "year_taken", 0.7f);
            HideIdColumn();
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();

            // The endpoint requires course_id — fetch courses, then units per course
            var cr = await ApiService.Instance.GetAsync<CoursesResponse>("/courses/index.php");
            var courses = cr?.Data ?? new List<CourseDto>();

            foreach (var course in courses)
            {
                var resp = await ApiService.Instance.GetAsync<ListResponse<CourseUnitRowDto>>(
                    "/courseunits/index.php", $"course_id={course.Id}");

                foreach (var u in resp?.Data ?? new List<CourseUnitRowDto>())
                {
                    var cu = new CourseUnitDto
                    {
                        CourseId   = course.Id,
                        UnitId     = u.Id,
                        Coursename = course.Coursename,
                        Unitcode   = u.Unitcode,
                        Unitname   = u.Unitname,
                        YearTaken  = u.YearTaken,
                    };
                    var row = AddRow(0, cu.Coursename, cu.Unitcode, cu.Unitname, cu.YearTaken ?? 1);
                    row.Tag = cu;
                }
            }
        }

        protected override async Task AddItemAsync()
        {
            var (courses, units) = await LoadCoursesAndUnitsAsync();
            using var dlg = new CourseUnitEditDialog(null, courses, units);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/courseunits/create.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is CourseUnitDto cu)) return;
            var (courses, units) = await LoadCoursesAndUnitsAsync();
            using var dlg = new CourseUnitEditDialog(cu, courses, units);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            // courseunits uses composite key (course_id + unit_id); no separate id field needed
            await ApiService.Instance.PutAsync<object>("/courseunits/update.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is CourseUnitDto cu)) return;
            await ApiService.Instance.DeleteAsync("/courseunits/delete.php",
                new { course_id = cu.CourseId, unit_id = cu.UnitId });
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

        // Matches the shape returned by /courseunits/index.php?course_id=X
        private class CourseUnitRowDto
        {
            [JsonProperty("id")]          public int    Id        { get; set; }
            [JsonProperty("unitcode")]    public string Unitcode  { get; set; }
            [JsonProperty("unitname")]    public string Unitname  { get; set; }
            [JsonProperty("year_taken")]  public int?   YearTaken { get; set; }
        }
    }

}
