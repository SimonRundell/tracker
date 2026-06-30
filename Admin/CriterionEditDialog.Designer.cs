namespace AtRiskTracker.Admin
{
    partial class CriterionEditDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblCode   = new System.Windows.Forms.Label();
            this._txtCode   = new System.Windows.Forms.TextBox();
            this._lblDesc   = new System.Windows.Forms.Label();
            this._txtDesc   = new System.Windows.Forms.TextBox();
            this._lblOrder  = new System.Windows.Forms.Label();
            this._numOrder  = new System.Windows.Forms.NumericUpDown();
            this._btnOk     = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._numOrder)).BeginInit();
            this.SuspendLayout();
            //
            // _lblCode
            //
            this._lblCode.Location = new System.Drawing.Point(20, 15);
            this._lblCode.Name     = "_lblCode";
            this._lblCode.Size     = new System.Drawing.Size(420, 18);
            this._lblCode.Text     = "Code (e.g. 1.1 or P)";
            //
            // _txtCode
            //
            this._txtCode.Location = new System.Drawing.Point(20, 33);
            this._txtCode.Name     = "_txtCode";
            this._txtCode.Size     = new System.Drawing.Size(420, 24);
            //
            // _lblDesc
            //
            this._lblDesc.Location = new System.Drawing.Point(20, 65);
            this._lblDesc.Name     = "_lblDesc";
            this._lblDesc.Size     = new System.Drawing.Size(420, 18);
            this._lblDesc.Text     = "Description";
            //
            // _txtDesc
            //
            this._txtDesc.Location   = new System.Drawing.Point(20, 83);
            this._txtDesc.Multiline  = true;
            this._txtDesc.Name       = "_txtDesc";
            this._txtDesc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtDesc.Size       = new System.Drawing.Size(420, 56);
            //
            // _lblOrder
            //
            this._lblOrder.Location = new System.Drawing.Point(20, 147);
            this._lblOrder.Name     = "_lblOrder";
            this._lblOrder.Size     = new System.Drawing.Size(200, 18);
            this._lblOrder.Text     = "Sort Order";
            //
            // _numOrder
            //
            this._numOrder.Location = new System.Drawing.Point(20, 165);
            this._numOrder.Maximum  = 999M;
            this._numOrder.Name     = "_numOrder";
            this._numOrder.Size     = new System.Drawing.Size(80, 24);
            //
            // _btnOk
            //
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.Location  = new System.Drawing.Point(20, 201);
            this._btnOk.Name      = "_btnOk";
            this._btnOk.Size      = new System.Drawing.Size(205, 28);
            this._btnOk.Text      = "OK";
            this._btnOk.UseVisualStyleBackColor   = false;
            this._btnOk.FlatAppearance.BorderSize = 0;
            this._btnOk.DialogResult              = System.Windows.Forms.DialogResult.OK;
            //
            // _btnCancel
            //
            this._btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Location     = new System.Drawing.Point(235, 201);
            this._btnCancel.Name         = "_btnCancel";
            this._btnCancel.Size         = new System.Drawing.Size(205, 28);
            this._btnCancel.Text         = "Cancel";
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //
            // CriterionEditDialog
            //
            this.AcceptButton        = this._btnOk;
            this.CancelButton        = this._btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(462, 247);
            this.Controls.Add(this._lblCode);
            this.Controls.Add(this._txtCode);
            this.Controls.Add(this._lblDesc);
            this.Controls.Add(this._txtDesc);
            this.Controls.Add(this._lblOrder);
            this.Controls.Add(this._numOrder);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._btnCancel);
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "CriterionEditDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Add Criterion";
            ((System.ComponentModel.ISupportInitialize)(this._numOrder)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label         _lblCode;
        private System.Windows.Forms.TextBox       _txtCode;
        private System.Windows.Forms.Label         _lblDesc;
        private System.Windows.Forms.TextBox       _txtDesc;
        private System.Windows.Forms.Label         _lblOrder;
        private System.Windows.Forms.NumericUpDown _numOrder;
        private System.Windows.Forms.Button        _btnOk;
        private System.Windows.Forms.Button        _btnCancel;
    }
}
