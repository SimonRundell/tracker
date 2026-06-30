/**
 * LoginForm — initial login dialog for AtRiskTracker.
 *
 * Authenticates staff via the /auth/login.php endpoint and opens
 * MainForm on success. Entirely code-built (no .Designer.cs).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using AtRiskTracker.Services;

namespace AtRiskTracker.Forms
{
    public class LoginForm : Form
    {
        private TextBox    _txtEmail;
        private TextBox    _txtPassword;
        private Button     _btnLogin;
        private Label      _lblError;
        private Label      _lblTitle;

        public LoginForm()
        {
            BuildUi();
        }

        private void BuildUi()
        {
            Text            = "AtRisk Tracker — Login";
            Size            = new Size(420, 320);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = Color.White;
            Font            = new Font("Trebuchet MS", 9f);

            _lblTitle = new Label
            {
                Text      = "Marking Tracker & At Risk Register",
                Font      = new Font("Trebuchet MS", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 70, 127),
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds    = new Rectangle(20, 20, 360, 40),
            };

            var lblEmail = new Label
            {
                Text   = "Email address",
                Bounds = new Rectangle(60, 80, 280, 20),
            };
            _txtEmail = new TextBox
            {
                Bounds        = new Rectangle(60, 100, 280, 25),
                Font          = new Font("Trebuchet MS", 10f),
            };

            var lblPwd = new Label
            {
                Text   = "Password",
                Bounds = new Rectangle(60, 140, 280, 20),
            };
            _txtPassword = new TextBox
            {
                Bounds        = new Rectangle(60, 160, 280, 25),
                Font          = new Font("Trebuchet MS", 10f),
                UseSystemPasswordChar = true,
            };

            _lblError = new Label
            {
                Text      = "",
                ForeColor = Color.DarkRed,
                Bounds    = new Rectangle(60, 195, 280, 20),
                AutoSize  = false,
            };

            _btnLogin = new Button
            {
                Text    = "Sign In",
                Bounds  = new Rectangle(60, 220, 280, 36),
                Font    = new Font("Trebuchet MS", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 70, 127),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnLogin.FlatAppearance.BorderSize = 0;
            _btnLogin.Click += OnLoginClick;

            // Allow Enter key to submit
            AcceptButton = _btnLogin;

            Controls.AddRange(new Control[]
            {
                _lblTitle, lblEmail, _txtEmail, lblPwd, _txtPassword, _lblError, _btnLogin
            });

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
                _lblError.Text    = ex.Message;
                _btnLogin.Enabled = true;
                _btnLogin.Text    = "Sign In";
            }
        }
    }
}
