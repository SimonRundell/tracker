namespace AtRiskTracker.Admin
{
    partial class QualTypeEditDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblName    = new System.Windows.Forms.Label();
            this._txtName    = new System.Windows.Forms.TextBox();
            this._lblSlug    = new System.Windows.Forms.Label();
            this._txtSlug    = new System.Windows.Forms.TextBox();
            this._chkPredict = new System.Windows.Forms.CheckBox();
            this._chkNcfe    = new System.Windows.Forms.CheckBox();
            this._chkBtec    = new System.Windows.Forms.CheckBox();
            this._btnOk      = new System.Windows.Forms.Button();
            this._btnCancel  = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // _lblName
            //
            this._lblName.Location = new System.Drawing.Point(20, 15);
            this._lblName.Name     = "_lblName";
            this._lblName.Size     = new System.Drawing.Size(320, 18);
            this._lblName.Text     = "Name";
            //
            // _txtName
            //
            this._txtName.Location = new System.Drawing.Point(20, 35);
            this._txtName.Name     = "_txtName";
            this._txtName.Size     = new System.Drawing.Size(320, 24);
            //
            // _lblSlug
            //
            this._lblSlug.Location = new System.Drawing.Point(20, 67);
            this._lblSlug.Name     = "_lblSlug";
            this._lblSlug.Size     = new System.Drawing.Size(320, 18);
            this._lblSlug.Text     = "Slug";
            //
            // _txtSlug
            //
            this._txtSlug.Location = new System.Drawing.Point(20, 87);
            this._txtSlug.Name     = "_txtSlug";
            this._txtSlug.Size     = new System.Drawing.Size(320, 24);
            //
            // _chkPredict
            //
            this._chkPredict.Location = new System.Drawing.Point(20, 121);
            this._chkPredict.Name     = "_chkPredict";
            this._chkPredict.Size     = new System.Drawing.Size(200, 22);
            this._chkPredict.Text     = "Show Predict";
            //
            // _chkNcfe
            //
            this._chkNcfe.Location = new System.Drawing.Point(20, 147);
            this._chkNcfe.Name     = "_chkNcfe";
            this._chkNcfe.Size     = new System.Drawing.Size(200, 22);
            this._chkNcfe.Text     = "Is NCFE";
            //
            // _chkBtec
            //
            this._chkBtec.Location = new System.Drawing.Point(20, 173);
            this._chkBtec.Name     = "_chkBtec";
            this._chkBtec.Size     = new System.Drawing.Size(200, 22);
            this._chkBtec.Text     = "BTec Overall Grades";
            //
            // _btnOk
            //
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.Location  = new System.Drawing.Point(20, 207);
            this._btnOk.Name      = "_btnOk";
            this._btnOk.Size      = new System.Drawing.Size(150, 28);
            this._btnOk.Text      = "OK";
            this._btnOk.UseVisualStyleBackColor   = false;
            this._btnOk.FlatAppearance.BorderSize = 0;
            this._btnOk.DialogResult              = System.Windows.Forms.DialogResult.OK;
            //
            // _btnCancel
            //
            this._btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Location     = new System.Drawing.Point(185, 207);
            this._btnCancel.Name         = "_btnCancel";
            this._btnCancel.Size         = new System.Drawing.Size(150, 28);
            this._btnCancel.Text         = "Cancel";
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //
            // QualTypeEditDialog
            //
            this.AcceptButton        = this._btnOk;
            this.CancelButton        = this._btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(362, 253);
            this.Controls.Add(this._lblName);
            this.Controls.Add(this._txtName);
            this.Controls.Add(this._lblSlug);
            this.Controls.Add(this._txtSlug);
            this.Controls.Add(this._chkPredict);
            this.Controls.Add(this._chkNcfe);
            this.Controls.Add(this._chkBtec);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._btnCancel);
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "QualTypeEditDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Add Qual Type";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label    _lblName;
        private System.Windows.Forms.TextBox  _txtName;
        private System.Windows.Forms.Label    _lblSlug;
        private System.Windows.Forms.TextBox  _txtSlug;
        private System.Windows.Forms.CheckBox _chkPredict;
        private System.Windows.Forms.CheckBox _chkNcfe;
        private System.Windows.Forms.CheckBox _chkBtec;
        private System.Windows.Forms.Button   _btnOk;
        private System.Windows.Forms.Button   _btnCancel;
    }
}
