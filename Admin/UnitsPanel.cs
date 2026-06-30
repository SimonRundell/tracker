/**
 * UnitsPanel — admin CRUD for units.
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
    public class UnitsPanel : AdminPanelBase
    {
        protected override void DefineColumns()
        {
            AddColText("ID",       "id",        0.4f);
            AddColText("Code",     "unitcode",  0.8f);
            AddColText("Name",     "unitname",  2.5f);
            AddColText("Credits",  "credits",   0.5f);
            AddColText("GLH",      "glh",       0.5f);
            AddColText("External", "external",  0.6f);
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<UnitAdminDto>>("/units/index.php");
            foreach (var u in resp?.Data ?? new List<UnitAdminDto>())
            {
                var row = AddRow(u.Id, u.Unitcode, u.Unitname, u.Credits, u.Glh,
                    u.IsExternal != 0 ? "Yes" : "No");
                row.Tag = u;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new UnitEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/units/index.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is UnitAdminDto u)) return;
            using var dlg = new UnitEditDialog(u);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>($"/units/index.php?id={u.Id}", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is UnitAdminDto u)) return;
            await ApiService.Instance.DeleteAsync($"/units/index.php?id={u.Id}");
            await LoadDataAsync();
        }
    }

    internal class UnitEditDialog : Form
    {
        private TextBox       _txtCode, _txtName;
        private NumericUpDown _numCredits, _numGlh;
        private CheckBox      _chkExternal;

        public UnitEditDialog(UnitAdminDto u)
        {
            Text            = u == null ? "Add Unit" : "Edit Unit";
            Size            = new Size(400, 310);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);

            int y = 15;
            Field("Unit Code", ref _txtCode, ref y, u?.Unitcode);
            Field("Unit Name", ref _txtName, ref y, u?.Unitname);

            Controls.Add(new Label { Text="Credits", Bounds=new Rectangle(20,y,160,18) });
            Controls.Add(new Label { Text="GLH",     Bounds=new Rectangle(200,y,160,18) }); y+=18;
            _numCredits = new NumericUpDown { Bounds=new Rectangle(20,y,160,24),Minimum=0,Maximum=500,Value=u?.Credits??0 }; Controls.Add(_numCredits);
            _numGlh     = new NumericUpDown { Bounds=new Rectangle(200,y,160,24),Minimum=0,Maximum=1000,Value=u?.Glh??0 }; Controls.Add(_numGlh); y+=32;

            _chkExternal = new CheckBox { Text="External Unit", Checked=(u?.IsExternal??0)!=0, Bounds=new Rectangle(20,y,200,22) }; Controls.Add(_chkExternal); y+=32;

            var ok = new Button { Text="OK",Bounds=new Rectangle(20,y,160,28),BackColor=Color.FromArgb(0,70,127),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.OK };
            ok.FlatAppearance.BorderSize=0;
            var cancel = new Button { Text="Cancel",Bounds=new Rectangle(195,y,160,28),FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.Cancel };
            Controls.AddRange(new Control[]{ok,cancel}); AcceptButton=ok; CancelButton=cancel;
        }

        private void Field(string lbl, ref TextBox txt, ref int y, string val)
        {
            Controls.Add(new Label { Text=lbl, Bounds=new Rectangle(20,y,340,18) }); y+=20;
            txt = new TextBox { Text=val??"", Bounds=new Rectangle(20,y,340,24) }; Controls.Add(txt); y+=32;
        }

        public object ToPayload() => new
        {
            unitcode    = _txtCode.Text.Trim(),
            unitname    = _txtName.Text.Trim(),
            credits     = (int)_numCredits.Value,
            glh         = (int)_numGlh.Value,
            is_external = _chkExternal.Checked ? 1 : 0,
        };
    }
}
