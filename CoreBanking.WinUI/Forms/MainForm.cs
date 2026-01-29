using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using CoreBanking.WinUI.Helpers;
using CoreBanking.WinUI.Controls;

namespace CoreBanking.WinUI.UI
{
    public partial class MainForm : Form
    {
        // --- Fields ---
        private readonly IServiceProvider _serviceProvider;
        private Button currentBtn;
        private Panel leftBorderBtn;
        private Form currentChildForm;

        // --- UI Controls (Khai báo tường minh để dễ quản lý) ---
        private Panel pnlMenu;
        private Panel pnlLogo;
        private PictureBox btnHome;
        private Panel pnlTitleBar;
        private Label lblTitle;
        private Panel pnlDesktop;
        private Panel pnlShadow;

        // Window Control Buttons
        private Button btnClose;
        private Button btnMaximize;
        private Button btnMinimize;

        // --- Drag Form Logic (DllImport) ---
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        // Constructor
        public MainForm() { InitializeComponent(); }

        public MainForm(IServiceProvider serviceProvider) : this()
        {
            _serviceProvider = serviceProvider;
            // Mặc định mở Dashboard
            OpenChildForm(_serviceProvider.GetRequiredService<LoanDashboard>());
        }

        private void InitializeComponent()
        {
            // 1. Form Properties
            this.Text = "Core Banking Enterprise";
            this.ClientSize = new Size(1400, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; // Không viền chuẩn Flat UI
            this.MinimumSize = new Size(1100, 600);
            this.BackColor = ThemeHelper.BackgroundColor;

            // 2. Init Components
            leftBorderBtn = new Panel();
            leftBorderBtn.Size = new Size(7, 60);
            pnlMenu = new Panel();
            pnlLogo = new Panel();
            pnlTitleBar = new Panel();
            lblTitle = new Label();
            pnlDesktop = new Panel();
            pnlShadow = new Panel();

            // --- SETUP MENU SIDEBAR ---
            pnlMenu.BackColor = ThemeHelper.PrimaryColor;
            pnlMenu.Dock = DockStyle.Left;
            pnlMenu.Width = 240;
            pnlMenu.Padding = new Padding(0, 0, 0, 15); // Padding bottom
            pnlMenu.ZOrder(); // Đảm bảo nằm trên

            // Setup Buttons (Add từ dưới lên để Dock Top xếp đúng, hoặc dùng hàm helper)
            // Logout (Dock Bottom)
            Button btnLogout = CreateMenuButton("Đăng Xuất", "🚪", null);
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.Click += (s, e) => {
                if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    Application.Exit();
            };
            pnlMenu.Controls.Add(btnLogout);

            // Menu Items chính
            // Lưu ý thứ tự: Cái nào Add sau cùng sẽ nằm trên cùng với DockStyle.Top
            CreateMenuItem("Cài Đặt", "icon_setting", null);
            CreateMenuItem("Báo Cáo", "icon_report", null);
            CreateMenuItem("Giao Dịch", "icon_transaction", null);
            CreateMenuItem("Khoản Vay", "icon_loan", () => OpenChildForm(_serviceProvider.GetRequiredService<LoanDashboard>())); // Demo
            CreateMenuItem("Khách Hàng", "icon_customer", null);
            CreateMenuItem("Dashboard", "icon_dashboard", () => OpenChildForm(_serviceProvider.GetRequiredService<LoanDashboard>()));

            // Setup Logo Area
            pnlLogo.Dock = DockStyle.Top;
            pnlLogo.Height = 100;
            pnlLogo.BackColor = ThemeHelper.DarkPrimary; // Màu đậm hơn chút

            Label lblLogoText = new Label();
            lblLogoText.Text = "CBS ADMIN";
            lblLogoText.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblLogoText.ForeColor = Color.White;
            lblLogoText.AutoSize = true;
            lblLogoText.Location = new Point(55, 35);
            pnlLogo.Controls.Add(lblLogoText);

            // Home Click Event (Reset)
            pnlLogo.Click += (s, e) => Reset();
            lblLogoText.Click += (s, e) => Reset();

            pnlMenu.Controls.Add(pnlLogo);
            this.Controls.Add(pnlMenu); // Add Menu vào Form

            // --- SETUP TITLE BAR ---
            pnlTitleBar.Dock = DockStyle.Top;
            pnlTitleBar.Height = 75;
            pnlTitleBar.BackColor = Color.White;
            pnlTitleBar.MouseDown += PnlTitleBar_MouseDown; // Drag Form

            // Title Label
            lblTitle.Text = "TỔNG QUAN";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = ThemeHelper.PrimaryColor;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(25, 25);
            pnlTitleBar.Controls.Add(lblTitle);

            // Control Box (Min/Max/Close)
            SetupControlBox();

            this.Controls.Add(pnlTitleBar);

            // --- SETUP SHADOW LINE ---
            pnlShadow.Dock = DockStyle.Top;
            pnlShadow.Height = 5;
            pnlShadow.BackColor = Color.FromArgb(230, 230, 230); // Màu bóng đổ giả
            this.Controls.Add(pnlShadow);

            // --- SETUP DESKTOP PANEL (MAIN CONTENT) ---
            pnlDesktop.Dock = DockStyle.Fill;
            pnlDesktop.BackColor = ThemeHelper.BackgroundColor;
            pnlDesktop.Padding = new Padding(10);
            this.Controls.Add(pnlDesktop);

            // Final Z-Order Adjustment
            // Trong WinForms: Control add cuối cùng sẽ có Z-Index thấp nhất (nằm dưới)
            // Nhưng với Docking, thứ tự Add quyết định ai chiếm không gian trước.
            // Menu (Left) -> TitleBar (Top) -> Shadow (Top) -> Desktop (Fill)
            // Logic này đã đúng để Desktop lấp đầy khoảng trống còn lại.
        }

        // --- LOGIC METHODS ---

        private void CreateMenuItem(string text, string iconName, Action action)
        {
            Button btn = new Button();
            btn.Dock = DockStyle.Top;
            btn.Height = 60;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Text = "   " + text;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn.Padding = new Padding(10, 0, 0, 0);
            btn.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            btn.ForeColor = Color.Gainsboro;
            btn.Cursor = Cursors.Hand;

            // Icon giả lập (Emoji) để không lỗi resource
            // Trong thực tế HMS dùng: btn.Image = Properties.Resources.ResourceManager.GetObject(iconName) as Image;
            string emoji = GetEmojiIcon(iconName);
            btn.Text = $"   {emoji}   {text}";

            btn.Click += (s, e) =>
            {
                ActivateButton(s);
                action?.Invoke();
            };

            pnlMenu.Controls.Add(btn);
        }

        private string GetEmojiIcon(string name)
        {
            return name switch
            {
                "icon_dashboard" => "🏠",
                "icon_customer" => "👥",
                "icon_loan" => "💰",
                "icon_transaction" => "💸",
                "icon_report" => "📊",
                "icon_setting" => "⚙️",
                _ => "🔹"
            };
        }

        private void ActivateButton(object senderBtn)
        {
            if (senderBtn != null)
            {
                DisableButton();
                currentBtn = (Button)senderBtn;

                // Button Style Active
                currentBtn.BackColor = Color.FromArgb(20, 30, 60); // Màu đậm custom
                currentBtn.ForeColor = Color.White;
                currentBtn.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold); // Phóng to nhẹ

                // Left Border Logic
                pnlMenu.Controls.Add(leftBorderBtn); // Add lại để nằm trên nút
                leftBorderBtn.BackColor = ThemeHelper.AccentColor;
                leftBorderBtn.Location = new Point(0, currentBtn.Location.Y);
                leftBorderBtn.Visible = true;
                leftBorderBtn.BringToFront();

                // Update Header Icon/Title
                lblTitle.Text = currentBtn.Text.Replace("   ", "").Substring(2).ToUpper(); // Remove emoji padding
            }
        }

