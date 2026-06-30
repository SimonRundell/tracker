namespace AtRiskTracker.Forms
{
    partial class ChangePasswordForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblCurrent = new System.Windows.Forms.Label();
            this._txtCurrent = new System.Windows.Forms.TextBox();
            this._lblNew     = new System.Windows.Forms.Label();
            this._txtNew     = new System.Windows.Forms.TextBox();
            this._lblConfirm = new System.Windows.Forms.Label();
            this._txtConfirm = new System.Windows.Forms.TextBox();
            this._lblError   = new System.Windows.Forms.Label();
            this._btnSave    = new System.Windows.Forms.Button();
            this._btnCancel  = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // _lblCurrent
            //
            this._lblCurrent.Location = new System.Drawing.Point(20, 20);
            this._lblCurrent.Name     = "_lblCurrent";
            this._lblCurrent.Size     = new System.Drawing.Size(320, 18);
            this._lblCurrent.Text     = "Current password";
            //
            // _txtCurrent
            //
            this._txtCurrent.Location              = new System.Drawing.Point(20, 40);
            this._txtCurrent.Name                  = "_txtCurrent";
            this._txtCurrent.Size                  = new System.Drawing.Size(320, 24);
            this._txtCurrent.UseSystemPasswordChar = true;
            //
            // _lblNew
            //
            this._lblNew.Location = new System.Drawing.Point(20, 70);
            this._lblNew.Name     = "_lblNew";
            this._lblNew.Size     = new System.Drawing.Size(320, 18);
            this._lblNew.Text     = "New password";
            //
            // _txtNew
            //
            this._txtNew.Location              = new System.Drawing.Point(20, 90);
            this._txtNew.Name                  = "_txtNew";
            this._txtNew.Size                  = new System.Drawing.Size(320, 24);
            this._txtNew.UseSystemPasswordChar = true;
            //
            // _lblConfirm
            //
            this._lblConfirm.Location = new System.Drawing.Point(20, 120);
            this._lblConfirm.Name     = "_lblConfirm";
            this._lblConfirm.Size     = new System.Drawing.Size(320, 18);
            this._lblConfirm.Text     = "Confirm new";
            //
            // _txtConfirm
            //
            this._txtConfirm.Location              = new System.Drawing.Point(20, 140);
            this._txtConfirm.Name                  = "_txtConfirm";
            this._txtConfirm.Size                  = new System.Drawing.Size(320, 24);
            this._txtConfirm.UseSystemPasswordChar = true;
            //
            // _lblError
            //
            this._lblError.ForeColor = System.Drawing.Color.DarkRed;
            this._lblError.Location  = new System.Drawing.Point(20, 170);
            this._lblError.Name      = "_lblError";
            this._lblError.Size      = new System.Drawing.Size(320, 20);
            //
            // _btnSave
            //
            this._btnSave.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSave.ForeColor = System.Drawing.Color.White;
            this._btnSave.Location  = new System.Drawing.Point(20, 196);
            this._btnSave.Name      = "_btnSave";
            this._btnSave.Size      = new System.Drawing.Size(150, 32);
            this._btnSave.Text      = "Save";
            this._btnSave.UseVisualStyleBackColor = false;
            this._btnSave.Click    += new System.EventHandler(this.OnSave);
            //
            // _btnCancel
            //
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Location  = new System.Drawing.Point(190, 196);
            this._btnCancel.Name      = "_btnCancel";
            this._btnCancel.Size      = new System.Drawing.Size(150, 32);
            this._btnCancel.Text      = "Cancel";
            this._btnCancel.Click    += new System.EventHandler(this.OnCancel);
            //
            // ChangePasswordForm
            //
            this.AcceptButton        = this._btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(362, 244);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this._lblCurrent, this._txtCurrent,
                this._lblNew,     this._txtNew,
                this._lblConfirm, this._txtConfirm,
                this._lblError,
                this._btnSave,    this._btnCancel
            });
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "ChangePasswordForm";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Change Password";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label    _lblCurrent;
        private System.Windows.Forms.TextBox  _txtCurrent;
        private System.Windows.Forms.Label    _lblNew;
        private System.Windows.Forms.TextBox  _txtNew;
        private System.Windows.Forms.Label    _lblConfirm;
        private System.Windows.Forms.TextBox  _txtConfirm;
        private System.Windows.Forms.Label    _lblError;
        private System.Windows.Forms.Button   _btnSave;
        private System.Windows.Forms.Button   _btnCancel;
    }
}
