/**
 * NotesDialog — view and edit freeform notes for a student.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Services;

namespace AtRiskTracker.Dialogs
{
    public class NotesDialog : Form
    {
        private RichTextBox _txt;
        public  string Notes => _txt.Text;

        private readonly StudentDto _student;

        public NotesDialog(StudentDto student)
        {
            _student = student;
            BuildUi();
        }

        private void BuildUi()
        {
            Text            = $"Notes — {_student.Firstname} {_student.Lastname}";
            Size            = new Size(520, 400);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            Font            = new Font("Trebuchet MS", 9f);

            _txt = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Trebuchet MS", 10f),
                Text = _student.Notes ?? "",
            };

            var panel = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };
            var btnSave = new Button
            {
                Text = "Save", Width = 100, Height = 30, Left = 8, Top = 7,
                BackColor = Color.FromArgb(0, 70, 127), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += OnSave;

            var btnCancel = new Button
            {
                Text = "Cancel", Width = 100, Height = 30, Left = 118, Top = 7,
                FlatStyle = FlatStyle.Flat,
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            panel.Controls.AddRange(new Control[] { btnSave, btnCancel });
            Controls.Add(_txt);
            Controls.Add(panel);
        }

        private async void OnSave(object sender, EventArgs e)
        {
            try
            {
                await ApiService.Instance.PutAsync<object>(
                    $"/students/index.php?id={_student.Id}",
                    new { notes = _txt.Text });
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving notes: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
