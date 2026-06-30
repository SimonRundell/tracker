/**
 * QualTypeEditDialog — add/edit a qualification type.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    internal partial class QualTypeEditDialog : Form
    {
        public QualTypeEditDialog() { InitializeComponent(); }

        public QualTypeEditDialog(QualTypeDto q) : this()
        {
            if (q != null)
            {
                Text                = "Edit Qual Type";
                _txtName.Text       = q.Name;
                _txtSlug.Text       = q.Slug;
                _chkPredict.Checked = q.ShowPredict != 0;
                _chkNcfe.Checked    = q.IsNcfe != 0;
                _chkBtec.Checked    = q.BtecOverallGrades != 0;
            }
        }

        public object ToPayload() => new
        {
            name                = _txtName.Text.Trim(),
            slug                = _txtSlug.Text.Trim(),
            show_predict        = _chkPredict.Checked ? 1 : 0,
            is_ncfe             = _chkNcfe.Checked ? 1 : 0,
            btec_overall_grades = _chkBtec.Checked ? 1 : 0,
        };
    }
}
