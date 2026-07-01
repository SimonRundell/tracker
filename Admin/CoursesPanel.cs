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
    public partial class CoursesPanel : AdminPanelBase
    {
        private TextBox _filterName, _filterQualType;

        public CoursesPanel() { InitializeComponent(); }

        protected override Control BuildFilterBar()
        {
            var bar = MakeFilterPanel();
            _filterName     = AddFilterBox(bar, "Course Name", 52, 200);
            _filterQualType = AddFilterBox(bar, "Qual Type",  260, 140);
            AddClearButton(bar, 408, _filterName, _filterQualType);
            return bar;
        }

        protected override bool RowMatchesFilter(DataGridViewRow row)
            => CellContains(row, 1, _filterName?.Text)
            && CellContains(row, 2, _filterQualType?.Text);

        protected override void DefineColumns()
        {
            AddColText("ID",          "id",          0.4f);
            AddColText("Course Name", "coursename",  2f);
            AddColText("Qual Type",   "qual_type",   1f);
            AddColText("Pass Pts",    "pass_pts",    0.6f);
            AddColText("Merit Pts",   "merit_pts",   0.6f);
            AddColText("Dist Pts",    "dist_pts",    0.6f);
            HideIdColumn();
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
            await ApiService.Instance.PostAsync<object>("/courses/create.php", dlg.ToPayload());
            await ReloadAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is CourseDto c)) return;
            using var dlg = new CourseEditDialog(c, await LoadQualTypesAsync());
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>("/courses/update.php", dlg.ToPayload(c.Id));
            await ReloadAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is CourseDto c)) return;
            await ApiService.Instance.DeleteAsync("/courses/delete.php", new { id = c.Id });
            await ReloadAsync();
        }

        private Task<List<QualTypeDto>> LoadQualTypesAsync()
            => System.Threading.Tasks.Task.FromResult(QualTypeDto.All());
    }

}
