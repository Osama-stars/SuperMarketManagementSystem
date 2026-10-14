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
    public partial class CategoriesForm : Form
    {
        private int selectedCategoryID = 0;
        public CategoriesForm()
        {
            InitializeComponent();

            LoadCategories();

            LanguageManager.LanguageChanged += UpdateLanguage;
            UpdateLanguage();

            this.WindowState = FormWindowState.Maximized;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void LoadCategories()
        {
            using (SqlConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT CategoryID, CategoryName
            FROM Categories
            ORDER BY CategoryID";

                SqlDataAdapter adapter =
                    new SqlDataAdapter(query, con);

                DataTable table = new DataTable();

                adapter.Fill(table);

                dgvCategories.DataSource = table;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

            if (txtCategoryName.Text.Trim() == "")
            {
                MessageBox.Show(
                    "Please enter category name.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (SqlConnection con = Database.GetConnection())
            {
                string query = @"
            INSERT INTO Categories (CategoryName)
            VALUES (@CategoryName)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue(
                        "@CategoryName",
                        txtCategoryName.Text.Trim());

                    con.Open();

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show(
                "Category added successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LoadCategories();

            ClearFields();

        }

        private void ClearFields()
        {
            txtCategoryName.Clear();

            selectedCategoryID = 0;

            dgvCategories.ClearSelection();

            txtCategoryName.Focus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void dgvCategories_CellClick(
    object sender,
    DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = dgvCategories.Rows[e.RowIndex];

            selectedCategoryID = Convert.ToInt32(
                row.Cells["CategoryID"].Value);

            txtCategoryName.Text =
                row.Cells["CategoryName"].Value.ToString();
        }

        private void dgvCategories_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (selectedCategoryID == 0)
            {
                MessageBox.Show("Please select a category first.");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this category?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection con = Database.GetConnection())
                {
                    string query =
                        "DELETE FROM Categories WHERE CategoryID = @CategoryID";

                    using (SqlCommand cmd =
                           new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@CategoryID",
                            SqlDbType.Int).Value = selectedCategoryID;

                        con.Open();

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Category deleted successfully.");

                            LoadCategories();
                            ClearFields();
                        }
                        else
                        {
                            MessageBox.Show("Category was not found.");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Cannot delete this category.\n\n" +
                    "It may be used by one or more products.\n\n" +
                    ex.Message,
                    "Delete Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void txtCategoryName_TextChanged(object sender, EventArgs e)
        {

        }
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            using (SqlConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT CategoryID, CategoryName
            FROM Categories
            WHERE CategoryName LIKE @Search
            ORDER BY CategoryID";

                SqlDataAdapter adapter =
                    new SqlDataAdapter(query, con);

                adapter.SelectCommand.Parameters.AddWithValue(
                    "@Search",
                    "%" + txtSearch.Text.Trim() + "%");

                DataTable table = new DataTable();

                adapter.Fill(table);

                dgvCategories.DataSource = table;
            }
        }

        private void btnUpdate_Click_1(object sender, EventArgs e)
        {

            if (selectedCategoryID == 0)
            {
                MessageBox.Show(
                    "Please select a category first.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (txtCategoryName.Text.Trim() == "")
            {
                MessageBox.Show(
                    "Please enter category name.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (SqlConnection con = Database.GetConnection())
            {
                string query = @"
            UPDATE Categories
            SET CategoryName = @CategoryName
            WHERE CategoryID = @CategoryID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue(
                        "@CategoryName",
                        txtCategoryName.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@CategoryID",
                        selectedCategoryID);

                    con.Open();

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show(
                "Category updated successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LoadCategories();

            ClearFields();

        }

        private void UpdateLanguage()
        {
            bool ar = LanguageManager.IsArabic;

            Text = ar ? "إدارة التصنيفات" : "Categories Management";

            btnAdd.Text = ar ? "إضافة" : "Add";
            btnUpdate.Text = ar ? "تعديل" : "Update";
            btnDelete.Text = ar ? "حذف" : "Delete";
            btnClear.Text = ar ? "مسح" : "Clear";
            lblBrandTitle.Text = ar ? "نظام إدارة سوبر ماركت" : "SUPERMARKET MANAGEMENT SYSTEM";
            label1.Text = ar ? "أسم الصنف" : "Category Name";
            label2.Text = ar ? "بحث" : "Search";
            if (dgvCategories.Columns.Contains("CategoryID"))
            {
                dgvCategories.Columns["CategoryID"].HeaderText =
                    ar ? "رقم التصنيف" : "Category ID";
            }

            if (dgvCategories.Columns.Contains("CategoryName"))
            {
                dgvCategories.Columns["CategoryName"].HeaderText =
                    ar ? "اسم التصنيف" : "Category Name";
            }

            RightToLeft = ar ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = ar;
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            ClearFields();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LanguageManager.LanguageChanged -= UpdateLanguage;
            base.OnFormClosed(e);
        }


    }
}
