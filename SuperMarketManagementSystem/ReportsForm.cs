using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SupermarketManagementSystem.Data;
using System.Data.SQLite;
using SupermarketManagementSystem.Helpers;

namespace SuperMarketManagementSystem
{
    public partial class ReportsForm : Form
    {
        public ReportsForm()
        {
            InitializeComponent();

            dtpFrom.Value = DateTime.Today;
            dtpTo.Value= DateTime.Today;

            LoadReport();
           
            

            LanguageManager.LanguageChanged += UpdateLanguage;
            UpdateLanguage();

            this.WindowState = FormWindowState.Maximized;
        }

        private void ReportsForm_Load(object sender, EventArgs e)
        {

        }

        private void LoadReport()
        {
            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT
                s.SaleID,
                IFNULL(c.CustomerName, 'Walk-in Customer') AS Customer,
                IFNULL(u.Username, 'N/A') AS UserName,
                s.SaleDate,
                s.TotalAmount
            FROM Sales s
            LEFT JOIN Customers c
                ON s.CustomerID = c.CustomerID
            LEFT JOIN Users u
                ON s.UserID = u.UserID
            WHERE datetime(s.SaleDate) >= datetime(@FromDate)
              AND datetime(s.SaleDate) < datetime(@ToDate)
            ORDER BY datetime(s.SaleDate) DESC";

                using (SQLiteDataAdapter adapter =
                       new SQLiteDataAdapter(query, con))
                {
                    adapter.SelectCommand.Parameters.AddWithValue(
                        "@FromDate",
                        dtpFrom.Value.Date.ToString("yyyy-MM-dd"));

                    adapter.SelectCommand.Parameters.AddWithValue(
                        "@ToDate",
                        dtpTo.Value.Date.AddDays(1).ToString("yyyy-MM-dd"));

                    DataTable table = new DataTable();

                    adapter.Fill(table);

                    dgvReport.DataSource = table;
                    FormatReportGrid();

                    CalculateTotal(table);
                }
            }
        }

        private void CalculateTotal(DataTable table)
        {
            decimal total = 0;

            foreach (DataRow row in table.Rows)
            {
                total += Convert.ToDecimal(row["TotalAmount"]);
            }

            lblTotalSales.Text =
                (LanguageManager.IsArabic
                    ? "إجمالي المبيعات: "
                    : "Total Sales: ")
                + total.ToString("0.00");
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {


            if (dtpFrom.Value.Date > dtpTo.Value.Date)
            {
                MessageBox.Show(
                    LanguageManager.IsArabic
                        ? "لا يمكن أن يكون تاريخ البداية بعد تاريخ النهاية."
                        : "From date cannot be greater than To date.",
                    LanguageManager.IsArabic
                        ? "تاريخ غير صالح"
                        : "Invalid Date",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            LoadReport();


        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UpdateLanguage()
        {
            bool ar = LanguageManager.IsArabic;

            Text = ar ? "تقارير المبيعات" : "Sales Reports";

            btnSearch.Text = ar ? "بحث" : "Search";
            btnClose.Text = ar ? "إغلاق" : "Close";
            lblBrandTitle.Text = ar ? "نظام إدارة سوبر ماركت" : "SUPERMARKET MANAGEMENT SYSTEM";
            label1.Text = ar ? "من" : "From";
            label2.Text = ar ? "الى" : "To";

            if (dgvReport.Columns.Contains("SaleID"))
                dgvReport.Columns["SaleID"].HeaderText =
                    ar ? "رقم البيع" : "Sale ID";

            if (dgvReport.Columns.Contains("Customer"))
                dgvReport.Columns["Customer"].HeaderText =
                    ar ? "العميل" : "Customer";

            if (dgvReport.Columns.Contains("UserName"))
                dgvReport.Columns["UserName"].HeaderText =
                    ar ? "اسم المستخدم" : "Username";

            if (dgvReport.Columns.Contains("SaleDate"))
                dgvReport.Columns["SaleDate"].HeaderText =
                    ar ? "تاريخ البيع" : "Sale Date";

            if (dgvReport.Columns.Contains("TotalAmount"))
                dgvReport.Columns["TotalAmount"].HeaderText =
                    ar ? "المبلغ الإجمالي" : "Total Amount";

            RightToLeft = ar ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = ar;

            if (dgvReport.DataSource is DataTable table)
                CalculateTotal(table);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LanguageManager.LanguageChanged -= UpdateLanguage;
            base.OnFormClosed(e);
        }

        private void FormatReportGrid()
        {
            // Prevent direct editing
            dgvReport.ReadOnly = true;
            dgvReport.AllowUserToAddRows = false;
            dgvReport.AllowUserToDeleteRows = false;

            // Row and header sizes
            dgvReport.RowTemplate.Height = 35;
            dgvReport.ColumnHeadersHeight = 40;
            dgvReport.AllowUserToResizeRows = false;
            dgvReport.AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode.None;

            // Font sizes
            dgvReport.DefaultCellStyle.Font =
                new Font("Segoe UI", 11);

            dgvReport.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 11, FontStyle.Bold);

            // Alignment
            dgvReport.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;

            dgvReport.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            // Column sizing
            dgvReport.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            if (dgvReport.Columns.Contains("SaleID"))
                dgvReport.Columns["SaleID"].FillWeight = 20;

            if (dgvReport.Columns.Contains("Customer"))
                dgvReport.Columns["Customer"].FillWeight = 45;

            if (dgvReport.Columns.Contains("UserName"))
                dgvReport.Columns["UserName"].FillWeight = 35;

            if (dgvReport.Columns.Contains("SaleDate"))
                dgvReport.Columns["SaleDate"].FillWeight = 45;

            if (dgvReport.Columns.Contains("TotalAmount"))
                dgvReport.Columns["TotalAmount"].FillWeight = 30;

            // Format total amounts to two decimal places
            if (dgvReport.Columns.Contains("TotalAmount"))
            {
                dgvReport.Columns["TotalAmount"].DefaultCellStyle.Format = "N2";

                dgvReport.Columns["TotalAmount"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }

            // Align sale IDs to the center
            if (dgvReport.Columns.Contains("SaleID"))
            {
                dgvReport.Columns["SaleID"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }

            // Select the entire sale row
            dgvReport.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvReport.MultiSelect = false;

            // Appearance
            dgvReport.BorderStyle = BorderStyle.None;
            dgvReport.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            dgvReport.EnableHeadersVisualStyles = false;
        }


    }
}
