namespace AtRiskTracker.Admin
{
    partial class CourseUnitEditDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblCourse  = new System.Windows.Forms.Label();
            this._cboCourse  = new System.Windows.Forms.ComboBox();
            this._lblUnit    = new System.Windows.Forms.Label();
            this._cboUnit    = new System.Windows.Forms.ComboBox();
            this._lblYear    = new System.Windows.Forms.Label();
            this._numYear    = new System.Windows.Forms.NumericUpDown();
            this._btnOk      = new System.Windows.Forms.Button();
            this._btnCancel  = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._numYear)).BeginInit();
            this.SuspendLayout();
            //
            // _lblCourse
            //
            this._lblCourse.Location = new System.Drawing.Point(20, 15);
            this._lblCourse.Name     = "_lblCourse";
            this._lblCourse.Size     = new System.Drawing.Size(340, 18);
            this._lblCourse.Text     = "Course";
            //
            // _cboCourse
            //
            this._cboCourse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboCourse.Location      = new System.Drawing.Point(20, 33);
            this._cboCourse.Name          = "_cboCourse";
            this._cboCourse.Size          = new System.Drawing.Size(340, 24);
            //
            // _lblUnit
            //
            this._lblUnit.Location = new System.Drawing.Point(20, 63);
            this._lblUnit.Name     = "_lblUnit";
            this._lblUnit.Size     = new System.Drawing.Size(340, 18);
            this._lblUnit.Text     = "Unit";
            //
            // _cboUnit
            //
            this._cboUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboUnit.Location      = new System.Drawing.Point(20, 81);
            this._cboUnit.Name          = "_cboUnit";
            this._cboUnit.Size          = new System.Drawing.Size(340, 24);
            //
            // _lblYear
            //
            this._lblYear.Location = new System.Drawing.Point(20, 111);
            this._lblYear.Name     = "_lblYear";
            this._lblYear.Size     = new System.Drawing.Size(340, 18);
            this._lblYear.Text     = "Year Taken";
            //
            // _numYear
            //
            this._numYear.Location = new System.Drawing.Point(20, 129);
            this._numYear.Minimum  = 1M;
            this._numYear.Maximum  = 3M;
            this._numYear.Value    = 1M;
            this._numYear.Name     = "_numYear";
            this._numYear.Size     = new System.Drawing.Size(100, 24);
            //
            // _btnOk
            //
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.Location  = new System.Drawing.Point(20, 163);
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
            this._btnCancel.Location     = new System.Drawing.Point(195, 163);
            this._btnCancel.Name         = "_btnCancel";
            this._btnCancel.Size         = new System.Drawing.Size(160, 28);
            this._btnCancel.Text         = "Cancel";
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //
            // CourseUnitEditDialog
            //
            this.AcceptButton        = this._btnOk;
            this.CancelButton        = this._btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(382, 209);
            this.Controls.Add(this._lblCourse);
            this.Controls.Add(this._cboCourse);
            this.Controls.Add(this._lblUnit);
            this.Controls.Add(this._cboUnit);
            this.Controls.Add(this._lblYear);
            this.Controls.Add(this._numYear);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._btnCancel);
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "CourseUnitEditDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Add Course Unit";
            ((System.ComponentModel.ISupportInitialize)(this._numYear)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label         _lblCourse;
        private System.Windows.Forms.ComboBox      _cboCourse;
        private System.Windows.Forms.Label         _lblUnit;
        private System.Windows.Forms.ComboBox      _cboUnit;
        private System.Windows.Forms.Label         _lblYear;
        private System.Windows.Forms.NumericUpDown _numYear;
        private System.Windows.Forms.Button        _btnOk;
        private System.Windows.Forms.Button        _btnCancel;
    }
}
