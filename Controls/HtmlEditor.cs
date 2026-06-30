/**
 * HtmlEditor — lightweight WYSIWYG HTML editor control.
 *
 * Uses the built-in WebBrowser / IE-MSHTML engine with a contenteditable body
 * so no additional NuGet packages or runtimes are required.  Output is an HTML
 * fragment compatible with TipTap (the React app's editor), allowing the two
 * clients to share the same backend notes field without conversion.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AtRiskTracker.Controls
{
    /// <summary>
    /// WinForms user control that hosts an HTML WYSIWYG editor via WebBrowser +
    /// contenteditable.  Set <see cref="Html"/> to load content; read it back to
    /// retrieve the edited HTML fragment.
    /// </summary>
    public partial class HtmlEditor : UserControl
    {
        private string _pendingHtml  = "";
        private bool   _documentReady;

        public HtmlEditor()
        {
            InitializeComponent();
            PopulateToolbar();
            LoadDocument("");
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Gets or sets the editor content as an HTML fragment.</summary>
        public string Html
        {
            get
            {
                if (!_documentReady || _browser.Document?.Body == null)
                    return _pendingHtml;
                return _browser.Document.Body.InnerHtml ?? "";
            }
            set
            {
                _pendingHtml = value ?? "";
                LoadDocument(_pendingHtml);
            }
        }

        // ── Toolbar population (called from constructor) ───────────────────────

        private void PopulateToolbar()
        {
            _toolbar.Items.Add(MakeFormatButton("B", "Bold (Ctrl+B)",      FontStyle.Bold,      "Bold"));
            _toolbar.Items.Add(MakeFormatButton("I", "Italic (Ctrl+I)",    FontStyle.Italic,    "Italic"));
            _toolbar.Items.Add(MakeFormatButton("U", "Underline (Ctrl+U)", FontStyle.Underline, "Underline"));
            _toolbar.Items.Add(new ToolStripSeparator());

            var btnBullet = new ToolStripButton { Text = "• List", ToolTipText = "Bullet list" };
            btnBullet.Click += (s, e) => ExecCmd("InsertUnorderedList");
            _toolbar.Items.Add(btnBullet);

            var btnNumbered = new ToolStripButton { Text = "1. List", ToolTipText = "Numbered list" };
            btnNumbered.Click += (s, e) => ExecCmd("InsertOrderedList");
            _toolbar.Items.Add(btnNumbered);

            _toolbar.Items.Add(new ToolStripSeparator());

            var cboStyle = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                ToolTipText   = "Paragraph style",
                Width         = 110,
            };
            cboStyle.Items.AddRange(new object[] { "Normal", "Heading 1", "Heading 2", "Heading 3" });
            cboStyle.SelectedIndex = 0;
            cboStyle.SelectedIndexChanged += (s, e) =>
            {
                string[] tags = { "p", "h1", "h2", "h3" };
                int idx = cboStyle.SelectedIndex;
                if (idx >= 0 && idx < tags.Length)
                    ExecCmd("FormatBlock", tags[idx]);
            };
            _toolbar.Items.Add(cboStyle);
        }

        private ToolStripButton MakeFormatButton(string label, string tip, FontStyle style, string command)
        {
            var btn = new ToolStripButton(label)
            {
                ToolTipText = tip,
                Font        = new Font("Trebuchet MS", 9f, style),
            };
            btn.Click += (s, e) => ExecCmd(command);
            return btn;
        }

        // ── Browser event handlers ─────────────────────────────────────────────

        private void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            _documentReady = true;
            _browser.Document?.Body?.Focus();
        }

        private void OnNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url?.AbsoluteUri;
            if (url != null && !url.StartsWith("about:"))
                e.Cancel = true;
        }

        // ── Internal helpers ───────────────────────────────────────────────────

        private void LoadDocument(string bodyHtml)
        {
            _documentReady        = false;
            _browser.DocumentText = BuildPageHtml(bodyHtml);
        }

        private void ExecCmd(string command, string value = null)
        {
            if (!_documentReady || _browser.Document == null) return;
            _browser.Document.ExecCommand(command, false, value);
            _browser.Focus();
        }

        /// <summary>Wraps a body HTML fragment in a minimal contenteditable page.</summary>
        private static string BuildPageHtml(string bodyContent) =>
            $@"<!DOCTYPE html>
<html>
<head>
<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
<style>
body {{ font-family: 'Trebuchet MS', sans-serif; font-size: 10pt; margin: 8px;
       line-height: 1.5; color: #1a1a1a; outline: none; }}
h1   {{ font-size: 14pt; margin: 6px 0 2px; }}
h2   {{ font-size: 12pt; margin: 5px 0 2px; }}
h3   {{ font-size: 11pt; margin: 4px 0 2px; }}
ul, ol {{ margin: 2px 0; padding-left: 20px; }}
p    {{ margin: 2px 0; }}
</style>
</head>
<body contenteditable=""true"">{bodyContent}</body>
</html>";
    }
}
