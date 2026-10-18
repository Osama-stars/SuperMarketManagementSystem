using SupermarketManagementSystem.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using SupermarketManagementSystem.Helpers;

namespace SuperMarketManagementSystem
{
    public partial class LoginForm : Form
    {

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(
        IntPtr hWnd,
        int Msg,
        IntPtr wParam,
        string lParam);

        private const int EM_SETCUEBANNER = 0x1501;
        private SQLiteCommand cmd;

        public LoginForm()
        {
            InitializeComponent();

            UpdateLanguage();


        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (username == "" || password == "")
            {
                MessageBox.Show(
                    "Please enter username and password.",
                    "Login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (SQLiteConnection con = Database.GetConnection())
            {
                string query = @"
            SELECT UserID, Username
            FROM Users
            WHERE Username = @Username
            AND Password = @Password";

                using (
                    
                    cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    con.Open();

                    SQLiteDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int userID = Convert.ToInt32(reader["UserID"]);

                        MessageBox.Show(
                            "Login successful!",
                            "Login",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        MainForm mainForm = new MainForm(userID);

                        this.Hide();

                        mainForm.ShowDialog();

                        this.Show();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Invalid username or password.",
                            "Login",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            SendMessage(
            txtUsername.Handle,
            EM_SETCUEBANNER,
            IntPtr.Zero,
           "Enter your username");

            SendMessage(
            txtPassword.Handle,
            EM_SETCUEBANNER,
            IntPtr.Zero,
           "Enter your password");

            


        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void lblBrandSubtitle_Click(object sender, EventArgs e)
        {

        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtPassword.Focus();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnLogin.PerformClick();
            }
        }

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            txtUsername.Focus();
        }

        private void btnLanguage_Click(object sender, EventArgs e)
        {

            LanguageManager.SetLanguage(
                LanguageManager.IsArabic ? "en" : "ar");

            UpdateLanguage();

        }

        private void UpdateLanguage()
        {
            Text = LanguageManager.IsArabic
                ? "تسجيل الدخول - نظام إدارة سوبر ماركت"
                : "Login - Supermarket management System";

            lblWelcome.Text = LanguageManager.IsArabic
                ? "مرحباً بك"
                : "Welcome";

            lblDescription.Text = LanguageManager.IsArabic
                ? "يرجى تسجيل الدخول للمتابعة"
                : "Sign in to continue";

            label2.Text = LanguageManager.IsArabic
                ? "اسم المستخدم"
                : "Username";

            label3.Text = LanguageManager.IsArabic
                ? "كلمة المرور"
                : "Password";

            btnLogin.Text = LanguageManager.IsArabic
                ? "تسجيل الدخول"
                : "Login";

            btnLanguage.Text = LanguageManager.IsArabic
                ? "English"
                : "العربية";

            lblBrandTitle.Text = LanguageManager.IsArabic ?
                "نظام إدارة سوبر ماركت"
                : "SUPERMARKET MANAGEMENT SYSTEM";

            lblBrandSubtitle.Text = LanguageManager.IsArabic ?
                "بسيط • سريع • امن"
                : "Simple • Fast • Reliable";

            RightToLeft = LanguageManager.IsArabic
                ? RightToLeft.Yes
                : RightToLeft.No;

            RightToLeftLayout = LanguageManager.IsArabic;
        }
    }

}
