namespace AtRiskTracker.Controls
{
    partial class HtmlEditor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._toolbar = new System.Windows.Forms.ToolStrip();
            this._browser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            //
            // _toolbar
            //
            this._toolbar.Dock      = System.Windows.Forms.DockStyle.Top;
            this._toolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolbar.Name      = "_toolbar";
            //
            // _browser
            //
            this._browser.Dock                           = System.Windows.Forms.DockStyle.Fill;
            this._browser.IsWebBrowserContextMenuEnabled = false;
            this._browser.Name                           = "_browser";
            this._browser.ScriptErrorsSuppressed         = true;
            this._browser.WebBrowserShortcutsEnabled     = true;
            this._browser.DocumentCompleted             += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.OnDocumentCompleted);
            this._browser.Navigating                    += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.OnNavigating);
            //
            // HtmlEditor
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._browser);
            this.Controls.Add(this._toolbar);
            this.Name = "HtmlEditor";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ToolStrip  _toolbar;
        private System.Windows.Forms.WebBrowser _browser;
    }
}
