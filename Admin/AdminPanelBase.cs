/**
 * AdminPanelBase — shared boilerplate for all admin CRUD panels.
 *
 * Provides: a DataGridView, toolbar buttons (Add / Edit / Delete / Refresh),
 * an error label and the Load event trigger. Subclasses implement:
 *   - DefineColumns()  — add columns to _grid
 *   - LoadDataAsync()  — populate _grid.Rows
 *   - AddItemAsync()   — open add dialog and POST
 *   - EditItemAsync(row) — open edit dialog and PUT
 *   - DeleteItemAsync(row) — DELETE
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AtRiskTracker.Admin
{
    public abstract class AdminPanelBase : UserControl
    {
        protected DataGridView _grid;
        protected Label        _lblError;
        protected Button       _btnAdd, _btnEdit, _btnDelete, _btnRefresh;

        protected AdminPanelBase()
        {
            Dock      = DockStyle.Fill;
            BackColor = Color.White;

            // Toolbar
            var toolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 40,
                Padding   = new Padding(4, 4, 4, 0),
                BackColor = Color.FromArgb(240, 244, 248),
            };

            _btnAdd     = MakeBtn("+ Add",    Color.FromArgb(0, 127, 0));
            _btnEdit    = MakeBtn("Edit",     Color.FromArgb(0, 70, 127));
            _btnDelete  = MakeBtn("Delete",   Color.FromArgb(180, 0, 0));
            _btnRefresh = MakeBtn("Refresh",  Color.FromArgb(80, 80, 80));

            _btnAdd.Left    = 4;
            _btnEdit.Left   = 110;
            _btnDelete.Left = 216;
            _btnRefresh.Left= 322;

            _btnAdd.Click    += async (s, e) => await SafeRunAsync(AddItemAsync);
            _btnEdit.Click   += async (s, e) => { if (SelectedRow != null) await SafeRunAsync(() => EditItemAsync(SelectedRow)); };
            _btnDelete.Click += async (s, e) => { if (SelectedRow != null && ConfirmDelete()) await SafeRunAsync(() => DeleteItemAsync(SelectedRow)); };
            _btnRefresh.Click+= async (s, e) => await SafeRunAsync(LoadDataAsync);

            toolbar.Controls.AddRange(new Control[] { _btnAdd, _btnEdit, _btnDelete, _btnRefresh });

            _lblError = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 22,
                ForeColor = Color.DarkRed,
                Font      = new Font("Trebuchet MS", 8.5f),
                Padding   = new Padding(4, 2, 0, 0),
            };

            _grid = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 70, 127),
                    ForeColor = Color.White,
                    Font      = new Font("Trebuchet MS", 8.5f, FontStyle.Bold),
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Trebuchet MS", 9f),
                    SelectionBackColor = Color.FromArgb(210, 230, 255),
                    SelectionForeColor = Color.Black,
                },
                RowTemplate = { Height = 24 },
            };
            _grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && SelectedRow != null)
                    _ = SafeRunAsync(() => EditItemAsync(SelectedRow));
            };

            DefineColumns();

            Controls.Add(_grid);
            Controls.Add(_lblError);
            Controls.Add(toolbar);

            Load += async (s, e) => await SafeRunAsync(LoadDataAsync);
        }

        // ----------------------------------------------------------------
        // Template methods — override in each admin panel
        // ----------------------------------------------------------------

        protected abstract void     DefineColumns();
        protected abstract Task     LoadDataAsync();
        protected abstract Task     AddItemAsync();
        protected abstract Task     EditItemAsync(DataGridViewRow row);
        protected abstract Task     DeleteItemAsync(DataGridViewRow row);

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        protected DataGridViewRow SelectedRow =>
            _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0] : null;

        protected void SetError(string msg) => _lblError.Text = msg ?? "";

        protected async Task SafeRunAsync(Func<Task> action)
        {
            SetError("");
            try    { await action(); }
            catch (Exception ex) { SetError(ex.Message); }
        }

        protected bool ConfirmDelete() =>
            MessageBox.Show("Delete this item?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

        protected void AddColText(string header, string name, float weight = 1)
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = header, Name = name, FillWeight = weight,
                SortMode   = DataGridViewColumnSortMode.NotSortable,
            });
        }

        protected DataGridViewRow AddRow(params object[] values)
        {
            int idx = _grid.Rows.Add(values);
            return _grid.Rows[idx];
        }

        private Button MakeBtn(string text, Color back)
        {
            var b = new Button
            {
                Text      = text,
                Width     = 100, Height = 30, Top = 4,
                BackColor = back, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Trebuchet MS", 8.5f),
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}
