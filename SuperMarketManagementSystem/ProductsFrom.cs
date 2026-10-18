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
    public partial class ProductsFrom : Form
    {
        private int selectedProductID=0;
 

        public ProductsFrom()
        {
            InitializeComponent();

            LoadCategories();
            LoadProducts();

            LanguageManager.LanguageChanged += UpdateLanguage;
            UpdateLanguage();

            this.WindowState = FormWindowState.Maximized;
        }

        private void ProductsFrom_Load(object sender, EventArgs e)
        {

        }

        private void LoadCategories()
        {
            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT CategoryID, CategoryName
            FROM Categories
            ORDER BY CategoryName";

                using (SQLiteDataAdapter adapter =
                       new SQLiteDataAdapter(query, con))
                {
                    DataTable table = new DataTable();

                    adapter.Fill(table);

                    cmbCategory.DataSource = table;
                    cmbCategory.DisplayMember = "CategoryName";
                    cmbCategory.ValueMember = "CategoryID";
                    cmbCategory.SelectedIndex = -1;
                }
            }
        }

        private void LoadProducts()
        {
            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT
                p.ProductID,
                p.ProductName,
                p.Price,
                p.Quantity,
                p.CategoryID,
                c.CategoryName
            FROM Products p
            INNER JOIN Categories c
                ON p.CategoryID = c.CategoryID
            ORDER BY p.ProductID";

                using (SQLiteDataAdapter adapter =
                       new SQLiteDataAdapter(query, con))
                {
                    DataTable table = new DataTable();

                    adapter.Fill(table);

                    dgvProducts.DataSource = table;

                    dgvProducts.Columns["CategoryID"].Visible = false;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

            if (txtProductName.Text.Trim() == "")
            {
                MessageBox.Show("Please enter product name.");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Please enter a valid price.");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity))
            {
                MessageBox.Show("Please enter a valid quantity.");
                return;
            }

            if (cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a category.");
                return;
            }

            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            INSERT INTO Products
            (
                ProductName,
                Price,
                Quantity,
                CategoryID
            )
            VALUES
            (
                @ProductName,
                @Price,
                @Quantity,
                @CategoryID
            )";

                using (SQLiteCommand cmd =
                       new SQLiteCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ProductName",
                        DbType.String, 100).Value =
                        txtProductName.Text.Trim();

                    cmd.Parameters.Add(
                        "@Price",
                        DbType.Decimal).Value = price;

                    cmd.Parameters.Add(
                        "@Quantity",
                        DbType.Int32).Value = quantity;

                    cmd.Parameters.Add(
                        "@CategoryID",
                        DbType.Int32).Value =
                        cmbCategory.SelectedValue;

                    con.Open();

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show(
                "Product added successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LoadProducts();

            ClearFields();

        }
        private void ClearFields()
        {
            txtProductName.Clear();
            txtPrice.Clear();
            txtQuantity.Clear();

            cmbCategory.SelectedIndex = -1;

            selectedProductID = 0;

            dgvProducts.ClearSelection();

            txtProductName.Focus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            
            
                ClearFields();
            
        }

        

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedProductID == 0)
            {
                MessageBox.Show(
                    "Please select a product first.");

                return;
            }

            if (txtProductName.Text.Trim() == "")
            {
                MessageBox.Show(
                    "Please enter product name.");

                return;
            }

            if (!decimal.TryParse(
                txtPrice.Text,
                out decimal price))
            {
                MessageBox.Show(
                    "Please enter a valid price.");

                return;
            }

            if (!int.TryParse(
                txtQuantity.Text,
                out int quantity))
            {
                MessageBox.Show(
                    "Please enter a valid quantity.");

                return;
            }

            if (cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Please select a category.");

                return;
            }

            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            UPDATE Products
            SET
                ProductName = @ProductName,
                Price = @Price,
                Quantity = @Quantity,
                CategoryID = @CategoryID
            WHERE ProductID = @ProductID";

                using (SQLiteCommand cmd =
                       new SQLiteCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ProductName",
                        DbType.String, 100).Value =
                        txtProductName.Text.Trim();

                    cmd.Parameters.Add(
                        "@Price",
                        DbType.Decimal).Value = price;

                    cmd.Parameters.Add(
                        "@Quantity",
                        DbType.Int32).Value = quantity;

                    cmd.Parameters.Add(
                        "@CategoryID",
                        DbType.Int32).Value =
                        cmbCategory.SelectedValue;

                    cmd.Parameters.Add(
                        "@ProductID",
                        DbType.Int32).Value =
                        selectedProductID;

                    con.Open();

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show(
                "Product updated successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LoadProducts();

            ClearFields();

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductID == 0)
            {
                MessageBox.Show(
                    "Please select a product first.");

                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this product?",
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
                DELETE FROM Products
                WHERE ProductID = @ProductID";

                    using (SQLiteCommand cmd =
                           new SQLiteCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@ProductID",
                            DbType.Int32).Value =
                            selectedProductID;

                        con.Open();

                        int rowsAffected =
                            cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show(
                                "Product deleted successfully.",
                                "Success",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            LoadProducts();
                            ClearFields();
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(
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
                p.ProductID,
                p.ProductName,
                p.Price,
                p.Quantity,
                p.CategoryID,
                c.CategoryName
            FROM Products p
            INNER JOIN Categories c
                ON p.CategoryID = c.CategoryID
            WHERE p.ProductName LIKE @Search
            ORDER BY p.ProductID";

                using (SQLiteDataAdapter adapter =
                       new SQLiteDataAdapter(query, con))
                {
                    adapter.SelectCommand.Parameters.Add(
                        "@Search",
                        DbType.String, 100).Value =
                        "%" + txtSearch.Text.Trim() + "%";

                    DataTable table = new DataTable();

                    adapter.Fill(table);

                    dgvProducts.DataSource = table;

                    dgvProducts.Columns["CategoryID"].Visible = false;
                }
            }

        }

        private void txtProductName_KeyDown(object sender, KeyEventArgs e)
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
                cmbCategory.Focus();
            }
        }

        private void cmbCategory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtQuantity.Focus();
            }
        }

        private void txtQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnAdd.Focus();
            }
        }

        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
                if (e.RowIndex < 0)
                    return;

                DataGridViewRow row = dgvProducts.Rows[e.RowIndex];

                selectedProductID = Convert.ToInt32(
                    row.Cells["ProductID"].Value
                );

                txtProductName.Text = row.Cells["ProductName"].Value.ToString();
                txtPrice.Text = row.Cells["Price"].Value.ToString();
                txtQuantity.Text = row.Cells["Quantity"].Value.ToString();

                cmbCategory.SelectedValue = Convert.ToInt32(
                    row.Cells["CategoryID"].Value
                );
            }

        private void UpdateLanguage()
        {
            bool ar = LanguageManager.IsArabic;

            Text = ar ? "إدارة المنتجات" : "Products Management";

            btnAdd.Text = ar ? "إضافة" : "Add";
            btnUpdate.Text = ar ? "تعديل" : "Update";
            btnDelete.Text = ar ? "حذف" : "Delete";
            btnClear.Text = ar ? "مسح" : "Clear";
            label1.Text = ar ? "أسم المنتج " : "Product Name:";
            label2.Text = ar ? "السعر" : "Price:";
            label3.Text = ar ? "الكمية " : "Quantity:";
            label4.Text = ar ? "الصنف " : "Category:";
            label5.Text = ar ? "بحث " : "Search:";
            lblBrandTitle.Text = ar ? "نظام إدارة سوبر ماركت" : "SUPERMARKET MANAGEMENT SYSTEM";

            // Translate the DataGridView headers.
            if (dgvProducts.Columns.Contains("ProductID"))
                dgvProducts.Columns["ProductID"].HeaderText =
                    ar ? "رقم المنتج" : "Product ID";

            if (dgvProducts.Columns.Contains("ProductName"))
                dgvProducts.Columns["ProductName"].HeaderText =
                    ar ? "اسم المنتج" : "Product Name";

            if (dgvProducts.Columns.Contains("Price"))
                dgvProducts.Columns["Price"].HeaderText =
                    ar ? "السعر" : "Price";

            if (dgvProducts.Columns.Contains("Quantity"))
                dgvProducts.Columns["Quantity"].HeaderText =
                    ar ? "الكمية" : "Quantity";

            if (dgvProducts.Columns.Contains("CategoryName"))
                dgvProducts.Columns["CategoryName"].HeaderText =
                    ar ? "التصنيف" : "Category";

            RightToLeft = ar ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = ar;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LanguageManager.LanguageChanged -= UpdateLanguage;
            base.OnFormClosed(e);
        }

        private string TranslateCategory(string category)
        {
            if (!LanguageManager.IsArabic)
                return category;

            switch (category)
            {
                case "Drinks": return "مشروبات";
                case "Food": return "مواد غذائية";
                case "Cleaning": return "منظفات";
                case "Sweets": return "حلويات";
                default: return category;
            }
        }
    }
    }

