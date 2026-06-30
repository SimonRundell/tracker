namespace AtRiskTracker.Admin
{
    partial class StudentEditDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblCis    = new System.Windows.Forms.Label();
            this._txtCis    = new System.Windows.Forms.TextBox();
            this._lblFirst  = new System.Windows.Forms.Label();
            this._txtFirst  = new System.Windows.Forms.TextBox();
            this._lblLast   = new System.Windows.Forms.Label();
            this._txtLast   = new System.Windows.Forms.TextBox();
            this._btnOk     = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // _lblCis
            //
            this._lblCis.Location = new System.Drawing.Point(20, 20);
            this._lblCis.Name     = "_lblCis";
            this._lblCis.Size     = new System.Drawing.Size(340, 18);
            this._lblCis.Text     = "CIS Number";
            //
            // _txtCis
            //
            this._txtCis.Location = new System.Drawing.Point(20, 40);
            this._txtCis.Name     = "_txtCis";
            this._txtCis.Size     = new System.Drawing.Size(340, 24);
            //
            // _lblFirst
            //
            this._lblFirst.Location = new System.Drawing.Point(20, 72);
            this._lblFirst.Name     = "_lblFirst";
            this._lblFirst.Size     = new System.Drawing.Size(340, 18);
            this._lblFirst.Text     = "Forename";
            //
            // _txtFirst
            //
            this._txtFirst.Location = new System.Drawing.Point(20, 92);
            this._txtFirst.Name     = "_txtFirst";
            this._txtFirst.Size     = new System.Drawing.Size(340, 24);
            //
            // _lblLast
            //
            this._lblLast.Location = new System.Drawing.Point(20, 124);
            this._lblLast.Name     = "_lblLast";
            this._lblLast.Size     = new System.Drawing.Size(340, 18);
            this._lblLast.Text     = "Surname";
            //
            // _txtLast
            //
            this._txtLast.Location = new System.Drawing.Point(20, 144);
            this._txtLast.Name     = "_txtLast";
            this._txtLast.Size     = new System.Drawing.Size(340, 24);
            //
            // _btnOk
            //
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.Location  = new System.Drawing.Point(20, 176);
            this._btnOk.Name      = "_btnOk";
            this._btnOk.Size      = new System.Drawing.Size(150, 30);
            this._btnOk.Text      = "OK";
            this._btnOk.UseVisualStyleBackColor   = false;
            this._btnOk.FlatAppearance.BorderSize = 0;
            this._btnOk.DialogResult              = System.Windows.Forms.DialogResult.OK;
            //
            // _btnCancel
            //
            this._btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Location     = new System.Drawing.Point(185, 176);
            this._btnCancel.Name         = "_btnCancel";
            this._btnCancel.Size         = new System.Drawing.Size(150, 30);
            this._btnCancel.Text         = "Cancel";
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //
            // StudentEditDialog
            //
            this.AcceptButton        = this._btnOk;
            this.CancelButton        = this._btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(382, 224);
            this.Controls.Add(this._lblCis);
            this.Controls.Add(this._txtCis);
            this.Controls.Add(this._lblFirst);
            this.Controls.Add(this._txtFirst);
            this.Controls.Add(this._lblLast);
            this.Controls.Add(this._txtLast);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._btnCancel);
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "StudentEditDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Add Student";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label   _lblCis;
        private System.Windows.Forms.TextBox _txtCis;
        private System.Windows.Forms.Label   _lblFirst;
        private System.Windows.Forms.TextBox _txtFirst;
        private System.Windows.Forms.Label   _lblLast;
        private System.Windows.Forms.TextBox _txtLast;
        private System.Windows.Forms.Button  _btnOk;
        private System.Windows.Forms.Button  _btnCancel;
    }
}
