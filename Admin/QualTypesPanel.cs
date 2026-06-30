/**
 * QualTypesPanel — admin CRUD for qualification types.
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
    public class QualTypesPanel : AdminPanelBase
    {
        protected override void DefineColumns()
        {
            AddColText("ID",          "id",      0.4f);
            AddColText("Name",        "name",    2f);
            AddColText("Slug",        "slug",    1.5f);
            AddColText("Show Predict","predict", 0.7f);
            AddColText("Is NCFE",     "ncfe",    0.7f);
            AddColText("BTec Grades", "btec",    0.7f);
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<QualTypesResponse>("/qualtypes/index.php");
            foreach (var q in resp?.Data ?? new List<QualTypeDto>())
            {
                var row = AddRow(q.Id, q.Name, q.Slug,
                    q.ShowPredict != 0 ? "Yes" : "No",
                    q.IsNcfe != 0 ? "Yes" : "No",
                    q.BtecOverallGrades != 0 ? "Yes" : "No");
                row.Tag = q;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new QualTypeEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/qualtypes/index.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is QualTypeDto q)) return;
            using var dlg = new QualTypeEditDialog(q);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>($"/qualtypes/index.php?id={q.Id}", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is QualTypeDto q)) return;
            await ApiService.Instance.DeleteAsync($"/qualtypes/index.php?id={q.Id}");
            await LoadDataAsync();
        }
    }

    internal class QualTypeEditDialog : Form
    {
        private TextBox  _txtName, _txtSlug;
        private CheckBox _chkPredict, _chkNcfe, _chkBtec;

        public QualTypeEditDialog(QualTypeDto q)
        {
            Text            = q == null ? "Add Qual Type" : "Edit Qual Type";
            Size            = new Size(380, 280);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);

            int y = 15;
            Field("Name", ref _txtName, ref y, q?.Name);
            Field("Slug", ref _txtSlug, ref y, q?.Slug);

            _chkPredict = new CheckBox { Text = "Show Predict", Checked = (q?.ShowPredict ?? 0) != 0, Bounds = new Rectangle(20, y, 200, 22) }; Controls.Add(_chkPredict); y += 26;
            _chkNcfe    = new CheckBox { Text = "Is NCFE",      Checked = (q?.IsNcfe      ?? 0) != 0, Bounds = new Rectangle(20, y, 200, 22) }; Controls.Add(_chkNcfe); y += 26;
            _chkBtec    = new CheckBox { Text = "BTec Overall Grades", Checked = (q?.BtecOverallGrades ?? 0) != 0, Bounds = new Rectangle(20, y, 200, 22) }; Controls.Add(_chkBtec); y += 32;

            var ok = new Button { Text="OK", Bounds=new Rectangle(20,y,150,28), BackColor=Color.FromArgb(0,70,127),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.OK };
            ok.FlatAppearance.BorderSize=0;
            var cancel = new Button { Text="Cancel", Bounds=new Rectangle(185,y,150,28),FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.Cancel };
            Controls.AddRange(new Control[]{ok,cancel}); AcceptButton=ok; CancelButton=cancel;
        }

        private void Field(string lbl, ref TextBox txt, ref int y, string val)
        {
            Controls.Add(new Label { Text=lbl, Bounds=new Rectangle(20,y,320,18) }); y+=20;
            txt = new TextBox { Text=val??"", Bounds=new Rectangle(20,y,320,24) }; Controls.Add(txt); y+=32;
        }

        public object ToPayload() => new
        {
            name               = _txtName.Text.Trim(),
            slug               = _txtSlug.Text.Trim(),
            show_predict       = _chkPredict.Checked ? 1 : 0,
            is_ncfe            = _chkNcfe.Checked ? 1 : 0,
            btec_overall_grades= _chkBtec.Checked ? 1 : 0,
        };
    }
}
