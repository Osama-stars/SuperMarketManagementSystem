using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Helpers;

namespace SuperMarketManagementSystem
{
    public partial class SalesForm : Form
    {

        private int userID;
        private decimal totalAmount = 0;
        private SQLiteDataAdapter adapter;
        private PrintDocument receiptDocument;
        private int lastSaleID = 0;
        private List<string> receiptLines = new List<string>();
        private int receiptLineIndex = 0;
        

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
            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT CustomerID, CustomerName
            FROM Customers
            ORDER BY CustomerName";

                using (
                    
                    adapter =
                       new SQLiteDataAdapter(query, con))
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
            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT ProductID, ProductName, Price, Quantity, Unit
            FROM Products
            WHERE Quantity > 0
            ORDER BY ProductName";

                using (SQLiteDataAdapter adapter =
                       new SQLiteDataAdapter(query, con))
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

            // Hide ProductID
            dgvCart.Columns["ProductID"].Visible = false;

            // Make the grid read-only
            dgvCart.AllowUserToAddRows = false;
            dgvCart.ReadOnly = true;
            dgvCart.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            // Improve readability
            dgvCart.RowTemplate.Height = 35;
            dgvCart.ColumnHeadersHeight = 40;

            dgvCart.DefaultCellStyle.Font =
                new Font("Segoe UI", 11);

            dgvCart.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 11, FontStyle.Bold);

            // Align text vertically in the middle
            dgvCart.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;

            dgvCart.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            // Format prices to two decimal places
            dgvCart.Columns["Price"].DefaultCellStyle.Format = "N2";
            dgvCart.Columns["Subtotal"].DefaultCellStyle.Format = "N2";

            dgvCart.Columns["Price"].DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

            dgvCart.Columns["Subtotal"].DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

            // Make Product wider than the other columns
            dgvCart.Columns["ProductName"].AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;

            dgvCart.Columns["Price"].Width = 100;
            dgvCart.Columns["Quantity"].Width = 100;
            dgvCart.Columns["Subtotal"].Width = 110;

            // Prevent users from manually resizing rows
            dgvCart.AllowUserToResizeRows = false;

            // Improve appearance
            dgvCart.BorderStyle = BorderStyle.None;
            dgvCart.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            dgvCart.EnableHeadersVisualStyles = false;

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

