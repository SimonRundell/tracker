namespace AtRiskTracker.Admin
{
    partial class SectionEditDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblLabel  = new System.Windows.Forms.Label();
            this._txtLabel  = new System.Windows.Forms.TextBox();
            this._lblTitle  = new System.Windows.Forms.Label();
            this._txtTitle  = new System.Windows.Forms.TextBox();
            this._lblOrder  = new System.Windows.Forms.Label();
            this._numOrder  = new System.Windows.Forms.NumericUpDown();
            this._btnOk     = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._numOrder)).BeginInit();
            this.SuspendLayout();
            //
            // _lblLabel
            //
            this._lblLabel.Location = new System.Drawing.Point(20, 15);
            this._lblLabel.Name     = "_lblLabel";
            this._lblLabel.Size     = new System.Drawing.Size(380, 18);
            this._lblLabel.Text     = "Label (e.g. LO1 or P/M/D)";
            //
            // _txtLabel
            //
            this._txtLabel.Location = new System.Drawing.Point(20, 33);
            this._txtLabel.Name     = "_txtLabel";
            this._txtLabel.Size     = new System.Drawing.Size(380, 24);
            //
            // _lblTitle
            //
            this._lblTitle.Location = new System.Drawing.Point(20, 65);
            this._lblTitle.Name     = "_lblTitle";
            this._lblTitle.Size     = new System.Drawing.Size(380, 18);
            this._lblTitle.Text     = "Title";
            //
            // _txtTitle
            //
            this._txtTitle.Location = new System.Drawing.Point(20, 83);
            this._txtTitle.Name     = "_txtTitle";
            this._txtTitle.Size     = new System.Drawing.Size(380, 24);
            //
            // _lblOrder
            //
            this._lblOrder.Location = new System.Drawing.Point(20, 115);
            this._lblOrder.Name     = "_lblOrder";
            this._lblOrder.Size     = new System.Drawing.Size(200, 18);
            this._lblOrder.Text     = "Sort Order";
            //
            // _numOrder
            //
            this._numOrder.Location = new System.Drawing.Point(20, 133);
            this._numOrder.Maximum  = 999M;
            this._numOrder.Name     = "_numOrder";
            this._numOrder.Size     = new System.Drawing.Size(80, 24);
            //
            // _btnOk
            //
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.Location  = new System.Drawing.Point(20, 169);
            this._btnOk.Name      = "_btnOk";
            this._btnOk.Size      = new System.Drawing.Size(185, 28);
            this._btnOk.Text      = "OK";
            this._btnOk.UseVisualStyleBackColor   = false;
            this._btnOk.FlatAppearance.BorderSize = 0;
            this._btnOk.DialogResult              = System.Windows.Forms.DialogResult.OK;
            //
            // _btnCancel
            //
            this._btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Location     = new System.Drawing.Point(215, 169);
            this._btnCancel.Name         = "_btnCancel";
            this._btnCancel.Size         = new System.Drawing.Size(185, 28);
            this._btnCancel.Text         = "Cancel";
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //
            // SectionEditDialog
            //
            this.AcceptButton        = this._btnOk;
            this.CancelButton        = this._btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(422, 215);
            this.Controls.Add(this._lblLabel);
            this.Controls.Add(this._txtLabel);
            this.Controls.Add(this._lblTitle);
            this.Controls.Add(this._txtTitle);
            this.Controls.Add(this._lblOrder);
            this.Controls.Add(this._numOrder);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._btnCancel);
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "SectionEditDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Add Section";
            ((System.ComponentModel.ISupportInitialize)(this._numOrder)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label         _lblLabel;
        private System.Windows.Forms.TextBox       _txtLabel;
        private System.Windows.Forms.Label         _lblTitle;
        private System.Windows.Forms.TextBox       _txtTitle;
        private System.Windows.Forms.Label         _lblOrder;
        private System.Windows.Forms.NumericUpDown _numOrder;
        private System.Windows.Forms.Button        _btnOk;
        private System.Windows.Forms.Button        _btnCancel;
    }
}
