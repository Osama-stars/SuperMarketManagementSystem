using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Helpers;

namespace SuperMarketManagementSystem
{
    public partial class CustomersForm : Form
    {
        private int selectedCustomerID = 0;
        private SQLiteCommand cmd;

        public CustomersForm()
        {
            InitializeComponent();
            LoadCustomers();

            LanguageManager.LanguageChanged += UpdateLanguage;
            UpdateLanguage();

            this.WindowState = FormWindowState.Maximized;
        }

        private void LoadCustomers()
        {
            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT
                CustomerID,
                CustomerName,
                Phone
            FROM Customers
            ORDER BY CustomerID";

                using (SQLiteDataAdapter adapter =
                       new SQLiteDataAdapter(query, con))
                {
                    DataTable table = new DataTable();

                    adapter.Fill(table);

                    dgvCustomers.DataSource = table;
                    FormatCustomersGrid();
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (txtCustomerName.Text.Trim() == "")
            {
                MessageBox.Show(
                    "Please enter customer name.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            INSERT INTO Customers
            (
                CustomerName,
                Phone
            )
            VALUES
            (
                @CustomerName,
                @Phone
            )";

                using (
                    cmd =
                       new SQLiteCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@CustomerName",
                        DbType.String, 100).Value =
                        txtCustomerName.Text.Trim();

                    cmd.Parameters.Add(
                        "@Phone",
                        DbType.String, 20).Value =
                        txtPhone.Text.Trim();

                    con.Open();

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show(
                "Customer added successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LoadCustomers();

            ClearFields();
        }

        private void ClearFields()
        {
            txtCustomerName.Clear();
            txtPhone.Clear();

            selectedCustomerID = 0;

            dgvCustomers.ClearSelection();

            txtCustomerName.Focus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void dgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row =
                dgvCustomers.Rows[e.RowIndex];

            selectedCustomerID =
                Convert.ToInt32(
                    row.Cells["CustomerID"].Value);

            txtCustomerName.Text =
                row.Cells["CustomerName"].Value.ToString();

            txtPhone.Text =
                row.Cells["Phone"].Value.ToString();

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedCustomerID == 0)
            {
                MessageBox.Show(
                    "Please select a customer first.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (txtCustomerName.Text.Trim() == "")
            {
                MessageBox.Show(
                    "Please enter customer name.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            UPDATE Customers
            SET
                CustomerName = @CustomerName,
                Phone = @Phone
            WHERE CustomerID = @CustomerID";

                using (SQLiteCommand cmd =
                       new SQLiteCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@CustomerName",
                        DbType.String, 100).Value =
                        txtCustomerName.Text.Trim();

                    cmd.Parameters.Add(
                        "@Phone",
                        DbType.String, 20).Value =
                        txtPhone.Text.Trim();

                    cmd.Parameters.Add(
                        "@CustomerID",
                        DbType.Int32).Value =
                        selectedCustomerID;

                    con.Open();

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show(
                "Customer updated successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LoadCustomers();

            ClearFields();

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedCustomerID == 0)
            {
                MessageBox.Show(
                    "Please select a customer first.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this customer?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SQLiteConnection con = Database.GetConnection())
                {
                    string query = @"
                DELETE FROM Customers
                WHERE CustomerID = @CustomerID";

                    using (SQLiteCommand cmd =
                           new SQLiteCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@CustomerID",
                            DbType.Int32).Value =
                            selectedCustomerID;

                        con.Open();

                        int rowsAffected =
                            cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show(
                                "Customer deleted successfully.",
                                "Success",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            LoadCustomers();

                            ClearFields();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Customer was not found.");
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(
                    "This customer may already have sales.\n\n" +
                    ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT
                CustomerID,
                CustomerName,
                Phone
            FROM Customers
            WHERE CustomerName LIKE @Search
               OR Phone LIKE @Search
            ORDER BY CustomerID";

                using (SQLiteDataAdapter adapter =
                       new SQLiteDataAdapter(query, con))
                {
                    adapter.SelectCommand.Parameters.Add(
                        "@Search",
                        DbType.String, 100).Value =
                        "%" + txtSearch.Text.Trim() + "%";

                    DataTable table = new DataTable();

                    adapter.Fill(table);

                    dgvCustomers.DataSource = table;
                }
            }

        }

        private void FormatCustomersGrid()
        {
            // Prevent direct editing
            dgvCustomers.ReadOnly = true;
            dgvCustomers.AllowUserToAddRows = false;
            dgvCustomers.AllowUserToDeleteRows = false;

            // Improve row and header sizes
            dgvCustomers.RowTemplate.Height = 35;
            dgvCustomers.ColumnHeadersHeight = 40;
            dgvCustomers.AllowUserToResizeRows = false;
            dgvCustomers.AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode.None;

            // Improve font readability
            dgvCustomers.DefaultCellStyle.Font =
                new Font("Segoe UI", 11);

            dgvCustomers.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 11, FontStyle.Bold);

            // Align text
            dgvCustomers.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;

            dgvCustomers.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            // Automatically use available width
            dgvCustomers.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            // Give the customer name more space
            if (dgvCustomers.Columns.Contains("CustomerName"))
            {
                dgvCustomers.Columns["CustomerName"].FillWeight = 70;
            }

            if (dgvCustomers.Columns.Contains("Phone"))
            {
                dgvCustomers.Columns["Phone"].FillWeight = 40;
            }

            // Hide the database ID
            if (dgvCustomers.Columns.Contains("CustomerID"))
            {
                dgvCustomers.Columns["CustomerID"].Visible = false;
            }

            // Selection behavior
            dgvCustomers.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvCustomers.MultiSelect = false;

            // Appearance
            dgvCustomers.BorderStyle = BorderStyle.None;
            dgvCustomers.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            dgvCustomers.EnableHeadersVisualStyles = false;
        }

        private void txtCustomerName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtPhone.Focus();
            }
        }

        private void txtPhone_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnAdd.Focus();
            }
        }

        private void UpdateLanguage()
        {
            bool ar = LanguageManager.IsArabic;

            Text = ar ? "إدارة العملاء" : "Customers Management";

            btnAdd.Text = ar ? "إضافة" : "Add";
            btnUpdate.Text = ar ? "تعديل" : "Update";
            btnDelete.Text = ar ? "حذف" : "Delete";
            btnClear.Text = ar ? "مسح" : "Clear";
            lblBrandTitle.Text = ar ? "نظام إدارة سوبر ماركت" : "SUPERMARKET MANAGEMENT SYSTEM";
            label2.Text = ar ? "أسم العميل" : "Customer Name";
            label1.Text = ar ? "الرقم" : "Phone";
            label3.Text = ar ? "بحث" : "Search";
            
            if (dgvCustomers.Columns.Contains("CustomerID"))
            {
                dgvCustomers.Columns["CustomerID"].HeaderText =
                    ar ? "رقم العميل" : "Customer ID";
            }

            if (dgvCustomers.Columns.Contains("CustomerName"))
            {
                dgvCustomers.Columns["CustomerName"].HeaderText =
                    ar ? "اسم العميل" : "Customer Name";
            }

            if (dgvCustomers.Columns.Contains("Phone"))
            {
                dgvCustomers.Columns["Phone"].HeaderText =
                    ar ? "رقم الهاتف" : "Phone";
            }

            RightToLeft = ar ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = ar;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LanguageManager.LanguageChanged -= UpdateLanguage;
            base.OnFormClosed(e);
        }
    }
}
