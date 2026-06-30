/**
 * CriterionEditDialog — add/edit a single assessment criterion.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    internal partial class CriterionEditDialog : Form
    {
        public CriterionEditDialog() { InitializeComponent(); }

        public CriterionEditDialog(CriterionDto crit, int defaultOrder = 10) : this()
        {
            _numOrder.Value = defaultOrder;
            if (crit != null)
            {
                Text            = "Edit Criterion";
                _txtCode.Text   = crit.Code;
                _txtDesc.Text   = crit.Description;
                _numOrder.Value = crit.SortOrder;
            }
        }

        /// <summary>Payload for a new criterion — omits id.</summary>
        public object ToPayload(int sectionId) => new
        {
            section_id  = sectionId,
            code        = _txtCode.Text.Trim(),
            description = _txtDesc.Text.Trim(),
            sort_order  = (int)_numOrder.Value,
        };

        /// <summary>Payload for updating an existing criterion.</summary>
        public object ToPayload(int sectionId, int criterionId) => new
        {
            id          = criterionId,
            section_id  = sectionId,
            code        = _txtCode.Text.Trim(),
            description = _txtDesc.Text.Trim(),
            sort_order  = (int)_numOrder.Value,
        };
    }
}
