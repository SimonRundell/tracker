/**
 * NotesDialog — view and edit freeform notes for a student.
 *
 * Notes are stored as simple HTML in the database (compatible with
 * the TipTap editor in the React web client).  This dialog strips
 * tags on load so the text is editable in a plain RichTextBox, and
 * wraps each paragraph back in <p> tags on save.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Services;

namespace AtRiskTracker.Dialogs
{
    public partial class NotesDialog : Form
    {
        private string _savedHtml = "";

        /// <summary>The saved HTML; only valid after DialogResult.OK.</summary>
        public string Notes => _savedHtml;

        private readonly StudentDto _student;

        public NotesDialog(StudentDto student)
        {
            _student = student;
            InitializeComponent();
            Text           = $"Notes — {_student.Firstname} {_student.Lastname}";
            _editor.Text   = HtmlToPlain(_student.Notes ?? "");
            _editor.Select(0, 0);
            _btnSave.FlatAppearance.BorderSize   = 0;
            _btnCancel.FlatAppearance.BorderSize = 0;
        }

        private async void OnSave(object sender, EventArgs e)
        {
            _savedHtml = PlainToHtml(_editor.Text);
            try
            {
                await ApiService.Instance.PutAsync<object>(
                    "/students/notes.php",
                    new { student_id = _student.Id, notes = _savedHtml });
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving notes: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ── HTML helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Converts simple stored HTML to plain text for display.
        /// Block-level closing tags become newlines; all other tags are stripped.
        /// </summary>
        private static string HtmlToPlain(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            // Block ends → newline
            string text = Regex.Replace(html, @"</p>|</li>|<br\s*/?>", "\n",
                RegexOptions.IgnoreCase);
            // Strip remaining tags
            text = Regex.Replace(text, @"<[^>]+>", "");
            // Decode common entities
            text = text.Replace("&amp;", "&")
                       .Replace("&lt;", "<")
                       .Replace("&gt;", ">")
                       .Replace("&nbsp;", " ")
                       .Replace("&quot;", "\"");
            return text.Trim();
        }

        /// <summary>
        /// Wraps each non-empty line of plain text in a &lt;p&gt; tag,
        /// matching TipTap's default HTML output.
        /// </summary>
        private static string PlainToHtml(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var sb = new System.Text.StringBuilder();
            foreach (string line in text.Split('\n'))
            {
                string trimmed = line.TrimEnd('\r');
                sb.Append("<p>");
                sb.Append(System.Net.WebUtility.HtmlEncode(trimmed));
                sb.Append("</p>");
            }
            return sb.ToString();
        }
    }
}
