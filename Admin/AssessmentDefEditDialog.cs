/**
 * AssessmentDefEditDialog — add or edit a single assessment definition (tblassessment_def row).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    public partial class AssessmentDefEditDialog : Form
    {
        private TextBox          _txtPartName;
        private NumericUpDown    _nudSortOrder;
        private Button           _btnOk;

        /// <summary>PartName value entered by the user.</summary>
        public string PartName   => _txtPartName.Text.Trim();

        /// <summary>SortOrder value entered by the user.</summary>
        public int    SortOrder  => (int)_nudSortOrder.Value;

        public AssessmentDefEditDialog(AssessmentDefAdminDto existing)
        {
            InitializeComponent();
            BuildUi(existing);
        }

        private void BuildUi(AssessmentDefAdminDto existing)
        {
            bool isEdit = existing != null;
            Text            = isEdit ? "Edit Assessment Definition" : "Add Assessment Definition";
            Size            = new Size(380, 190);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);
            BackColor       = Color.White;

            int y = 16;

            Controls.Add(new Label { Text = "Part name:", Location = new Point(16, y), AutoSize = true });
            y += 20;

            _txtPartName = new TextBox
            {
                Location  = new Point(16, y),
                Size      = new Size(336, 24),
                Text      = existing?.PartName ?? "",
                MaxLength = 100,
            };
            Controls.Add(_txtPartName);
            y += 34;

            Controls.Add(new Label { Text = "Sort order:", Location = new Point(16, y), AutoSize = true });
            y += 20;

            _nudSortOrder = new NumericUpDown
            {
                Location = new Point(16, y),
                Size     = new Size(80, 24),
                Minimum  = 0,
                Maximum  = 999,
                Value    = existing?.SortOrder ?? 0,
            };
            Controls.Add(_nudSortOrder);
            y += 40;

            _btnOk = new Button
            {
                Text      = isEdit ? "Save" : "Add",
                Location  = new Point(16, y),
                Size      = new Size(100, 28),
                BackColor = Color.FromArgb(0, 70, 127),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnOk.FlatAppearance.BorderSize = 0;
            _btnOk.Click += OnOk;

            var btnCancel = new Button
            {
                Text         = "Cancel",
                Location     = new Point(126, y),
                Size         = new Size(100, 28),
                FlatStyle    = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
            };

            AcceptButton = _btnOk;
            CancelButton = btnCancel;
            Controls.AddRange(new System.Windows.Forms.Control[] { _btnOk, btnCancel });
        }

        private void OnOk(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtPartName.Text))
            {
                MessageBox.Show("Part name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtPartName.Focus();
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
