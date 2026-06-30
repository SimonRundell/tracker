/**
 * StudentEditDialog — add/edit a student record.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Windows.Forms;
using AtRiskTracker.Models;

namespace AtRiskTracker.Admin
{
    internal partial class StudentEditDialog : Form
    {
        public StudentEditDialog() { InitializeComponent(); }

        public StudentEditDialog(StudentAdminDto s) : this()
        {
            if (s != null)
            {
                Text           = "Edit Student";
                _txtCis.Text   = s.Cisnumber;
                _txtFirst.Text = s.Firstname;
                _txtLast.Text  = s.Lastname;
            }
        }

        public object ToPayload(int id = 0) => new
        {
            id,
            cisnumber = _txtCis.Text.Trim(),
            firstname = _txtFirst.Text.Trim(),
            lastname  = _txtLast.Text.Trim(),
        };
    }
}
