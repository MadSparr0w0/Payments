using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace UtilityPaymentsManager
{
    public partial class TableEditorForm : Form
    {
        private readonly string tableName;
        private DataTable dataTable;
        private DataGridView dataGridView;
        private Button btnSave;
        private Button btnRefresh;
        private Button btnDelete;
        private Button btnClose;
        private Label lblTitle;

        public TableEditorForm(string tableName, string formTitle)
        {
            this.tableName = tableName;
            this.Text = $"Редактор: {formTitle}";
            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            this.ClientSize = new Size(900, 650);
            this.BackColor = Color.FromArgb(240, 245, 249);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            lblTitle = new Label
            {
                Text = this.Text,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Location = new Point(30, 20)
            };
            this.Controls.Add(lblTitle);

            dataGridView = new DataGridView
            {
                Location = new Point(30, 60),
                Size = new Size(840, 500),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9),
                EnableHeadersVisualStyles = false,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(52, 73, 94),
                    SelectionBackColor = Color.FromArgb(41, 128, 185),
                    SelectionForeColor = Color.White,
                    Font = new Font("Segoe UI", 9)
                },
                RowHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(240, 245, 249)
                },
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                EditMode = DataGridViewEditMode.EditOnKeystroke,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                GridColor = Color.FromArgb(224, 224, 224)
            };
            this.Controls.Add(dataGridView);

            var panel = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(840, 60),
                Location = new Point(30, 570),
                Padding = new Padding(10)
            };
            this.Controls.Add(panel);

            btnDelete = CreateStyledButton("Удалить", Color.FromArgb(192, 57, 43), 0);
            btnSave = CreateStyledButton("Сохранить", Color.FromArgb(39, 174, 96), 130);
            btnRefresh = CreateStyledButton("Обновить", Color.FromArgb(41, 128, 185), 260);
            btnClose = CreateStyledButton("Закрыть", Color.FromArgb(149, 165, 166), 650);

            btnSave.Click += BtnSave_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClose.Click += BtnClose_Click;

            panel.Controls.Add(btnDelete);
            panel.Controls.Add(btnSave);
            panel.Controls.Add(btnRefresh);
            panel.Controls.Add(btnClose);
        }

        private Button CreateStyledButton(string text, Color backColor, int leftMargin)
        {
            return new Button
            {
                Text = text,
                Size = new Size(120, 40),
                Location = new Point(leftMargin, 10),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = {
                    BorderSize = 0,
                    MouseOverBackColor = Color.FromArgb(
                        Math.Max(backColor.R - 30, 0),
                        Math.Max(backColor.G - 30, 0),
                        Math.Max(backColor.B - 30, 0)),
                    MouseDownBackColor = Color.FromArgb(
                        Math.Max(backColor.R - 50, 0),
                        Math.Max(backColor.G - 50, 0),
                        Math.Max(backColor.B - 50, 0))
                }
            };
        }

        private void LoadData()
        {
            try
            {
                dataTable = DatabaseHelper.GetTableData(tableName);
                if (dataTable != null)
                {
                    dataGridView.DataSource = dataTable;
                    ConfigureGridColumns();
                    SetupForeignKeyColumns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                column.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                column.DefaultCellStyle.Padding = new Padding(3);

                if (column.ValueType == typeof(decimal) || column.ValueType == typeof(int))
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.Format = "N2";
                }

                if (column.Name.ToLower().Contains("id"))
                {
                    column.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                    column.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            }
        }

        private void SetupForeignKeyColumns()
        {
            switch (tableName.ToLower())
            {
                case "payments":
                    SetupComboBoxColumn("contract_id", "contracts", "contract_id", "contract_number");
                    SetupComboBoxColumn("service_id", "services", "service_id", "service_name");
                    break;

                case "tariffs":
                    SetupComboBoxColumn("service_id", "services", "service_id", "service_name");
                    break;
            }
        }

        private void SetupComboBoxColumn(string columnName, string foreignTable, string idColumn, string displayExpression)
        {
            if (dataGridView.Columns.Contains(columnName))
            {
                var column = dataGridView.Columns[columnName];
                var comboBoxColumn = new DataGridViewComboBoxColumn
                {
                    Name = column.Name + "_Combo",
                    HeaderText = column.HeaderText,
                    DataPropertyName = column.DataPropertyName,
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing,
                    FlatStyle = FlatStyle.Flat,
                    Width = column.Width
                };

                using (var connection = new NpgsqlConnection(DatabaseHelper.ConnectionString))
                {
                    connection.Open();
                    string query = $"SELECT {idColumn}, ({displayExpression}) AS display_value FROM {foreignTable} ORDER BY display_value";
                    var command = new NpgsqlCommand(query, connection);
                    var adapter = new NpgsqlDataAdapter(command);
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    comboBoxColumn.DataSource = dataTable;
                    comboBoxColumn.ValueMember = idColumn;
                    comboBoxColumn.DisplayMember = "display_value";
                }

                int columnIndex = column.Index;
                dataGridView.Columns.Remove(column);
                dataGridView.Columns.Insert(columnIndex, comboBoxColumn);

                comboBoxColumn.DefaultCellStyle.BackColor = Color.White;
                comboBoxColumn.DefaultCellStyle.ForeColor = Color.FromArgb(52, 73, 94);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView.EndEdit();

                DataTable changes = ((DataTable)dataGridView.DataSource).GetChanges();

                if (changes != null && changes.Rows.Count > 0)
                {
                    if (DatabaseHelper.SaveChanges(changes, tableName))
                    {
                        MessageBox.Show("Изменения успешно сохранены!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                }
                else
                {
                    MessageBox.Show("Нет изменений для сохранения.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var result = MessageBox.Show("Удалить выбранные записи?", "Подтверждение удаления",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        foreach (DataGridViewRow row in dataGridView.SelectedRows)
                        {
                            if (!row.IsNewRow)
                            {
                                dataGridView.Rows.Remove(row);
                            }
                        }

                        DataTable changes = ((DataTable)dataGridView.DataSource).GetChanges();
                        if (changes != null)
                        {
                            DatabaseHelper.SaveChanges(changes, tableName);
                            MessageBox.Show("Записи успешно удалены!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите записи для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dataTable?.Dispose();
                dataGridView?.Dispose();
                btnSave?.Dispose();
                btnRefresh?.Dispose();
                btnDelete?.Dispose();
                btnClose?.Dispose();
                lblTitle?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}