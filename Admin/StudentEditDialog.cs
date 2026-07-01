/**
 * StudentEditDialog — add/edit a student record.
 *
 * Includes CIS number, name, concern dropdown (loaded from API) and a notes text area.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Services;

namespace AtRiskTracker.Admin
{
    internal partial class StudentEditDialog : Form
    {
        // Concern to pre-select once the API data arrives
        private string _pendingConcern = "None";

        public StudentEditDialog() { InitializeComponent(); }

        public StudentEditDialog(StudentAdminDto s) : this()
        {
            if (s != null)
            {
                Text              = "Edit Student";
                _txtCis.Text      = s.Cisnumber ?? "";
                _txtFirst.Text    = s.Firstname  ?? "";
                _txtLast.Text     = s.Lastname   ?? "";
                _txtNotes.Text    = s.Notes      ?? "";
                _pendingConcern   = s.Concern    ?? "None";
            }

            Load += async (sender, e) => await LoadConcernsAsync();
        }

        /// <summary>Loads concern options from the API and selects the current one.</summary>
        private async Task LoadConcernsAsync()
        {
            try
            {
                var resp = await ApiService.Instance.GetAsync<ListResponse<ConcernDto>>("/concerns/index.php");
                _cboConcern.Items.Clear();
                _cboConcern.Items.Add("None");
                foreach (var c in resp?.Data ?? new List<ConcernDto>())
                    _cboConcern.Items.Add(c.Concern);
            }
            catch
            {
                if (_cboConcern.Items.Count == 0) _cboConcern.Items.Add("None");
            }

            _cboConcern.SelectedItem = _pendingConcern;
            if (_cboConcern.SelectedIndex < 0) _cboConcern.SelectedIndex = 0;
        }

        public object ToPayload(int id = 0) => new
        {
            id,
            cisnumber = _txtCis.Text.Trim(),
            firstname = _txtFirst.Text.Trim(),
            lastname  = _txtLast.Text.Trim(),
            concern   = _cboConcern.SelectedItem?.ToString() ?? "None",
            notes     = _txtNotes.Text,
        };
    }
}
