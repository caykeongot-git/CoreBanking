using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using CoreBanking.BLL.Interfaces;
using CoreBanking.WinUI.Helpers; // Dùng ThemeHelper và ThemeColor
using CoreBanking.DAL.Entities; // Dùng User entity

namespace CoreBanking.WinUI.UI
{
    public class LoginForm : Form
    {
        private TextBox _txtUser;
        private TextBox _txtPass;
        private Button _btnLogin;
        private Label _lblMsg;
        private readonly IUserService _userService; // Sử dụng Service thay vì DbContext trực tiếp

        // Constructor nhận IUserService từ DI
        public LoginForm(IUserService userService)
        {
            _userService = userService;

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(750, 450);
            this.BackColor = Color.White;

            InitializeUI();
        }

        private void InitializeUI()
        {
            // --- LEFT SIDE ---
            Panel pnlLeft = new Panel { Dock = DockStyle.Left, Width = 300, BackColor = ThemeColor.Sidebar };
            Label lblLogo = new Label { Text = "CORE\nBANK", Font = new Font("Segoe UI", 36, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(50, 120), TextAlign = ContentAlignment.MiddleCenter };
            Label lblSlogan = new Label { Text = "Core Banking System\nEnterprise Edition", Font = new Font("Segoe UI", 12, FontStyle.Regular), ForeColor = Color.FromArgb(200, 255, 255, 255), AutoSize = true, Location = new Point(55, 250), TextAlign = ContentAlignment.MiddleCenter };
            pnlLeft.Controls.Add(lblLogo); pnlLeft.Controls.Add(lblSlogan);

            // --- RIGHT SIDE ---
            Panel pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40) };
            Label lblClose = new Label { Text = "X", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(410, 10), AutoSize = true, Cursor = Cursors.Hand };
            lblClose.Click += (s, e) => Application.Exit();

            Label lblTitle = new Label { Text = "Welcome Back", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.Black, Location = new Point(40, 50), AutoSize = true };

            Label lblUser = new Label { Text = "Username", ForeColor = Color.Gray, Location = new Point(40, 120), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _txtUser = new TextBox { Location = new Point(40, 145), Width = 370, Font = new Font("Segoe UI", 12), BorderStyle = BorderStyle.FixedSingle, Text = "admin" };

            Label lblPass = new Label { Text = "Password", ForeColor = Color.Gray, Location = new Point(40, 190), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _txtPass = new TextBox { Location = new Point(40, 215), Width = 370, Font = new Font("Segoe UI", 12), BorderStyle = BorderStyle.FixedSingle, PasswordChar = '•', Text = "admin" };

            _btnLogin = new Button { Text = "LOGIN", BackColor = ThemeColor.Sidebar, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold), Size = new Size(370, 45), Location = new Point(40, 280), Cursor = Cursors.Hand };
            _btnLogin.FlatAppearance.BorderSize = 0;
            _btnLogin.Click += BtnLogin_Click; // Gán sự kiện click

            _lblMsg = new Label { Text = "", ForeColor = Color.Red, Location = new Point(40, 340), AutoSize = true, Font = new Font("Segoe UI", 9) };
            Label lblFooter = new Label { Text = "Default: admin / admin", ForeColor = Color.LightGray, Location = new Point(150, 420), AutoSize = true, Font = new Font("Segoe UI", 8) };

            pnlRight.Controls.Add(lblClose); pnlRight.Controls.Add(lblTitle);
            pnlRight.Controls.Add(lblUser); pnlRight.Controls.Add(_txtUser);
            pnlRight.Controls.Add(lblPass); pnlRight.Controls.Add(_txtPass);
            pnlRight.Controls.Add(_btnLogin); pnlRight.Controls.Add(_lblMsg); pnlRight.Controls.Add(lblFooter);

            this.Controls.Add(pnlRight); this.Controls.Add(pnlLeft);

            pnlLeft.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); } };
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string u = _txtUser.Text.Trim();
            string p = _txtPass.Text.Trim();

            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
            {
                _lblMsg.Text = "Please enter username and password.";
                return;
            }

            _btnLogin.Enabled = false;
            _btnLogin.Text = "LOGGING IN...";

            try
            {
                // Gọi Service để đăng nhập
                var user = await _userService.LoginAsync(u, p);

                if (user != null)
                {
                    // --- LƯU SESSION ---
                    Session.CurrentUser = user;

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    _lblMsg.Text = "Invalid username or password!";
                    _txtPass.Clear();
                    _txtPass.Focus();
                }
            }
            catch (Exception ex)
            {
                _lblMsg.Text = "Login Error: " + ex.Message;
            }
            finally
            {
                _btnLogin.Enabled = true;
                _btnLogin.Text = "LOGIN";
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool ReleaseCapture();
    }
}