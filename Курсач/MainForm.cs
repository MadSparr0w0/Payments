using System;
using System.Drawing;
using System.Windows.Forms;

namespace UtilityPaymentsManager
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
            SetupDatabaseButtons();
            SetupReportButton();
        }

        private void InitializeUI()
        {
            this.Text = "Управление коммунальными платежами";
            this.ClientSize = new Size(800, 600);
            this.BackColor = Color.FromArgb(240, 245, 249);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            var lblTitle = new Label
            {
                Text = "Выберите таблицу",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Location = new Point(50, 30)
            };
            this.Controls.Add(lblTitle);

            var panel = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(700, 400),
                Location = new Point(50, 100)
            };
            this.Controls.Add(panel);
        }

        private void SetupDatabaseButtons()
        {
            string[] tables = { "Плательщики", "Адреса", "Услуги", "Тарифы", "Договоры", "Платежи" };
            string[] tableNames = { "payers", "addresses", "services", "tariffs", "contracts", "payments" };
            Color[] colors =
            {
                Color.FromArgb(231, 76, 60),
                Color.FromArgb(41, 128, 185),
                Color.FromArgb(39, 174, 96),
                Color.FromArgb(243, 156, 18),
                Color.FromArgb(142, 68, 173),
                Color.FromArgb(44, 62, 80)
            };

            for (int i = 0; i < tables.Length; i++)
            {
                var btn = new Button
                {
                    Text = tables[i],
                    Tag = tableNames[i],
                    Size = new Size(300, 60),
                    Location = new Point(50 + (i % 2) * 350, 50 + (i / 2) * 80),
                    BackColor = colors[i],
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };

                btn.FlatAppearance.BorderSize = 0;
                btn.Click += TableButton_Click;
                this.Controls[1].Controls.Add(btn);
            }
        }

        private void SetupReportButton()
        {
            var btnReports = new Button
            {
                Text = "Создать отчеты",
                Size = new Size(180, 40),
                Location = new Point(300, 520),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReports.FlatAppearance.BorderSize = 0;
            btnReports.Click += BtnReports_Click;
            this.Controls.Add(btnReports);

            var btnExit = new Button
            {
                Text = "Выход",
                Size = new Size(150, 40),
                Location = new Point(650, 520),
                BackColor = Color.FromArgb(189, 195, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) => this.Close();
            this.Controls.Add(btnExit);
        }

        private void TableButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string tableName = (string)button.Tag;

            using (var editorForm = new TableEditorForm(tableName, button.Text))
            {
                editorForm.ShowDialog();
            }
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            using (var reportForm = new ReportsForm())
            {
                reportForm.ShowDialog();
            }
        }
    }
}