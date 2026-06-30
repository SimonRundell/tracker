/**
 * ChangePasswordForm — simple dialog to change the current user's password.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Windows.Forms;
using AtRiskTracker.Services;

namespace AtRiskTracker.Forms
{
    public partial class ChangePasswordForm : Form
    {
        public ChangePasswordForm()
        {
            InitializeComponent();
            _btnSave.FlatAppearance.BorderSize   = 0;
            _btnCancel.FlatAppearance.BorderSize = 0;
        }

        private async void OnSave(object sender, EventArgs e)
        {
            _lblError.Text   = "";
            if (_txtNew.Text != _txtConfirm.Text)
            {
                _lblError.Text = "New passwords do not match.";
                return;
            }
            _btnSave.Enabled = false;
            try
            {
                await ApiService.Instance.ChangePasswordAsync(_txtCurrent.Text, _txtNew.Text);
                System.Windows.Forms.MessageBox.Show("Password changed successfully.", "Success",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                _lblError.Text   = ex.Message;
                _btnSave.Enabled = true;
            }
        }

        private void OnCancel(object sender, EventArgs e) => Close();
    }
}