        private void DisableButton()
        {
            if (currentBtn != null)
            {
                currentBtn.BackColor = ThemeHelper.PrimaryColor;
                currentBtn.ForeColor = Color.Gainsboro;
                currentBtn.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            }
        }

        private void Reset()
        {
            DisableButton();
            leftBorderBtn.Visible = false;
            lblTitle.Text = "TỔNG QUAN";
            // Quay về Home Form
            OpenChildForm(_serviceProvider.GetRequiredService<LoanDashboard>());
        }

        private void OpenChildForm(Form childForm)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close(); // Đóng form cũ
            }

            currentChildForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            pnlDesktop.Controls.Add(childForm);
            pnlDesktop.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        // --- CONTROL BOX LOGIC ---
        private void SetupControlBox()
        {
            btnClose = CreateControlBtn("X", Color.IndianRed);
            btnClose.Click += (s, e) => Application.Exit();

            btnMaximize = CreateControlBtn("⬜", Color.Gray);
            btnMaximize.Click += (s, e) => {
                if (WindowState == FormWindowState.Normal) WindowState = FormWindowState.Maximized;
                else WindowState = FormWindowState.Normal;
            };

            btnMinimize = CreateControlBtn("_", Color.Gray);
            btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;

            // Add ngược chiều để Neo phải đúng
            pnlTitleBar.Controls.Add(btnClose);
            pnlTitleBar.Controls.Add(btnMaximize);
            pnlTitleBar.Controls.Add(btnMinimize);
        }

        private Button CreateControlBtn(string text, Color hoverColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Dock = DockStyle.Right;
            btn.Width = 45;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.Click += (s, e) => { }; // Placeholder event

            // Hover effect
            btn.MouseEnter += (s, e) => { btn.BackColor = hoverColor; btn.ForeColor = Color.White; };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.White; btn.ForeColor = Color.Black; };

            return btn;
        }

        // Drag Form Event
        private void PnlTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
    }
}