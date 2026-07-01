/**
 * BulkDateDialog — sets an assignment "date set" across all students in a group.
 *
 * Presents a DateTimePicker, a choice of which assessment part(s) to update,
 * and an overwrite option. The caller then sends the result to the
 * /assessments/bulk-update.php endpoint.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Dialogs
{
    public partial class BulkDateDialog : Form
    {
        // ── outputs ──────────────────────────────────────────────────────────
        /// <summary>Assessment def IDs to update (all defs if the "All parts" entry was chosen).</summary>
        public List<int> SelectedDefIds { get; private set; } = new List<int>();

        /// <summary>date_set value in yyyy-MM-dd format.</summary>
        public string DateSet { get; private set; }

        /// <summary>When true, overwrite existing records; otherwise only fill where not yet set.</summary>
        public bool OverwriteAll { get; private set; }

        // ── state ─────────────────────────────────────────────────────────────
        private readonly List<AssessmentDefDto> _defs;
        private ComboBox        _cboPart;
        private DateTimePicker  _dtpDate;
        private CheckBox        _chkOverwrite;
        private Button          _btnOk;

        public BulkDateDialog(UnitDto unit)
        {
            InitializeComponent();
            _defs = unit.AssessmentDefs ?? new List<AssessmentDefDto>();
            BuildUi($"{unit.Unitcode} — {unit.Unitname}");
        }

        private void BuildUi(string unitLabel)
        {
            Text            = $"Set Assignment Date — {unitLabel}";
            Size            = new Size(440, 270);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);
            BackColor       = Color.White;

            int y = 18;

            Controls.Add(new Label
            {
                Text     = "Apply to:",
                Location = new Point(20, y),
                AutoSize = true,
                Font     = new Font("Trebuchet MS", 9f),
            });
            y += 22;

            _cboPart = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location      = new Point(20, y),
                Size          = new Size(390, 24),
            };
            _cboPart.Items.Add("— All assessment parts —");
            foreach (var d in _defs) _cboPart.Items.Add(d.Title ?? $"Part {d.Id}");
            _cboPart.SelectedIndex = 0;
            Controls.Add(_cboPart);
            y += 36;

            Controls.Add(new Label
            {
                Text     = "Date assigned to students:",
                Location = new Point(20, y),
                AutoSize = true,
            });
            y += 22;

            _dtpDate = new DateTimePicker
            {
                Location     = new Point(20, y),
                Size         = new Size(200, 24),
                Format       = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Value        = DateTime.Today,
            };
            Controls.Add(_dtpDate);
            y += 38;

            _chkOverwrite = new CheckBox
            {
                Text     = "Overwrite students who already have a date set",
                Location = new Point(20, y),
                AutoSize = true,
                Checked  = false,
            };
            Controls.Add(_chkOverwrite);
            y += 36;

            _btnOk = new Button
            {
                Text      = "Apply to All Students",
                Location  = new Point(20, y),
                Size      = new Size(180, 30),
                BackColor = Color.FromArgb(0, 70, 127),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnOk.FlatAppearance.BorderSize = 0;
            _btnOk.Click += OnOk;

            var btnCancel = new Button
            {
                Text      = "Cancel",
                Location  = new Point(212, y),
                Size      = new Size(110, 30),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
            };

            AcceptButton = _btnOk;
            CancelButton = btnCancel;
            Controls.AddRange(new Control[] { _btnOk, btnCancel });
        }

        private void OnOk(object sender, EventArgs e)
        {
            DateSet      = _dtpDate.Value.ToString("yyyy-MM-dd");
            OverwriteAll = _chkOverwrite.Checked;

            if (_cboPart.SelectedIndex == 0)
                // "All parts" selected
                SelectedDefIds = _defs.Select(d => d.Id).ToList();
            else
                SelectedDefIds = new List<int> { _defs[_cboPart.SelectedIndex - 1].Id };

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
