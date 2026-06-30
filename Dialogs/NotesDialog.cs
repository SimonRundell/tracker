/**
 * NotesDialog — view and edit freeform rich-text notes for a student.
 *
 * Uses HtmlEditor (WebBrowser + contenteditable) to produce HTML fragments
 * compatible with the TipTap editor used by the React web client.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Services;

namespace AtRiskTracker.Dialogs
{
    public partial class NotesDialog : Form
    {
        private string _savedHtml = "";

        /// <summary>The saved HTML fragment; only valid after DialogResult.OK.</summary>
        public string Notes => _savedHtml;

        private readonly StudentDto _student;

        public NotesDialog(StudentDto student)
        {
            _student = student;
            InitializeComponent();
            Text          = $"Notes — {_student.Firstname} {_student.Lastname}";
            _editor.Html  = _student.Notes ?? "";
            _btnSave.FlatAppearance.BorderSize   = 0;
            _btnCancel.FlatAppearance.BorderSize = 0;
        }

        private async void OnSave(object sender, EventArgs e)
        {
            _savedHtml = _editor.Html;
            try
            {
                await ApiService.Instance.PutAsync<object>(
                    $"/students/index.php?id={_student.Id}",
                    new { notes = _savedHtml });
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
    }
}
