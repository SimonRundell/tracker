namespace AtRiskTracker.Admin
{
    partial class UnitEditDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblCode    = new System.Windows.Forms.Label();
            this._txtCode    = new System.Windows.Forms.TextBox();
            this._lblName    = new System.Windows.Forms.Label();
            this._txtName    = new System.Windows.Forms.TextBox();
            this._lblCredits = new System.Windows.Forms.Label();
            this._numCredits = new System.Windows.Forms.NumericUpDown();
            this._lblGlh     = new System.Windows.Forms.Label();
            this._numGlh     = new System.Windows.Forms.NumericUpDown();
            this._chkExternal = new System.Windows.Forms.CheckBox();
            this._btnOk      = new System.Windows.Forms.Button();
            this._btnCancel  = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._numCredits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numGlh)).BeginInit();
            this.SuspendLayout();
            //
            // _lblCode
            //
            this._lblCode.Location = new System.Drawing.Point(20, 15);
            this._lblCode.Name     = "_lblCode";
            this._lblCode.Size     = new System.Drawing.Size(340, 18);
            this._lblCode.Text     = "Unit Code";
            //
            // _txtCode
            //
            this._txtCode.Location = new System.Drawing.Point(20, 35);
            this._txtCode.Name     = "_txtCode";
            this._txtCode.Size     = new System.Drawing.Size(340, 24);
            //
            // _lblName
            //
            this._lblName.Location = new System.Drawing.Point(20, 67);
            this._lblName.Name     = "_lblName";
            this._lblName.Size     = new System.Drawing.Size(340, 18);
            this._lblName.Text     = "Unit Name";
            //
            // _txtName
            //
            this._txtName.Location = new System.Drawing.Point(20, 87);
            this._txtName.Name     = "_txtName";
            this._txtName.Size     = new System.Drawing.Size(340, 24);
            //
            // _lblCredits
            //
            this._lblCredits.Location = new System.Drawing.Point(20, 119);
            this._lblCredits.Name     = "_lblCredits";
            this._lblCredits.Size     = new System.Drawing.Size(160, 18);
            this._lblCredits.Text     = "Credits";
            //
            // _numCredits
            //
            this._numCredits.Location = new System.Drawing.Point(20, 137);
            this._numCredits.Maximum  = 500M;
            this._numCredits.Name     = "_numCredits";
            this._numCredits.Size     = new System.Drawing.Size(160, 24);
            //
            // _lblGlh
            //
            this._lblGlh.Location = new System.Drawing.Point(200, 119);
            this._lblGlh.Name     = "_lblGlh";
            this._lblGlh.Size     = new System.Drawing.Size(160, 18);
            this._lblGlh.Text     = "GLH";
            //
            // _numGlh
            //
            this._numGlh.Location = new System.Drawing.Point(200, 137);
            this._numGlh.Maximum  = 1000M;
            this._numGlh.Name     = "_numGlh";
            this._numGlh.Size     = new System.Drawing.Size(160, 24);
            //
            // _chkExternal
            //
            this._chkExternal.Location = new System.Drawing.Point(20, 169);
            this._chkExternal.Name     = "_chkExternal";
            this._chkExternal.Size     = new System.Drawing.Size(200, 22);
            this._chkExternal.Text     = "External Unit";
            //
            // _btnOk
            //
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.Location  = new System.Drawing.Point(20, 201);
            this._btnOk.Name      = "_btnOk";
            this._btnOk.Size      = new System.Drawing.Size(160, 28);
            this._btnOk.Text      = "OK";
            this._btnOk.UseVisualStyleBackColor   = false;
            this._btnOk.FlatAppearance.BorderSize = 0;
            this._btnOk.DialogResult              = System.Windows.Forms.DialogResult.OK;
            //
            // _btnCancel
            //
            this._btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Location     = new System.Drawing.Point(195, 201);
            this._btnCancel.Name         = "_btnCancel";
            this._btnCancel.Size         = new System.Drawing.Size(160, 28);
            this._btnCancel.Text         = "Cancel";
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //
            // UnitEditDialog
            //
            this.AcceptButton        = this._btnOk;
            this.CancelButton        = this._btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(382, 247);
            this.Controls.Add(this._lblCode);
            this.Controls.Add(this._txtCode);
            this.Controls.Add(this._lblName);
            this.Controls.Add(this._txtName);
            this.Controls.Add(this._lblCredits);
            this.Controls.Add(this._numCredits);
            this.Controls.Add(this._lblGlh);
            this.Controls.Add(this._numGlh);
            this.Controls.Add(this._chkExternal);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._btnCancel);
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "UnitEditDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Add Unit";
            ((System.ComponentModel.ISupportInitialize)(this._numCredits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numGlh)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label         _lblCode;
        private System.Windows.Forms.TextBox       _txtCode;
        private System.Windows.Forms.Label         _lblName;
        private System.Windows.Forms.TextBox       _txtName;
        private System.Windows.Forms.Label         _lblCredits;
        private System.Windows.Forms.NumericUpDown _numCredits;
        private System.Windows.Forms.Label         _lblGlh;
        private System.Windows.Forms.NumericUpDown _numGlh;
        private System.Windows.Forms.CheckBox      _chkExternal;
        private System.Windows.Forms.Button        _btnOk;
        private System.Windows.Forms.Button        _btnCancel;
    }
}
