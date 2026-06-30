namespace AtRiskTracker.Admin
{
    partial class GroupEditDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblName   = new System.Windows.Forms.Label();
            this._txtName   = new System.Windows.Forms.TextBox();
            this._lblCourse = new System.Windows.Forms.Label();
            this._cboCourse = new System.Windows.Forms.ComboBox();
            this._btnOk     = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // _lblName
            //
            this._lblName.Location = new System.Drawing.Point(20, 15);
            this._lblName.Name     = "_lblName";
            this._lblName.Size     = new System.Drawing.Size(320, 18);
            this._lblName.Text     = "Group Name";
            //
            // _txtName
            //
            this._txtName.Location = new System.Drawing.Point(20, 33);
            this._txtName.Name     = "_txtName";
            this._txtName.Size     = new System.Drawing.Size(320, 24);
            //
            // _lblCourse
            //
            this._lblCourse.Location = new System.Drawing.Point(20, 65);
            this._lblCourse.Name     = "_lblCourse";
            this._lblCourse.Size     = new System.Drawing.Size(320, 18);
            this._lblCourse.Text     = "Course";
            //
            // _cboCourse
            //
            this._cboCourse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboCourse.Location      = new System.Drawing.Point(20, 83);
            this._cboCourse.Name          = "_cboCourse";
            this._cboCourse.Size          = new System.Drawing.Size(320, 24);
            //
            // _btnOk
            //
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.Location  = new System.Drawing.Point(20, 118);
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
            this._btnCancel.Location     = new System.Drawing.Point(185, 118);
            this._btnCancel.Name         = "_btnCancel";
            this._btnCancel.Size         = new System.Drawing.Size(150, 28);
            this._btnCancel.Text         = "Cancel";
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //
            // GroupEditDialog
            //
            this.AcceptButton        = this._btnOk;
            this.CancelButton        = this._btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(362, 164);
            this.Controls.Add(this._lblName);
            this.Controls.Add(this._txtName);
            this.Controls.Add(this._lblCourse);
            this.Controls.Add(this._cboCourse);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._btnCancel);
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "GroupEditDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Add Group";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label    _lblName;
        private System.Windows.Forms.TextBox  _txtName;
        private System.Windows.Forms.Label    _lblCourse;
        private System.Windows.Forms.ComboBox _cboCourse;
        private System.Windows.Forms.Button   _btnOk;
        private System.Windows.Forms.Button   _btnCancel;
    }
}
