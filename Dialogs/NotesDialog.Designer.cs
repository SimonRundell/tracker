using AtRiskTracker.Controls;

namespace AtRiskTracker.Dialogs
{
    partial class NotesDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._editor    = new HtmlEditor();
            this._btnPanel  = new System.Windows.Forms.Panel();
            this._btnSave   = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // _editor
            //
            this._editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this._editor.Name = "_editor";
            //
            // _btnPanel
            //
            this._btnPanel.Controls.Add(this._btnSave);
            this._btnPanel.Controls.Add(this._btnCancel);
            this._btnPanel.Dock    = System.Windows.Forms.DockStyle.Bottom;
            this._btnPanel.Height  = 44;
            this._btnPanel.Name    = "_btnPanel";
            this._btnPanel.Padding = new System.Windows.Forms.Padding(8);
            //
            // _btnSave
            //
            this._btnSave.BackColor = System.Drawing.Color.FromArgb(0, 70, 127);
            this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSave.ForeColor = System.Drawing.Color.White;
            this._btnSave.Location  = new System.Drawing.Point(8, 7);
            this._btnSave.Name      = "_btnSave";
            this._btnSave.Size      = new System.Drawing.Size(100, 30);
            this._btnSave.Text      = "Save";
            this._btnSave.UseVisualStyleBackColor = false;
            this._btnSave.Click    += new System.EventHandler(this.OnSave);
            //
            // _btnCancel
            //
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Location  = new System.Drawing.Point(118, 7);
            this._btnCancel.Name      = "_btnCancel";
            this._btnCancel.Size      = new System.Drawing.Size(100, 30);
            this._btnCancel.Text      = "Cancel";
            this._btnCancel.Click    += new System.EventHandler(this.OnCancel);
            //
            // NotesDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(562, 402);
            this.Controls.Add(this._editor);
            this.Controls.Add(this._btnPanel);
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize     = new System.Drawing.Size(400, 300);
            this.Name            = "NotesDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Notes";
            this._btnPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private Controls.HtmlEditor            _editor;
        private System.Windows.Forms.Panel     _btnPanel;
        private System.Windows.Forms.Button    _btnSave;
        private System.Windows.Forms.Button    _btnCancel;
    }
}
