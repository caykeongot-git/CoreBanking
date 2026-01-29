using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using CoreBanking.WinUI.Helpers;

namespace CoreBanking.WinUI.UI
{
    public partial class LoginForm : Form
    {
        // Service Provider để mở form tiếp theo
        private readonly IServiceProvider _serviceProvider;

        // Các Control giao diện
        private Panel pnlLeft;
        private Panel pnlRight;
        private PictureBox pbLogo;
        private Label lblTitle;
        private Label lblSubTitle;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblExit;

        // Constructor cho Designer (Bắt buộc)
        public LoginForm()
        {
            InitializeComponent();
        }

        // Constructor cho DI (Chạy thực tế)
        public LoginForm(IServiceProvider serviceProvider) : this()
        {
            _serviceProvider = serviceProvider;
        }

        private void InitializeComponent()
        {
            // 1. Cấu hình Form chính (Style HMS: Không viền, Center Screen)
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(750, 450);
            this.BackColor = Color.White;

            // 2. Khởi tạo Panel Trái (Branding Area)
            pnlLeft = new Panel();
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Width = 350;
            pnlLeft.BackColor = ThemeHelper.PrimaryColor; // Màu xanh Navy chuẩn Banking

            // Logo/Icon (Giả lập bằng Label nếu chưa có ảnh)
            Label lblIcon = new Label();
            lblIcon.Text = "🏦"; // Icon ngân hàng
            lblIcon.Font = new Font("Segoe UI Emoji", 48, FontStyle.Regular);
            lblIcon.ForeColor = Color.White;
            lblIcon.AutoSize = true;
            lblIcon.Location = new Point(120, 120);
            pnlLeft.Controls.Add(lblIcon);

            // Tên hệ thống bên trái
            Label lblSystemName = new Label();
            lblSystemName.Text = "CORE BANKING\nSYSTEM";
            lblSystemName.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblSystemName.ForeColor = Color.White;
            lblSystemName.TextAlign = ContentAlignment.MiddleCenter;
            lblSystemName.AutoSize = true;
            lblSystemName.Location = new Point(70, 220);
            pnlLeft.Controls.Add(lblSystemName);

            // 3. Khởi tạo Panel Phải (Input Area)
            pnlRight = new Panel();
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.BackColor = Color.White;
            pnlRight.Padding = new Padding(40);

            // Nút thoát (X)
            lblExit = new Label();
            lblExit.Text = "X";
            lblExit.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblExit.ForeColor = Color.Gray;
            lblExit.Cursor = Cursors.Hand;
            lblExit.AutoSize = true;
            lblExit.Location = new Point(370, 10); // Góc phải trên
            lblExit.Click += (s, e) => Application.Exit();
            pnlRight.Controls.Add(lblExit);

            // Tiêu đề Form Login
            lblTitle = new Label();
            lblTitle.Text = "Welcome Back";
            lblTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblTitle.ForeColor = ThemeHelper.PrimaryColor;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(40, 60);
            pnlRight.Controls.Add(lblTitle);

            lblSubTitle = new Label();
            lblSubTitle.Text = "Please login to your account";
            lblSubTitle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblSubTitle.ForeColor = Color.Gray;
            lblSubTitle.AutoSize = true;
            lblSubTitle.Location = new Point(42, 100);
            pnlRight.Controls.Add(lblSubTitle);

            // Ô nhập liệu: Username
            Panel pnlUserLine = new Panel { BackColor = Color.LightGray, Size = new Size(300, 2), Location = new Point(45, 185) };
            txtUsername = new TextBox();
            txtUsername.BorderStyle = BorderStyle.None;
            txtUsername.Font = new Font("Segoe UI", 12);
            txtUsername.ForeColor = Color.Black;
            txtUsername.Location = new Point(45, 155);
            txtUsername.Width = 300;
            txtUsername.PlaceholderText = "Username";
            txtUsername.Text = "admin"; // Default for testing
            pnlRight.Controls.Add(txtUsername);
            pnlRight.Controls.Add(pnlUserLine);

            // Ô nhập liệu: Password
            Panel pnlPassLine = new Panel { BackColor = Color.LightGray, Size = new Size(300, 2), Location = new Point(45, 245) };
            txtPassword = new TextBox();
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Segoe UI", 12);
            txtPassword.ForeColor = Color.Black;
            txtPassword.Location = new Point(45, 215);
            txtPassword.Width = 300;
            txtPassword.PlaceholderText = "Password";
            txtPassword.PasswordChar = '•';
            txtPassword.Text = "admin"; // Default for testing
            pnlRight.Controls.Add(txtPassword);
            pnlRight.Controls.Add(pnlPassLine);

            // Nút Login
            btnLogin = new Button();
            btnLogin.Text = "LOGIN";
            btnLogin.BackColor = ThemeHelper.PrimaryColor;
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnLogin.Size = new Size(300, 45);
            btnLogin.Location = new Point(45, 290);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;
            pnlRight.Controls.Add(btnLogin);

            // QUAN TRỌNG: Thêm Panel vào Form theo thứ tự chuẩn
            // Add pnlLeft trước để nó Dock Left
            // Add pnlRight sau để nó Fill phần còn lại
            this.Controls.Add(pnlLeft);
            this.Controls.Add(pnlRight);

            // Đảm bảo Z-Order đúng: pnlRight phải nằm trên cùng (Top) trong vùng Fill
            pnlRight.BringToFront();
            // pnlLeft nằm dưới hoặc bên cạnh, không được đè lên pnlRight
            pnlLeft.SendToBack();

            // Hỗ trợ phím Enter để login
            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            // Logic Core Banking: Kiểm tra user
            // Ở đây tạm thời hardcode admin/admin như yêu cầu logic cũ
            // Thực tế có thể gọi _serviceProvider.GetService<IAuthService>().Login(...)
            if (txtUsername.Text == "admin" && txtPassword.Text == "admin")
            {
                this.Hide();
                if (_serviceProvider != null)
                {
                    var mainForm = _serviceProvider.GetRequiredService<MainForm>();
                    mainForm.FormClosed += (s, args) => this.Close();
                    mainForm.Show();
                }
                else
                {
                    MessageBox.Show("Service Provider is null (Design Mode?)");
                }
            }
            else
            {
                MessageBox.Show("Invalid username or password!", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Override OnPaint để vẽ bo góc Form nếu muốn (Style HMS)
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Có thể thêm code vẽ viền hoặc bo góc ở đây nếu cần thiết kế trau chuốt hơn
        }
    }
}