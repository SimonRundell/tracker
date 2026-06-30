namespace AtRiskTracker.Admin
{
    partial class UserEditDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblName     = new System.Windows.Forms.Label();
            this._txtName     = new System.Windows.Forms.TextBox();
            this._lblEmail    = new System.Windows.Forms.Label();
            this._txtEmail    = new System.Windows.Forms.TextBox();
            this._lblPassword = new System.Windows.Forms.Label();
            this._txtPassword = new System.Windows.Forms.TextBox();
            this._lblRole     = new System.Windows.Forms.Label();
            this._cboRole     = new System.Windows.Forms.ComboBox();
            this._btnOk       = new System.Windows.Forms.Button();
            this._btnCancel   = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // _lblName
            //
            this._lblName.Location = new System.Drawing.Point(20, 15);
            this._lblName.Name     = "_lblName";
            this._lblName.Size     = new System.Drawing.Size(320, 18);
            this._lblName.Text     = "Full Name";
            //
            // _txtName
            //
            this._txtName.Location = new System.Drawing.Point(20, 33);
            this._txtName.Name     = "_txtName";
            this._txtName.Size     = new System.Drawing.Size(320, 24);
            //
            // _lblEmail
            //
            this._lblEmail.Location = new System.Drawing.Point(20, 65);
            this._lblEmail.Name     = "_lblEmail";
            this._lblEmail.Size     = new System.Drawing.Size(320, 18);
            this._lblEmail.Text     = "Email";
            //
            // _txtEmail
            //
            this._txtEmail.Location = new System.Drawing.Point(20, 83);
            this._txtEmail.Name     = "_txtEmail";
            this._txtEmail.Size     = new System.Drawing.Size(320, 24);
            //
            // _lblPassword
            //
            this._lblPassword.Location = new System.Drawing.Point(20, 115);
            this._lblPassword.Name     = "_lblPassword";
            this._lblPassword.Size     = new System.Drawing.Size(320, 18);
            this._lblPassword.Text     = "Password";
            //
            // _txtPassword
            //
            this._txtPassword.Location              = new System.Drawing.Point(20, 133);
            this._txtPassword.Name                  = "_txtPassword";
            this._txtPassword.Size                  = new System.Drawing.Size(320, 24);
            this._txtPassword.UseSystemPasswordChar = true;
            //
            // _lblRole
            //
            this._lblRole.Location = new System.Drawing.Point(20, 165);
            this._lblRole.Name     = "_lblRole";
            this._lblRole.Size     = new System.Drawing.Size(320, 18);
            this._lblRole.Text     = "Role";
            //
            // _cboRole
            //
            this._cboRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboRole.Location      = new System.Drawing.Point(20, 183);
            this._cboRole.Name          = "_cboRole";
            this._cboRole.Size          = new System.Drawing.Size(320, 24);
            //
            // _btnOk
            //
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.Location  = new System.Drawing.Point(20, 215);
            this._btnOk.Name      = "_btnOk";
            this._btnOk.Size      = new System.Drawing.Size(155, 28);
            this._btnOk.Text      = "OK";
            this._btnOk.UseVisualStyleBackColor   = false;
            this._btnOk.FlatAppearance.BorderSize = 0;
            this._btnOk.DialogResult              = System.Windows.Forms.DialogResult.OK;
            //
            // _btnCancel
            //
            this._btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Location     = new System.Drawing.Point(190, 215);
            this._btnCancel.Name         = "_btnCancel";
            this._btnCancel.Size         = new System.Drawing.Size(155, 28);
            this._btnCancel.Text         = "Cancel";
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //
            // UserEditDialog
            //
            this.AcceptButton        = this._btnOk;
            this.CancelButton        = this._btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(362, 261);
            this.Controls.Add(this._lblName);
            this.Controls.Add(this._txtName);
            this.Controls.Add(this._lblEmail);
            this.Controls.Add(this._txtEmail);
            this.Controls.Add(this._lblPassword);
            this.Controls.Add(this._txtPassword);
            this.Controls.Add(this._lblRole);
            this.Controls.Add(this._cboRole);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._btnCancel);
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "UserEditDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Add User";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label   _lblName;
        private System.Windows.Forms.TextBox _txtName;
        private System.Windows.Forms.Label   _lblEmail;
        private System.Windows.Forms.TextBox _txtEmail;
        private System.Windows.Forms.Label   _lblPassword;
        private System.Windows.Forms.TextBox _txtPassword;
        private System.Windows.Forms.Label   _lblRole;
        private System.Windows.Forms.ComboBox _cboRole;
        private System.Windows.Forms.Button  _btnOk;
        private System.Windows.Forms.Button  _btnCancel;
    }
}
