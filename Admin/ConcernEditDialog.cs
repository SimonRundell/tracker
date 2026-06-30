/**
 * ConcernEditDialog — add/edit a concern label.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    internal partial class ConcernEditDialog : Form
    {
        public ConcernEditDialog() { InitializeComponent(); }

        public ConcernEditDialog(ConcernDto c) : this()
        {
            if (c != null)
            {
                Text                = "Edit Concern";
                _txtConcern.Text    = c.Concern;
            }
        }

        public object ToPayload(int id = 0) => new
        {
            id,
            concern = _txtConcern.Text.Trim(),
        };
    }
}
