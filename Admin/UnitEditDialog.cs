/**
 * UnitEditDialog — add/edit a unit record.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    internal partial class UnitEditDialog : Form
    {
        public UnitEditDialog() { InitializeComponent(); }

        public UnitEditDialog(UnitAdminDto u) : this()
        {
            if (u != null)
            {
                Text                 = "Edit Unit";
                _txtCode.Text        = u.Unitcode;
                _txtName.Text        = u.Unitname;
                _numCredits.Value    = u.Credits;
                _numGlh.Value        = u.Glh;
                _chkExternal.Checked = u.IsExternal != 0;
            }
        }

        public object ToPayload(int id = 0) => new
        {
            id,
            unitcode    = _txtCode.Text.Trim(),
            unitname    = _txtName.Text.Trim(),
            credits     = (int)_numCredits.Value,
            glh         = (int)_numGlh.Value,
            is_external = _chkExternal.Checked ? 1 : 0,
        };
    }
}
