using System;
using System.Drawing;
using System.Windows.Forms;
using CoreBanking.WinUI.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBanking.WinUI.UI
{
    public partial class LoginForm : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private TextBox txtUser;
        private TextBox txtPass;

        public LoginForm() { InitializeComponent(); }

        public LoginForm(IServiceProvider serviceProvider) : this()
        {
            _serviceProvider = serviceProvider;
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(800, 500);

            // 1. Panel Trái (Branding)
            Panel pnlLeft = new Panel();
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Width = 350;
            pnlLeft.BackColor = ThemeHelper.PrimaryColor;

            Label lblLogo = new Label();
            lblLogo.Text = "CBS";
            lblLogo.Font = new Font("Segoe UI", 48, FontStyle.Bold);
            lblLogo.ForeColor = Color.White;
            lblLogo.AutoSize = true;
            lblLogo.Location = new Point(100, 150);
            pnlLeft.Controls.Add(lblLogo);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Core Banking System";
            lblSubtitle.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            lblSubtitle.ForeColor = Color.WhiteSmoke;
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(90, 230);
            pnlLeft.Controls.Add(lblSubtitle);

            this.Controls.Add(pnlLeft);

            // 2. Panel Phải (Form)
            Panel pnlRight = new Panel();
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.BackColor = Color.White;

            // Nút Close
            Label lblClose = new Label();
            lblClose.Text = "X";
            lblClose.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblClose.ForeColor = Color.Gray;
            lblClose.Location = new Point(410, 10);
            lblClose.Cursor = Cursors.Hand;
            lblClose.Click += (s, e) => Application.Exit();
            pnlRight.Controls.Add(lblClose);

            // Tiêu đề
            Label lblTitle = new Label();
            lblTitle.Text = "ĐĂNG NHẬP";
            lblTitle.Font = ThemeHelper.HeaderFont;
            lblTitle.ForeColor = ThemeHelper.PrimaryColor;
            lblTitle.Location = new Point(50, 80);
            lblTitle.AutoSize = true;
            pnlRight.Controls.Add(lblTitle);

            // Inputs
            txtUser = CreateInput("Tên đăng nhập", 150);
            txtPass = CreateInput("Mật khẩu", 220, true);
            pnlRight.Controls.Add(txtUser);
            pnlRight.Controls.Add(txtPass);

            // Button
            Button btnLogin = new Button();
            btnLogin.Text = "LOGIN";
            btnLogin.BackColor = ThemeHelper.PrimaryColor;
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Size = new Size(300, 45);
            btnLogin.Location = new Point(50, 300);
            btnLogin.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;
            pnlRight.Controls.Add(btnLogin);

            this.Controls.Add(pnlRight);
        }

        private TextBox CreateInput(string placeholder, int y, bool isPass = false)
        {
            TextBox txt = new TextBox();
            txt.PlaceholderText = placeholder;
            txt.Location = new Point(50, y);
            txt.Size = new Size(300, 30);
            txt.Font = new Font("Segoe UI", 11);
            txt.BorderStyle = BorderStyle.FixedSingle;
            if (isPass) txt.PasswordChar = '•';
            txt.Text = "admin"; // Auto fill để test
            return txt;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (txtUser.Text == "admin" && txtPass.Text == "admin")
            {
                this.Hide();
                if (_serviceProvider != null)
                {
                    var mainForm = _serviceProvider.GetRequiredService<MainForm>();
                    mainForm.FormClosed += (s, args) => this.Close();
                    mainForm.Show();
                }
            }
            else
            {
                MessageBox.Show("Sai thông tin! (admin/admin)");
            }
        }
    }
}