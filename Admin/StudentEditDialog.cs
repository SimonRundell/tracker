/**
 * StudentEditDialog — add/edit a student person record.
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
        private int? _pendingConcernId = null;

        /// <summary>The concern_id selected by the user; null means "None".</summary>
        public int? SelectedConcernId
        {
            get
            {
                if (_cboConcern.SelectedItem is ConcernDto c) return c.Id;
                return null;
            }
        }

        public StudentEditDialog() { InitializeComponent(); }

        public StudentEditDialog(StudentAdminDto s) : this()
        {
            if (s != null)
            {
                Text              = "Edit Student";
                _txtCis.Text      = s.Cisnumber ?? "";
                _txtFirst.Text    = s.Firstname  ?? "";
                _txtLast.Text     = s.Lastname   ?? "";
                _pendingConcernId = s.ConcernId;
            }

            Load += async (sender, e) => await LoadConcernsAsync();
        }

        private async Task LoadConcernsAsync()
        {
            try
            {
                var resp = await ApiService.Instance.GetAsync<ListResponse<ConcernDto>>("/concerns/index.php");
                _cboConcern.Items.Clear();
                _cboConcern.Items.Add(new ConcernDto { Id = 0, Concern = "None" });
                foreach (var c in resp?.Data ?? new List<ConcernDto>())
                    _cboConcern.Items.Add(c);

                _cboConcern.DisplayMember = "Concern";
            }
            catch
            {
                if (_cboConcern.Items.Count == 0)
                    _cboConcern.Items.Add(new ConcernDto { Id = 0, Concern = "None" });
            }

            // Select the matching concern by ID
            _cboConcern.SelectedIndex = 0;
            if (_pendingConcernId.HasValue && _pendingConcernId.Value > 0)
            {
                foreach (var item in _cboConcern.Items)
                {
                    if (item is ConcernDto c && c.Id == _pendingConcernId.Value)
                    {
                        _cboConcern.SelectedItem = c;
                        break;
                    }
                }
            }
        }

        public object ToPayload(int id = 0) => new
        {
            id,
            cisnumber = _txtCis.Text.Trim(),
            firstname = _txtFirst.Text.Trim(),
            lastname  = _txtLast.Text.Trim(),
        };
    }
}
