using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SupermarketManagementSystem.Helpers;

namespace SuperMarketManagementSystem
{
    public partial class MainForm : Form
    {

        private int userID;
        public MainForm(int userID)
        {
            InitializeComponent();

            this.userID = userID;

            LanguageManager.LanguageChanged += UpdateLanguage;
            UpdateLanguage();

            this.WindowState = FormWindowState.Maximized;
        }

        private void btnCategories_Click(object sender, EventArgs e)
        {
            CategoriesForm form = new CategoriesForm();

            form.ShowDialog();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void btnProducts_Click(object sender, EventArgs e)
        {

            ProductsFrom form = new ProductsFrom();
            form.ShowDialog();

        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {

            CustomersForm form = new CustomersForm();
            form.ShowDialog();

        }

        

        private void btnSales_Click(object sender, EventArgs e)
        {
            SalesForm form = new SalesForm(userID);
            form.ShowDialog();
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            ReportsForm form = new ReportsForm();
            form.ShowDialog();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
       "Are you sure you want to logout?",
       "Logout",
       MessageBoxButtons.YesNo,
       MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Hide();

                LoginForm loginForm = new LoginForm();
                loginForm.ShowDialog();

                this.Close();
            }

        }

        private void UpdateLanguage()
        {
            bool ar = LanguageManager.IsArabic;

            Text = LanguageManager.IsArabic
                ? "نظام إدارة سوبر ماركت"
                : "Supermarket management System";

            btnProducts.Text = ar ? "المنتجات" : "Products";
            btnCategories.Text = ar ? "التصنيفات" : "Categories";
            btnCustomers.Text = ar ? "العملاء" : "Customers";
            btnSales.Text = ar ? "المبيعات" : "Sales";
            btnReports.Text = ar ? "التقارير" : "Reports";
            btnLogout.Text = ar ? "تسجيل الخروج" : "Logout";

            lblBrandTitle.Text = ar
                ? "نظام إدارة سوبر ماركت"
                : "SUPERMARKET MANAGEMENT SYSTEM";

            label1.Text = ar ? "أختر القسم الذي تريد الذهاب اليه" :
                "Select The page you want to go to.";

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
