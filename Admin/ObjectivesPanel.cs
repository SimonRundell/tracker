/**
 * ObjectivesPanel — admin CRUD for learning objectives / grade-band criteria.
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
    public class ObjectivesPanel : AdminPanelBase
    {
        protected override void DefineColumns()
        {
            AddColText("ID",          "id",          0.4f);
            AddColText("Unit Code",   "unitcode",    0.8f);
            AddColText("Grade",       "grade",       0.5f);
            AddColText("Description", "description", 4f);
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<ObjectiveDto>>("/criteria/index.php");
            foreach (var o in resp?.Data ?? new List<ObjectiveDto>())
            {
                var row = AddRow(o.Id, o.Unitcode, o.Grade, o.Description);
                row.Tag = o;
            }
        }

        protected override async Task AddItemAsync()
        {
            var units = await LoadUnitsAsync();
            using var dlg = new ObjectiveEditDialog(null, units);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/criteria/index.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is ObjectiveDto o)) return;
            var units = await LoadUnitsAsync();
            using var dlg = new ObjectiveEditDialog(o, units);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>($"/criteria/index.php?id={o.Id}", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is ObjectiveDto o)) return;
            await ApiService.Instance.DeleteAsync($"/criteria/index.php?id={o.Id}");
            await LoadDataAsync();
        }

        private async Task<List<UnitAdminDto>> LoadUnitsAsync()
        {
            try { var r = await ApiService.Instance.GetAsync<ListResponse<UnitAdminDto>>("/units/index.php"); return r?.Data ?? new List<UnitAdminDto>(); }
            catch { return new List<UnitAdminDto>(); }
        }
    }

    internal class ObjectiveEditDialog : Form
    {
        private ComboBox _cboUnit, _cboGrade;
        private TextBox  _txtDesc;

        private static readonly string[] Grades = { "P", "M", "D" };

        public ObjectiveEditDialog(ObjectiveDto o, List<UnitAdminDto> units)
        {
            Text            = o == null ? "Add Objective" : "Edit Objective";
            Size            = new Size(460, 280);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);

            Controls.Add(new Label { Text="Unit", Bounds=new Rectangle(20,15,400,18) });
            _cboUnit = new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList, Bounds=new Rectangle(20,33,400,24) };
            _cboUnit.Items.Add("-- select --"); _cboUnit.SelectedIndex=0;
            foreach (var u in units) _cboUnit.Items.Add(u);
            if (o!=null) foreach(UnitAdminDto u in _cboUnit.Items) if(u is UnitAdminDto ud&&ud.Id==o.UnitId){_cboUnit.SelectedItem=ud;break;}
            Controls.Add(_cboUnit);

            Controls.Add(new Label { Text="Grade", Bounds=new Rectangle(20,63,160,18) });
            _cboGrade = new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList, Bounds=new Rectangle(20,81,160,24) };
            foreach (string g in Grades) _cboGrade.Items.Add(g);
            _cboGrade.SelectedItem = o?.Grade ?? "P";
            Controls.Add(_cboGrade);

            Controls.Add(new Label { Text="Description", Bounds=new Rectangle(20,111,400,18) });
            _txtDesc = new TextBox { Text=o?.Description??"", Bounds=new Rectangle(20,129,400,44), Multiline=true }; Controls.Add(_txtDesc);

            var ok=new Button{Text="OK",Bounds=new Rectangle(20,183,185,28),BackColor=Color.FromArgb(0,70,127),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.OK};ok.FlatAppearance.BorderSize=0;
            var cancel=new Button{Text="Cancel",Bounds=new Rectangle(218,183,185,28),FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.Cancel};
            Controls.AddRange(new Control[]{ok,cancel}); AcceptButton=ok; CancelButton=cancel;
        }

        public object ToPayload() => new
        {
            unit_id     = (_cboUnit.SelectedItem is UnitAdminDto u) ? u.Id : 0,
            grade       = _cboGrade.SelectedItem?.ToString() ?? "P",
            description = _txtDesc.Text.Trim(),
        };
    }
}
