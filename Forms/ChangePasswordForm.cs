/**
 * ChangePasswordForm — simple dialog to change the current user's password.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using AtRiskTracker.Services;

namespace AtRiskTracker.Forms
{
    public class ChangePasswordForm : Form
    {
        private TextBox _txtCurrent, _txtNew, _txtConfirm;
        private Label   _lblError;
        private Button  _btnSave;

        public ChangePasswordForm()
        {
            Text            = "Change Password";
            Size            = new Size(380, 270);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            Font            = new Font("Trebuchet MS", 9f);

            int y = 20;
            AddRow("Current password",  ref _txtCurrent, ref y, true);
            AddRow("New password",      ref _txtNew,     ref y, true);
            AddRow("Confirm new",       ref _txtConfirm, ref y, true);

            _lblError = new Label
            {
                ForeColor = Color.DarkRed,
                Bounds    = new Rectangle(20, y, 320, 20),
                AutoSize  = false,
            };
            Controls.Add(_lblError);
            y += 26;

            _btnSave = new Button
            {
                Text      = "Save",
                Bounds    = new Rectangle(20, y, 150, 32),
                BackColor = Color.FromArgb(0, 70, 127),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += OnSave;

            var btnCancel = new Button
            {
                Text      = "Cancel",
                Bounds    = new Rectangle(190, y, 150, 32),
                FlatStyle = FlatStyle.Flat,
            };
            btnCancel.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { _lblError, _btnSave, btnCancel });
            AcceptButton = _btnSave;
        }

        private void AddRow(string label, ref TextBox txt, ref int y, bool password = false)
        {
            Controls.Add(new Label { Text = label, Bounds = new Rectangle(20, y, 320, 18) });
            y += 20;
            txt = new TextBox
            {
                Bounds = new Rectangle(20, y, 320, 24),
                UseSystemPasswordChar = password,
            };
            Controls.Add(txt);
            y += 30;
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
                MessageBox.Show("Password changed successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                _lblError.Text   = ex.Message;
                _btnSave.Enabled = true;
            }
        }
    }
}
