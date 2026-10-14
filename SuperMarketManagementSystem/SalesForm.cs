using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Helpers;

namespace SuperMarketManagementSystem
{
    public partial class SalesForm : Form
    {
        private int userID;
        private decimal totalAmount = 0;
        public SalesForm(int userID)
        {
            InitializeComponent();
            this.userID = userID;

            LoadCustomers();
            LoadProducts();

            SetupCart();

            LanguageManager.LanguageChanged += UpdateLanguage;
            UpdateLanguage();

            this.WindowState = FormWindowState.Maximized;
        }

        private void SalesForm_Load(object sender, EventArgs e)
        {

        }

        private void LoadCustomers()
        {
            using (SqlConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT CustomerID, CustomerName
            FROM Customers
            ORDER BY CustomerName";

                using (SqlDataAdapter adapter =
                       new SqlDataAdapter(query, con))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    cmbCustomer.DataSource = table;
                    cmbCustomer.DisplayMember = "CustomerName";
                    cmbCustomer.ValueMember = "CustomerID";

                    cmbCustomer.SelectedIndex = -1;
                }
            }
        }

        private void LoadProducts()
        {
            using (SqlConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT ProductID, ProductName, Price, Quantity
            FROM Products
            WHERE Quantity > 0
            ORDER BY ProductName";

                using (SqlDataAdapter adapter =
                       new SqlDataAdapter(query, con))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    cmbProduct.DataSource = table;
                    cmbProduct.DisplayMember = "ProductName";
                    cmbProduct.ValueMember = "ProductID";

                    cmbProduct.SelectedIndex = -1;
                }
            }
        }

        private void SetupCart()
        {
            dgvCart.Columns.Clear();

            dgvCart.Columns.Add("ProductID", "Product ID");
            dgvCart.Columns.Add("ProductName", "Product");
            dgvCart.Columns.Add("Price", "Price");
            dgvCart.Columns.Add("Quantity", "Quantity");
            dgvCart.Columns.Add("Subtotal", "Subtotal");

            dgvCart.Columns["ProductID"].Visible = false;

            dgvCart.AllowUserToAddRows = false;
            dgvCart.ReadOnly = true;
            dgvCart.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
        }

        private void cmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedIndex == -1)
            {
                txtPrice.Text = "";
                return;
            }

            DataRowView row =
                cmbProduct.SelectedItem as DataRowView;

            if (row != null)
            {
                txtPrice.Text =
                    Convert.ToDecimal(row["Price"]).ToString("0.00");
            }

        }

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a product.");
                return;
            }

            int productID =
                Convert.ToInt32(cmbProduct.SelectedValue);

            DataRowView product =
                cmbProduct.SelectedItem as DataRowView;

            if (product == null)
                return;

            string productName =
                product["ProductName"].ToString();

            decimal price =
                Convert.ToDecimal(product["Price"]);

            int availableQuantity =
                Convert.ToInt32(product["Quantity"]);

            int quantity =
                Convert.ToInt32(nudQuantity.Value);

            if (quantity > availableQuantity)
            {
                MessageBox.Show(
                    "Not enough stock available.\n" +
                    "Available quantity: " +
                    availableQuantity);

                return;
            }

            decimal subtotal =
                price * quantity;

            dgvCart.Rows.Add(
                productID,
                productName,
                price,
                quantity,
                subtotal);

            CalculateTotal();

            cmbProduct.SelectedIndex = -1;
            nudQuantity.Value = 1;

        }

        private void CalculateTotal()
        {
            totalAmount = 0;

            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                if (row.IsNewRow)
                    continue;

                totalAmount +=
                    Convert.ToDecimal(
                        row.Cells["Subtotal"].Value);
            }

            lblTotal.Text =
                totalAmount.ToString("0.00");
        }

        private void btnClearCart_Click(object sender, EventArgs e)
        {
            dgvCart.Rows.Clear();

            totalAmount = 0;

            lblTotal.Text = "0.00";

        }

        private void btnCompleteSale_Click(object sender, EventArgs e)
        {

            // 1. Check if cart is empty
            if (dgvCart.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Please add at least one product to the cart.",
                    "Empty Cart",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // 2. Get selected customer
            int? customerID = null;

            if (cmbCustomer.SelectedIndex != -1)
            {
                customerID = Convert.ToInt32(cmbCustomer.SelectedValue);
            }

            try
            {
                using (SqlConnection con = Database.GetConnection())
                {
                    con.Open();

                    // Start transaction
                    SqlTransaction transaction = con.BeginTransaction();

                    try
                    {
                        // 3. Insert Sale
                        string saleQuery = @"
                    INSERT INTO Sales
                    (CustomerID, UserID, SaleDate, TotalAmount)
                    VALUES
                    (@CustomerID, @UserID, GETDATE(), @TotalAmount);

                    SELECT SCOPE_IDENTITY();";

                        int saleID;

                        using (SqlCommand cmd =
                               new SqlCommand(saleQuery, con, transaction))
                        {
                            cmd.Parameters.Add(
                                "@CustomerID",
                                SqlDbType.Int).Value =
                                customerID.HasValue
                                ? (object)customerID.Value
                                : DBNull.Value;

                            cmd.Parameters.Add(
                                "@UserID",
                                SqlDbType.Int).Value = userID;

                            cmd.Parameters.Add(
                                "@TotalAmount",
                                SqlDbType.Decimal).Value = totalAmount;

                            saleID = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 4. Insert Sale Details and update stock
                        foreach (DataGridViewRow row in dgvCart.Rows)
                        {
                            if (row.IsNewRow)
                                continue;

                            int productID =
                                Convert.ToInt32(
                                    row.Cells["ProductID"].Value);

                            int quantity =
                                Convert.ToInt32(
                                    row.Cells["Quantity"].Value);

                            decimal price =
                                Convert.ToDecimal(
                                    row.Cells["Price"].Value);

                            // Insert SaleDetail
                            string detailQuery = @"
                        INSERT INTO SaleDetails
                        (SaleID, ProductID, Quantity, Price)
                        VALUES
                        (@SaleID, @ProductID, @Quantity, @Price);";

                            using (SqlCommand cmd =
                                   new SqlCommand(
                                       detailQuery,
                                       con,
                                       transaction))
                            {
                                cmd.Parameters.Add(
                                    "@SaleID",
                                    SqlDbType.Int).Value = saleID;

                                cmd.Parameters.Add(
                                    "@ProductID",
                                    SqlDbType.Int).Value = productID;

                                cmd.Parameters.Add(
                                    "@Quantity",
                                    SqlDbType.Int).Value = quantity;

                                cmd.Parameters.Add(
                                    "@Price",
                                    SqlDbType.Decimal).Value = price;

                                cmd.ExecuteNonQuery();
                            }

                            // Update product stock
                            string stockQuery = @"
                        UPDATE Products
                        SET Quantity = Quantity - @Quantity
                        WHERE ProductID = @ProductID
                        AND Quantity >= @Quantity;"; using (SqlCommand cmd =
                           new SqlCommand(
                               stockQuery,
                               con,
                               transaction))
                            {
                                cmd.Parameters.Add(
                                    "@Quantity",
                                    SqlDbType.Int).Value = quantity;

                                cmd.Parameters.Add(
                                    "@ProductID",
                                    SqlDbType.Int).Value = productID;

                                int rowsAffected =
                                    cmd.ExecuteNonQuery();

                                if (rowsAffected == 0)
                                {
                                    throw new Exception(
                                        "Not enough stock for product ID: "
                                        + productID);
                                }
                            }
                        }

                        // 5. Commit transaction
                        transaction.Commit();

                        MessageBox.Show(
                            "Sale completed successfully.\nSale ID: "
                            + saleID,
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        // 6. Clear cart
                        dgvCart.Rows.Clear();

                        totalAmount = 0;
                        lblTotal.Text = "0.00";

                        // Reload products to show updated stock
                        LoadProducts();

                        cmbCustomer.SelectedIndex = -1;
                        txtPrice.Clear();
                        nudQuantity.Value = 1;
                    }
                    catch (Exception ex)
                    {
                        // Undo everything if an error occurs
                        transaction.Rollback();

                        MessageBox.Show(
                            "Sale could not be completed.\n\n"
                            + ex.Message,
                            "Sale Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Database error.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        
    }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cmbProduct_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                cmbCustomer.Focus();
            }
        }

        private void cmbCustomer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                nudQuantity.Focus();
            }
        }

        private void nudQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtPrice.Focus();
            }
        }

        private void txtPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnAddToCart.Focus();
            }
        }

        private void UpdateLanguage()
        {
            bool ar = LanguageManager.IsArabic;

            Text = ar ? "إدارة المبيعات" : "Sales Management";

            btnAddToCart.Text = ar ? "إضافة إلى السلة" : "Add to Cart";
            btnClearCart.Text = ar ? "مسح السلة" : "Clear Cart";
            btnCompleteSale.Text = ar ? "إتمام البيع" : "Complete Sale";
            lblBrandTitle.Text = ar ? "نظام إدارة سوبر ماركت" : "SUPERMARKET MANAGEMENT SYSTEM";
            label1.Text = ar ? "المنتج" : "Product";
            label2.Text = ar ? "العميل" : "Customer";
            label4.Text = ar ? "الكمية" : "Quantity";
            label3.Text = ar ? "السعر" : "Price";
            label5.Text = ar ? "الأجمالي" : "Total:";

            if (dgvCart.Columns.Contains("ProductID"))
                dgvCart.Columns["ProductID"].HeaderText =
                    ar ? "رقم المنتج" : "Product ID";

            if (dgvCart.Columns.Contains("ProductName"))
                dgvCart.Columns["ProductName"].HeaderText =
                    ar ? "المنتج" : "Product";

            if (dgvCart.Columns.Contains("Price"))
                dgvCart.Columns["Price"].HeaderText =
                    ar ? "السعر" : "Price";

            if (dgvCart.Columns.Contains("Quantity"))
                dgvCart.Columns["Quantity"].HeaderText =
                    ar ? "الكمية" : "Quantity";

            if (dgvCart.Columns.Contains("Subtotal"))
                dgvCart.Columns["Subtotal"].HeaderText =
                    ar ? "الإجمالي الفرعي" : "Subtotal";

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
