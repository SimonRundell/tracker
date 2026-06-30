/**
 * ConcernsPanel — admin CRUD for concern labels.
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
    public class ConcernsPanel : AdminPanelBase
    {
        protected override void DefineColumns()
        {
            AddColText("ID",    "id",    0.4f);
            AddColText("Label", "label", 3f);
            AddColText("Colour","color", 1f);
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<ConcernDto>>("/concerns/index.php");
            foreach (var c in resp?.Data ?? new List<ConcernDto>())
            {
                var row = AddRow(c.Id, c.Label, c.Color ?? "");
                row.Tag = c;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new ConcernEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<object>("/concerns/index.php", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is ConcernDto c)) return;
            using var dlg = new ConcernEditDialog(c);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<object>($"/concerns/index.php?id={c.Id}", dlg.ToPayload());
            await LoadDataAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is ConcernDto c)) return;
            await ApiService.Instance.DeleteAsync($"/concerns/index.php?id={c.Id}");
            await LoadDataAsync();
        }
    }

    internal class ConcernEditDialog : Form
    {
        private TextBox _txtLabel, _txtColor;

        public ConcernEditDialog(ConcernDto c)
        {
            Text            = c == null ? "Add Concern" : "Edit Concern";
            Size            = new Size(360, 190);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);

            Controls.Add(new Label { Text="Label", Bounds=new Rectangle(20,15,300,18) });
            _txtLabel = new TextBox { Text=c?.Label??"", Bounds=new Rectangle(20,33,300,24) }; Controls.Add(_txtLabel);

            Controls.Add(new Label { Text="Colour (CSS hex, e.g. #ff0000)", Bounds=new Rectangle(20,63,300,18) });
            _txtColor = new TextBox { Text=c?.Color??"", Bounds=new Rectangle(20,81,300,24) }; Controls.Add(_txtColor);

            var ok=new Button{Text="OK",Bounds=new Rectangle(20,115,130,28),BackColor=Color.FromArgb(0,70,127),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.OK};ok.FlatAppearance.BorderSize=0;
            var cancel=new Button{Text="Cancel",Bounds=new Rectangle(165,115,130,28),FlatStyle=FlatStyle.Flat,DialogResult=DialogResult.Cancel};
            Controls.AddRange(new Control[]{ok,cancel}); AcceptButton=ok; CancelButton=cancel;
        }

        public object ToPayload() => new
        {
            label = _txtLabel.Text.Trim(),
            color = _txtColor.Text.Trim(),
        };
    }
}
