/**
 * SectionEditDialog — add/edit a criterion section within a unit.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    internal partial class SectionEditDialog : Form
    {
        public SectionEditDialog() { InitializeComponent(); }

        public SectionEditDialog(SectionDto sec, int defaultOrder = 10) : this()
        {
            _numOrder.Value = defaultOrder;
            if (sec != null)
            {
                Text            = "Edit Section";
                _txtLabel.Text  = sec.SectionLabel;
                _txtTitle.Text  = sec.SectionTitle;
                _numOrder.Value = sec.SortOrder;
            }
        }

        /// <summary>Payload for a new section — omits id.</summary>
        public object ToPayload(int unitId) => new
        {
            unit_id    = unitId,
            label      = _txtLabel.Text.Trim(),
            title      = _txtTitle.Text.Trim(),
            sort_order = (int)_numOrder.Value,
        };

        /// <summary>Payload for updating an existing section — includes id.</summary>
        public object ToPayload(int unitId, int sectionId) => new
        {
            id         = sectionId,
            unit_id    = unitId,
            label      = _txtLabel.Text.Trim(),
            title      = _txtTitle.Text.Trim(),
            sort_order = (int)_numOrder.Value,
        };
    }
}
