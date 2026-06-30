namespace AtRiskTracker.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblTitle   = new System.Windows.Forms.Label();
            this._lblEmail   = new System.Windows.Forms.Label();
            this._txtEmail   = new System.Windows.Forms.TextBox();
            this._lblPwd     = new System.Windows.Forms.Label();
            this._txtPassword = new System.Windows.Forms.TextBox();
            this._lblError   = new System.Windows.Forms.Label();
            this._btnLogin   = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // _lblTitle
            //
            this._lblTitle.Font      = new System.Drawing.Font("Trebuchet MS", 13F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._lblTitle.Location  = new System.Drawing.Point(20, 20);
            this._lblTitle.Name      = "_lblTitle";
            this._lblTitle.Size      = new System.Drawing.Size(360, 40);
            this._lblTitle.Text      = "Marking Tracker & At Risk Register";
            this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // _lblEmail
            //
            this._lblEmail.Location = new System.Drawing.Point(60, 80);
            this._lblEmail.Name     = "_lblEmail";
            this._lblEmail.Size     = new System.Drawing.Size(280, 20);
            this._lblEmail.Text     = "Email address";
            //
            // _txtEmail
            //
            this._txtEmail.Font     = new System.Drawing.Font("Trebuchet MS", 10F);
            this._txtEmail.Location = new System.Drawing.Point(60, 100);
            this._txtEmail.Name     = "_txtEmail";
            this._txtEmail.Size     = new System.Drawing.Size(280, 25);
            //
            // _lblPwd
            //
            this._lblPwd.Location = new System.Drawing.Point(60, 140);
            this._lblPwd.Name     = "_lblPwd";
            this._lblPwd.Size     = new System.Drawing.Size(280, 20);
            this._lblPwd.Text     = "Password";
            //
            // _txtPassword
            //
            this._txtPassword.Font                  = new System.Drawing.Font("Trebuchet MS", 10F);
            this._txtPassword.Location              = new System.Drawing.Point(60, 160);
            this._txtPassword.Name                  = "_txtPassword";
            this._txtPassword.Size                  = new System.Drawing.Size(280, 25);
            this._txtPassword.UseSystemPasswordChar = true;
            //
            // _lblError
            //
            this._lblError.ForeColor = System.Drawing.Color.DarkRed;
            this._lblError.Location  = new System.Drawing.Point(60, 195);
            this._lblError.Name      = "_lblError";
            this._lblError.Size      = new System.Drawing.Size(280, 20);
            //
            // _btnLogin
            //
            this._btnLogin.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnLogin.Font      = new System.Drawing.Font("Trebuchet MS", 10F, System.Drawing.FontStyle.Bold);
            this._btnLogin.ForeColor = System.Drawing.Color.White;
            this._btnLogin.Location  = new System.Drawing.Point(60, 220);
            this._btnLogin.Name      = "_btnLogin";
            this._btnLogin.Size      = new System.Drawing.Size(280, 36);
            this._btnLogin.Text      = "Sign In";
            this._btnLogin.UseVisualStyleBackColor = false;
            this._btnLogin.Click    += new System.EventHandler(this.OnLoginClick);
            //
            // LoginForm
            //
            this.AcceptButton       = this._btnLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode      = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor          = System.Drawing.Color.White;
            this.ClientSize         = new System.Drawing.Size(404, 282);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this._lblTitle,
                this._lblEmail,
                this._txtEmail,
                this._lblPwd,
                this._txtPassword,
                this._lblError,
                this._btnLogin
            });
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "LoginForm";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "AtRisk Tracker — Login";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label    _lblTitle;
        private System.Windows.Forms.Label    _lblEmail;
        private System.Windows.Forms.TextBox  _txtEmail;
        private System.Windows.Forms.Label    _lblPwd;
        private System.Windows.Forms.TextBox  _txtPassword;
        private System.Windows.Forms.Label    _lblError;
        private System.Windows.Forms.Button   _btnLogin;
    }
}
