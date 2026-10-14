using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using SupermarketManagementSystem.Data;
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
            using (SqlConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT
                s.SaleID,
                ISNULL(c.CustomerName, 'Walk-in Customer') AS Customer,
                u.Username AS UserName,
                s.SaleDate,
                s.TotalAmount
            FROM Sales s
            LEFT JOIN Customers c
                ON s.CustomerID = c.CustomerID
            INNER JOIN Users u
                ON s.UserID = u.UserID
            WHERE s.SaleDate >= @FromDate
              AND s.SaleDate < @ToDate
            ORDER BY s.SaleDate DESC";

                using (SqlDataAdapter adapter =
                       new SqlDataAdapter(query, con))
                {
                    adapter.SelectCommand.Parameters.Add(
                        "@FromDate",
                        SqlDbType.DateTime).Value =
                        dtpFrom.Value.Date;

                    adapter.SelectCommand.Parameters.Add(
                        "@ToDate",
                        SqlDbType.DateTime).Value =
                        dtpTo.Value.Date.AddDays(1);

                    DataTable table = new DataTable();

                    adapter.Fill(table);

                    dgvReport.DataSource = table;

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


    }
}
