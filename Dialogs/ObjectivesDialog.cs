/**
 * ObjectivesDialog — per-student, per-unit grade/criteria dialog.
 *
 * For external BTec units: numeric mark entry with auto-derived grade.
 * For criteria units: loads criteria sections and evidence tick-boxes.
 * For all: exposes a GradePicker combo to manually override.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AtRiskTracker.Models;
using AtRiskTracker.Services;
using AtRiskTracker.Utils;
using Newtonsoft.Json;

namespace AtRiskTracker.Dialogs
{
    public class ObjectivesDialog : Form
    {
        // Output
        public string SelectedGrade { get; private set; }
        public int?   RawMark       { get; private set; }

        private readonly StudentDto _student;
        private readonly UnitDto    _unit;
        private readonly string     _qualType;

        private ComboBox  _cboGrade;
        private NumericUpDown _numMark;
        private Label     _lblDerived;
        private Label     _lblMarkGrade;
        private Button    _btnApply;
        private Panel     _criteriaPanel;
        private bool      _isExternal;

        // Criteria state: criteriaId -> achieved
        private Dictionary<int, bool> _evidenceMap = new Dictionary<int, bool>();
        private List<dynamic>         _sections     = new List<dynamic>();

        public ObjectivesDialog(StudentDto student, UnitDto unit, string currentGrade,
            int? rawMark, string qualType)
        {
            _student = student;
            _unit    = unit;
            _qualType= qualType ?? "";
            SelectedGrade = currentGrade ?? "NS";
            RawMark       = rawMark;

            // BTec units with is_external flag use mark entry; others use criteria tick-boxes
            _isExternal = unit.IsExternal != 0;

            BuildUi(currentGrade, rawMark);
        }

        private void BuildUi(string currentGrade, int? rawMark)
        {
            Text            = $"{_unit.Unitname} — {_student.Firstname} {_student.Lastname}";
            Size            = new Size(680, 600);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            Font            = new Font("Trebuchet MS", 9f);
            MinimumSize     = new Size(500, 400);

            var topBar = new Panel
            {
                Dock    = DockStyle.Top,
                Height  = _isExternal ? 100 : 60,
                Padding = new Padding(10),
            };

            int tx = 10;
            topBar.Controls.Add(new Label
            {
                Text   = $"Student: {_student.Firstname} {_student.Lastname}    Unit: {_unit.Unitcode}",
                Font   = new Font("Trebuchet MS", 10f, FontStyle.Bold),
                Bounds = new Rectangle(tx, 8, 600, 22),
            });

            if (_isExternal)
            {
                // Show grade boundaries
                string bounds = BuildBoundaryString();
                topBar.Controls.Add(new Label
                {
                    Text   = "Boundaries: " + bounds,
                    Bounds = new Rectangle(tx, 34, 600, 18),
                });

                topBar.Controls.Add(new Label
                    { Text = $"Mark (max {MaxMark()}):", Bounds = new Rectangle(tx, 58, 120, 22) });
                _numMark = new NumericUpDown
                {
                    Bounds   = new Rectangle(tx + 125, 56, 80, 24),
                    Minimum  = 0,
                    Maximum  = MaxMark(),
                    Value    = rawMark ?? 0,
                };
                _numMark.ValueChanged += OnMarkChanged;
                _lblMarkGrade = new Label
                {
                    Bounds = new Rectangle(tx + 215, 58, 120, 22),
                    Font   = new Font("Trebuchet MS", 9f, FontStyle.Bold),
                };
                UpdateMarkGradeLabel();
                topBar.Controls.AddRange(new Control[] { _numMark, _lblMarkGrade });
            }
            else
            {
                topBar.Controls.Add(new Label
                {
                    Text   = "Override grade:",
                    Bounds = new Rectangle(tx, 34, 110, 22),
                });
                _cboGrade = new ComboBox
                {
                    Bounds        = new Rectangle(tx + 115, 32, 100, 24),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                };
                foreach (string g in GradeCalc.GradeOptions) _cboGrade.Items.Add(g);
                _cboGrade.SelectedItem = currentGrade ?? "NS";

                _lblDerived = new Label
                {
                    Text   = "Criteria suggest: …",
                    Bounds = new Rectangle(tx + 230, 34, 300, 22),
                    ForeColor = Color.DarkBlue,
                };
                topBar.Controls.AddRange(new Control[] { _cboGrade, _lblDerived });
            }

            // Scrollable criteria area
            _criteriaPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                AutoScroll= true,
                Padding   = new Padding(10),
                BackColor = Color.White,
            };

            // Bottom button bar
            var btnBar = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };
            _btnApply = new Button
            {
                Text      = "Apply Grade",
                Width     = 120, Height = 30, Left = 8, Top = 7,
                BackColor = Color.FromArgb(0, 70, 127), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            _btnApply.FlatAppearance.BorderSize = 0;
            _btnApply.Click += OnApply;

            var btnClose = new Button
            {
                Text = "Close", Width = 100, Height = 30, Left = 138, Top = 7,
                FlatStyle = FlatStyle.Flat,
            };
            btnClose.Click += (s, e) => Close();
            btnBar.Controls.AddRange(new Control[] { _btnApply, btnClose });

            Controls.Add(_criteriaPanel);
            Controls.Add(topBar);
            Controls.Add(btnBar);

            if (!_isExternal)
                Load += async (s, e) => await LoadCriteriaAsync();
        }

        private void OnMarkChanged(object sender, EventArgs e) => UpdateMarkGradeLabel();

        private void UpdateMarkGradeLabel()
        {
            if (_lblMarkGrade == null || _numMark == null) return;
            int mark  = (int)_numMark.Value;
            string gr = GradeCalc.MarkToGrade(mark, _unit.Glh);
            _lblMarkGrade.Text = $"{mark} → {gr}";
        }

        private int MaxMark()
        {
            // External boundaries D mark is the max score
            var table = new Dictionary<int, int> { { 90, 24 }, { 120, 32 } };
            return table.ContainsKey(_unit.Glh) ? table[_unit.Glh] : 24;
        }

        private string BuildBoundaryString()
        {
            var b90  = new[] { ("NP",6), ("P",9),  ("M",15), ("D",24) };
            var b120 = new[] { ("NP",8), ("P",12), ("M",20), ("D",32) };
            var b = _unit.Glh == 120 ? b120 : b90;
            return string.Join("  ", b.Select(x => $"{x.Item1}: {x.Item2}+"));
        }

        private async System.Threading.Tasks.Task LoadCriteriaAsync()
        {
            try
            {
                var critResp = await ApiService.Instance.GetAsync<dynamic>(
                    "/criteria/index.php", $"unit_id={_unit.Id}");
                var evResp   = await ApiService.Instance.GetAsync<dynamic>(
                    "/evidence/index.php", $"student_id={_student.Id}&unit_id={_unit.Id}");

                string critJson = JsonConvert.SerializeObject(critResp);
                string evJson   = JsonConvert.SerializeObject(evResp);

                // Parse sections
                var critObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(critJson);
                var evObj   = JsonConvert.DeserializeObject<Dictionary<string, object>>(evJson);

                // Build evidence map: criteriaId -> achieved
                if (evObj != null && evObj.TryGetValue("data", out var evData))
                {
                    string evDataJson = JsonConvert.SerializeObject(evData);
                    var evList = JsonConvert.DeserializeObject<List<Dictionary<string,object>>>(evDataJson);
                    if (evList != null)
                        foreach (var ev in evList)
                            if (ev.TryGetValue("criteria_id", out var cid) &&
                                ev.TryGetValue("achieved", out var ach))
                            {
                                int id = Convert.ToInt32(cid);
                                bool achieved = Convert.ToBoolean(Convert.ToInt32(ach));
                                _evidenceMap[id] = achieved;
                            }
                }

                // Parse sections -> criteria
                List<CritSection> sections = new List<CritSection>();
                if (critObj != null && critObj.TryGetValue("data", out var secData))
                {
                    string secJson = JsonConvert.SerializeObject(secData);
                    var secList = JsonConvert.DeserializeObject<List<Dictionary<string,object>>>(secJson);
                    if (secList != null)
                    {
                        foreach (var sec in secList)
                        {
                            var cs = new CritSection();
                            cs.Label = sec.TryGetValue("section_label", out var sl) ? sl?.ToString() : "";
                            cs.Criteria = new List<CritItem>();
                            if (sec.TryGetValue("criteria", out var cr))
                            {
                                string crJson = JsonConvert.SerializeObject(cr);
                                var crList = JsonConvert.DeserializeObject<List<Dictionary<string,object>>>(crJson);
                                if (crList != null)
                                    foreach (var c in crList)
                                    {
                                        cs.Criteria.Add(new CritItem
                                        {
                                            Id          = Convert.ToInt32(c.TryGetValue("id", out var ci) ? ci : 0),
                                            Description = c.TryGetValue("description", out var cd) ? cd?.ToString() : "",
                                        });
                                    }
                            }
                            sections.Add(cs);
                        }
                    }
                }

                _sections = sections.Cast<dynamic>().ToList();
                RenderCriteria(sections);
                UpdateDerivedGrade();
            }
            catch (Exception ex)
            {
                _criteriaPanel.Controls.Add(new Label
                {
                    Text = "Error loading criteria: " + ex.Message,
                    ForeColor = Color.DarkRed,
                    AutoSize = true,
                });
            }
        }

        private void RenderCriteria(List<CritSection> sections)
        {
            _criteriaPanel.SuspendLayout();
            _criteriaPanel.Controls.Clear();
            int y = 5;

            foreach (var sec in sections)
            {
                var lblSec = new Label
                {
                    Text      = $"Section: {sec.Label}",
                    Font      = new Font("Trebuchet MS", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 70, 127),
                    Bounds    = new Rectangle(5, y, 600, 20),
                };
                _criteriaPanel.Controls.Add(lblSec);
                y += 24;

                foreach (var crit in sec.Criteria)
                {
                    int capturedId = crit.Id;
                    bool achieved  = _evidenceMap.TryGetValue(capturedId, out bool a) && a;

                    var chk = new CheckBox
                    {
                        Text    = crit.Description,
                        Checked = achieved,
                        Bounds  = new Rectangle(10, y, 600, 22),
                        AutoSize= false,
                    };
                    chk.CheckedChanged += async (s, e) =>
                    {
                        _evidenceMap[capturedId] = chk.Checked;
                        UpdateDerivedGrade();
                        try
                        {
                            await ApiService.Instance.PutAsync<object>("/evidence/update.php", new
                            {
                                student_id  = _student.Id,
                                criteria_id = capturedId,
                                achieved    = chk.Checked,
                            });
                        }
                        catch { /* non-fatal */ }
                    };
                    _criteriaPanel.Controls.Add(chk);
                    y += 24;
                }
                y += 4;
            }
            _criteriaPanel.ResumeLayout();
        }

        private void UpdateDerivedGrade()
        {
            if (_lblDerived == null) return;
            // Simple heuristic: if all criteria achieved → D, most → M, some → P, none → NS
            int total    = _evidenceMap.Count;
            int achieved = _evidenceMap.Values.Count(v => v);
            string grade = total == 0 ? "NS"
                         : achieved == total ? "D"
                         : (double)achieved / total >= 0.67 ? "M"
                         : achieved > 0 ? "P"
                         : "NS";
            _lblDerived.Text = $"Criteria suggest: {grade}";
        }

        private async void OnApply(object sender, EventArgs e)
        {
            _btnApply.Enabled = false;
            try
            {
                if (_isExternal)
                {
                    int mark  = (int)_numMark.Value;
                    string gr = GradeCalc.MarkToGrade(mark, _unit.Glh);
                    await ApiService.Instance.PutAsync<object>("/results/update.php", new
                    {
                        student_id = _student.Id,
                        unit_id    = _unit.Id,
                        result     = gr,
                        raw_mark   = mark,
                    });
                    SelectedGrade = gr;
                    RawMark       = mark;
                }
                else
                {
                    string grade = _cboGrade?.SelectedItem?.ToString() ?? "NS";
                    await ApiService.Instance.PutAsync<object>("/results/update.php", new
                    {
                        student_id = _student.Id,
                        unit_id    = _unit.Id,
                        result     = grade,
                    });
                    SelectedGrade = grade;
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying grade: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _btnApply.Enabled = true;
            }
        }

        // Simple nested types for criteria parsing
        private class CritSection
        {
            public string         Label    { get; set; }
            public List<CritItem> Criteria { get; set; }
        }
        private class CritItem
        {
            public int    Id          { get; set; }
            public string Description { get; set; }
        }
    }
}
