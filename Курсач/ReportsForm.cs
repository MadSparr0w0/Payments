using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace UtilityPaymentsManager
{
    public class ReportsForm : Form
    {
        private ComboBox cmbTables;
        private DataGridView dgvReport;
        private Button btnExport;

        public ReportsForm()
        {
            this.Text = "Генератор отчетов";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            cmbTables = new ComboBox
            {
                Location = new Point(20, 20),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTables.Items.AddRange(new[] { "payers", "addresses", "services", "tariffs", "contracts", "payments" });
            cmbTables.SelectedIndexChanged += CmbTables_SelectedIndexChanged;
            this.Controls.Add(cmbTables);

            btnExport = new Button
            {
                Text = "Экспорт в Excel",
                Location = new Point(240, 20),
                Size = new Size(120, 23)
            };
            btnExport.Click += BtnExport_Click;
            this.Controls.Add(btnExport);

            dgvReport = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(840, 480),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(dgvReport);
        }

        private void CmbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            string tableName = cmbTables.SelectedItem.ToString();
            try
            {
                string query = $"SELECT * FROM {tableName} ORDER BY 1";
                using (var conn = new NpgsqlConnection(DatabaseHelper.ConnectionString))
                {
                    var adapter = new NpgsqlDataAdapter(query, conn);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvReport.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvReport.DataSource == null)
            {
                MessageBox.Show("Сначала выберите таблицу", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Сохранить отчет",
                FileName = $"Отчет_{cmbTables.SelectedItem}_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Функция экспорта требует установки EPPlus", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}