            string Unit =
                product["Unit"].ToString();

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
                using (SQLiteConnection con = Database.GetConnection())
                {
                    con.Open();

                    // Start transaction
                    SQLiteTransaction transaction = con.BeginTransaction();

                    try
                    {
                        // 3. Insert Sale
                        string saleQuery = @"
    INSERT INTO Sales
    (CustomerID, UserID, SaleDate, TotalAmount)
    VALUES
    (@CustomerID, @UserID, CURRENT_TIMESTAMP, @TotalAmount);

    SELECT last_insert_rowid();";

                        int saleID;

                        using (SQLiteCommand cmd =
                               new SQLiteCommand(saleQuery, con, transaction))
                        {
                            cmd.Parameters.Add(
                                "@CustomerID",
                                DbType.Int32).Value =
                                customerID.HasValue
                                ? (object)customerID.Value
                                : DBNull.Value;

                            cmd.Parameters.Add(
                                "@UserID",
                                DbType.Int32).Value = userID;

                            cmd.Parameters.Add(
                                "@TotalAmount",
                                DbType.Decimal).Value = totalAmount;

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

                            using (SQLiteCommand cmd =
                                   new SQLiteCommand(
                                       detailQuery,
                                       con,
                                       transaction))
                            {
                                cmd.Parameters.Add(
                                    "@SaleID",
                                    DbType.Int32).Value = saleID;

                                cmd.Parameters.Add(
                                    "@ProductID",
                                    DbType.Int32).Value = productID;

                                cmd.Parameters.Add(
                                    "@Quantity",
                                    DbType.Int32).Value = quantity;

                                cmd.Parameters.Add(
                                    "@Price",
                                    DbType.Decimal).Value = price;

                                cmd.ExecuteNonQuery();
                            }

                            // Update product stock
                            string stockQuery = @"
                        UPDATE Products
                        SET Quantity = Quantity - @Quantity
                        WHERE ProductID = @ProductID
                        AND Quantity >= @Quantity;"; using (SQLiteCommand cmd =
                           new SQLiteCommand(
                               stockQuery,
                               con,
                               transaction))
                            {
                                cmd.Parameters.Add(
                                    "@Quantity",
                                    DbType.Int32).Value = quantity;

                                cmd.Parameters.Add(
                                    "@ProductID",
                                    DbType.Int32).Value = productID;

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
                        lastSaleID = saleID;

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
            btnPrintBill.Text = ar ? "طباعة" : "Print Bill";

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

        private bool PrepareReceipt(int saleID)
        {
            receiptLines.Clear();

            using (SQLiteConnection con = Database.GetConnection())
            {
                con.Open();

                string saleQuery = @"
            SELECT Sales.SaleID,
                   Sales.SaleDate,
                   Sales.TotalAmount,
                   Customers.CustomerName
            FROM Sales
            LEFT JOIN Customers
                ON Sales.CustomerID = Customers.CustomerID
            WHERE Sales.SaleID = @SaleID;";

                using (SQLiteCommand cmd = new SQLiteCommand(saleQuery, con))
                {
                    cmd.Parameters.AddWithValue("@SaleID", saleID);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        receiptLines.Add("       SUPERMARKET");
                        receiptLines.Add("     MANAGEMENT SYSTEM");
                        receiptLines.Add("--------------------------------");
                        receiptLines.Add("Receipt No: " + reader["SaleID"]);
                        receiptLines.Add("Date: " + reader["SaleDate"]);
                        receiptLines.Add(
                            "Customer: " +
                            (reader["CustomerName"] == DBNull.Value
                                ? "Walk-in Customer"
                                : reader["CustomerName"].ToString()));

                        receiptLines.Add("--------------------------------");
                        receiptLines.Add("Item");
                        receiptLines.Add("Qty     Price       Subtotal");
                        receiptLines.Add("--------------------------------");
                    }
                }

                string detailsQuery = @"
            SELECT Products.ProductName,
                   SaleDetails.Quantity,
                   SaleDetails.Price,
                   Products.Unit
            FROM SaleDetails
            INNER JOIN Products
                ON SaleDetails.ProductID = Products.ProductID
            WHERE SaleDetails.SaleID = @SaleID;";

                using (SQLiteCommand cmd = new SQLiteCommand(detailsQuery, con))
                {
                    cmd.Parameters.AddWithValue("@SaleID", saleID);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader["ProductName"].ToString();
                            int quantity = Convert.ToInt32(reader["Quantity"]);
                            decimal price = Convert.ToDecimal(reader["Price"]);
                            string unit = reader["Unit"] == DBNull.Value
                                ? ""
                                : reader["Unit"].ToString();

                            decimal subtotal = quantity * price;

                            receiptLines.Add(name);
                            receiptLines.Add(
                                $"{quantity} {unit}  x  {price:0.00}  =  {subtotal:0.00}");
                        }
                    }
                }

                string totalQuery =
                    "SELECT TotalAmount FROM Sales WHERE SaleID = @SaleID;";

                using (SQLiteCommand cmd = new SQLiteCommand(totalQuery, con))
                {
                    cmd.Parameters.AddWithValue("@SaleID", saleID);

                    decimal total = Convert.ToDecimal(cmd.ExecuteScalar());

                    receiptLines.Add("--------------------------------");
                    receiptLines.Add($"TOTAL: {total:0.00}");
                    receiptLines.Add("--------------------------------");
                    receiptLines.Add("     Thank you for shopping!");
                    receiptLines.Add("          Visit again");
                }
            }

            return true;
        }

        private void ReceiptDocument_PrintPage(
    object sender,
    PrintPageEventArgs e)
        {
            using (Font font = new Font("Consolas", 9))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                float y = 20;
                float lineHeight = font.GetHeight(e.Graphics) + 4;

                while (receiptLineIndex < receiptLines.Count)
                {
                    if (y + lineHeight > e.MarginBounds.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    e.Graphics.DrawString(
                        receiptLines[receiptLineIndex],
                        font,
                        brush,
                        e.MarginBounds.Left,
                        y);

                    y += lineHeight;
                    receiptLineIndex++;
                }
            }

            e.HasMorePages = false;
        }

        private void btnPrintBill_Click(object sender, EventArgs e)
        {

            if (lastSaleID <= 0)
            {
                MessageBox.Show(
                    "Please complete a sale before printing a receipt.",
                    "No Receipt",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                if (!PrepareReceipt(lastSaleID))
                {
                    MessageBox.Show("The sale receipt could not be found.");
                    return;
                }

                using (PrintDialog dialog = new PrintDialog())
                using (PrintDocument document = new PrintDocument())
                {
                    receiptDocument = document;
                    receiptDocument.PrintPage += ReceiptDocument_PrintPage;

                    dialog.Document = receiptDocument;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        receiptLineIndex = 0;
                        receiptDocument.Print();
                    }

                    receiptDocument.PrintPage -= ReceiptDocument_PrintPage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not print the receipt.\n\n" + ex.Message,
                    "Printing Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

        }
    }
}
