namespace AtRiskTracker.Admin
{
    partial class CourseEditDialog
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
            this._lblQualType = new System.Windows.Forms.Label();
            this._cbqQualType = new System.Windows.Forms.ComboBox();
            this._lblPass    = new System.Windows.Forms.Label();
            this._numPass    = new System.Windows.Forms.NumericUpDown();
            this._lblMerit   = new System.Windows.Forms.Label();
            this._numMerit   = new System.Windows.Forms.NumericUpDown();
            this._lblDist    = new System.Windows.Forms.Label();
            this._numDist    = new System.Windows.Forms.NumericUpDown();
            this._btnOk      = new System.Windows.Forms.Button();
            this._btnCancel  = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._numPass)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numMerit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numDist)).BeginInit();
            this.SuspendLayout();
            //
            // _lblName
            //
            this._lblName.Location = new System.Drawing.Point(20, 15);
            this._lblName.Name     = "_lblName";
            this._lblName.Size     = new System.Drawing.Size(360, 18);
            this._lblName.Text     = "Course Name";
            //
            // _txtName
            //
            this._txtName.Location = new System.Drawing.Point(20, 35);
            this._txtName.Name     = "_txtName";
            this._txtName.Size     = new System.Drawing.Size(360, 24);
            //
            // _lblQualType
            //
            this._lblQualType.Location = new System.Drawing.Point(20, 67);
            this._lblQualType.Name     = "_lblQualType";
            this._lblQualType.Size     = new System.Drawing.Size(360, 18);
            this._lblQualType.Text     = "Qual Type";
            //
            // _cbqQualType
            //
            this._cbqQualType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cbqQualType.Location      = new System.Drawing.Point(20, 87);
            this._cbqQualType.Name          = "_cbqQualType";
            this._cbqQualType.Size          = new System.Drawing.Size(360, 24);
            //
            // _lblPass
            //
            this._lblPass.Location = new System.Drawing.Point(20, 119);
            this._lblPass.Name     = "_lblPass";
            this._lblPass.Size     = new System.Drawing.Size(110, 18);
            this._lblPass.Text     = "Pass Pts";
            //
            // _numPass
            //
            this._numPass.Location = new System.Drawing.Point(20, 137);
            this._numPass.Maximum  = 9999M;
            this._numPass.Name     = "_numPass";
            this._numPass.Size     = new System.Drawing.Size(110, 24);
            //
            // _lblMerit
            //
            this._lblMerit.Location = new System.Drawing.Point(145, 119);
            this._lblMerit.Name     = "_lblMerit";
            this._lblMerit.Size     = new System.Drawing.Size(110, 18);
            this._lblMerit.Text     = "Merit Pts";
            //
            // _numMerit
            //
            this._numMerit.Location = new System.Drawing.Point(145, 137);
            this._numMerit.Maximum  = 9999M;
            this._numMerit.Name     = "_numMerit";
            this._numMerit.Size     = new System.Drawing.Size(110, 24);
            //
            // _lblDist
            //
            this._lblDist.Location = new System.Drawing.Point(270, 119);
            this._lblDist.Name     = "_lblDist";
            this._lblDist.Size     = new System.Drawing.Size(110, 18);
            this._lblDist.Text     = "Dist Pts";
            //
            // _numDist
            //
            this._numDist.Location = new System.Drawing.Point(270, 137);
            this._numDist.Maximum  = 9999M;
            this._numDist.Name     = "_numDist";
            this._numDist.Size     = new System.Drawing.Size(110, 24);
            //
            // _btnOk
            //
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.Location  = new System.Drawing.Point(20, 174);
            this._btnOk.Name      = "_btnOk";
            this._btnOk.Size      = new System.Drawing.Size(170, 30);
            this._btnOk.Text      = "OK";
            this._btnOk.UseVisualStyleBackColor   = false;
            this._btnOk.FlatAppearance.BorderSize = 0;
            this._btnOk.DialogResult              = System.Windows.Forms.DialogResult.OK;
            //
            // _btnCancel
            //
            this._btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Location     = new System.Drawing.Point(205, 174);
            this._btnCancel.Name         = "_btnCancel";
            this._btnCancel.Size         = new System.Drawing.Size(170, 30);
            this._btnCancel.Text         = "Cancel";
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //
            // CourseEditDialog
            //
            this.AcceptButton        = this._btnOk;
            this.CancelButton        = this._btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(402, 222);
            this.Controls.Add(this._lblName);
            this.Controls.Add(this._txtName);
            this.Controls.Add(this._lblQualType);
            this.Controls.Add(this._cbqQualType);
            this.Controls.Add(this._lblPass);
            this.Controls.Add(this._numPass);
            this.Controls.Add(this._lblMerit);
            this.Controls.Add(this._numMerit);
            this.Controls.Add(this._lblDist);
            this.Controls.Add(this._numDist);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._btnCancel);
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "CourseEditDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Add Course";
            ((System.ComponentModel.ISupportInitialize)(this._numPass)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numMerit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numDist)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label         _lblName;
        private System.Windows.Forms.TextBox       _txtName;
        private System.Windows.Forms.Label         _lblQualType;
        private System.Windows.Forms.ComboBox      _cbqQualType;
        private System.Windows.Forms.Label         _lblPass;
        private System.Windows.Forms.NumericUpDown _numPass;
        private System.Windows.Forms.Label         _lblMerit;
        private System.Windows.Forms.NumericUpDown _numMerit;
        private System.Windows.Forms.Label         _lblDist;
        private System.Windows.Forms.NumericUpDown _numDist;
        private System.Windows.Forms.Button        _btnOk;
        private System.Windows.Forms.Button        _btnCancel;
    }
}
