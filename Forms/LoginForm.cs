/**
 * LoginForm — initial login dialog for AtRiskTracker.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Windows.Forms;
using AtRiskTracker.Services;

namespace AtRiskTracker.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            if (Program.AppIcon != null) Icon = Program.AppIcon;
            _btnLogin.FlatAppearance.BorderSize = 0;
            // TODO: remove before release
            _txtEmail.Text    = "simonrundell@exe-coll.ac.uk";
            _txtPassword.Text = "f3oHCjb7";
            _txtEmail.Focus();
        }

        private async void OnLoginClick(object sender, EventArgs e)
        {
            _lblError.Text    = "";
            _btnLogin.Enabled = false;
            _btnLogin.Text    = "Signing in...";

            try
            {
                await ApiService.Instance.LoginAsync(
                    _txtEmail.Text.Trim(),
                    _txtPassword.Text);

                var main = new MainForm();
                main.Show();
                Hide();
                main.FormClosed += (s, a) => Close();
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message.Contains("401")
                    ? "Login or password incorrect — please try again."
                    : ex.Message;
                _btnLogin.Enabled = true;
                _btnLogin.Text    = "Sign In";
            }
        }
    }
}
