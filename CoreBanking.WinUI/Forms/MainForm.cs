using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using CoreBanking.WinUI.Helpers;

namespace CoreBanking.WinUI.UI
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider _serviceProvider;

        // --- UI Controls ---
        private Panel pnlSidebar;
        private Panel pnlHeader;
        private Panel pnlContent;
        private Label lblTitle;
        private Button btnToggleMenu;
        private Panel pnlLogo;
        private Label lblLogoIcon;
        private Label lblLogoText;

        // --- State ---
        private Button currentBtn;
        private Panel leftBorderBtn; // Thanh highlight bên cạnh nút active
        private bool isSidebarExpanded = true;

        // --- Constants (Responsive) ---
        private const int SIDEBAR_WIDTH_EXPANDED = 260;
        private const int SIDEBAR_WIDTH_COLLAPSED = 70; // Đủ rộng để chứa icon 30px
        private const int HEADER_HEIGHT = 60;

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(IServiceProvider serviceProvider) : this()
        {
            _serviceProvider = serviceProvider;
        }

        private void InitializeComponent()
        {
            // 1. Cấu hình Form chính
            this.Text = "Core Banking Administration";
            this.Size = new Size(1366, 768); // Chuẩn HD
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = ThemeHelper.SecondaryColor;

            // Init Highlight Panel
            leftBorderBtn = new Panel();
            leftBorderBtn.Size = new Size(7, 60); // Độ dày thanh highlight
            leftBorderBtn.BackColor = ThemeHelper.AccentColor;
            leftBorderBtn.Visible = false;

            // 2. Sidebar (Container)
            pnlSidebar = new Panel();
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Width = SIDEBAR_WIDTH_EXPANDED;
            pnlSidebar.BackColor = ThemeHelper.PrimaryColor; // Màu xanh Navy

            // --- LOGO AREA ---
            pnlLogo = new Panel { Dock = DockStyle.Top, Height = 100 };

            // Icon Logo (Emoji hoặc Image)
            lblLogoIcon = new Label
            {
                Text = "🏦", // Bank Icon
                Font = new Font("Segoe UI Emoji", 30, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(SIDEBAR_WIDTH_COLLAPSED, 100), // Chiếm trọn bề ngang khi thu nhỏ
                TextAlign = ContentAlignment.MiddleCenter, // Luôn căn giữa
                Location = new Point(0, 0)
            };

            // Text Logo (Sẽ ẩn khi thu nhỏ)
            lblLogoText = new Label
            {
                Text = "CBS ADMIN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(80, 35) // Nằm bên phải icon
            };

            pnlLogo.Controls.Add(lblLogoText); // Add text trước
            pnlLogo.Controls.Add(lblLogoIcon); // Add icon sau để đè lên nếu cần
            pnlSidebar.Controls.Add(pnlLogo);

            // --- MENU ITEMS ---
            // Nút Logout (Đáy Sidebar)
            Button btnLogout = CreateMenuButton("Logout", "🚪");
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.ForeColor = Color.LightCoral;
            btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("Đăng xuất khỏi hệ thống?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    this.Close();
            };
            pnlSidebar.Controls.Add(btnLogout);

            // Menu chính (Add ngược từ dưới lên để Dock Top xếp đúng thứ tự)
            // 5. Settings
            AddMenuItem("Cấu hình hệ thống", "⚙️", null);
            // 4. Reports
            AddMenuItem("Báo cáo & Thống kê", "📊", null);
            // 3. Transactions
            AddMenuItem("Lịch sử Giao dịch", "💸", null);
            // 2. Customers
            AddMenuItem("Quản lý Khách hàng", "👥", null);
            // 1. Dashboard (Mặc định)
            AddMenuItem("Tổng quan", "🏠", OpenForm<LoanDashboard>);

            this.Controls.Add(pnlSidebar);

            // 3. Header (Top Bar)
            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = HEADER_HEIGHT;
            pnlHeader.BackColor = Color.White;

            // Shadow giả (Border bottom)
            Panel pnlHeaderBorder = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.LightGray };
            pnlHeader.Controls.Add(pnlHeaderBorder);

            // Toggle Button (Hamburger)
            btnToggleMenu = new Button();
            btnToggleMenu.Text = "☰";
            btnToggleMenu.Font = new Font("Segoe UI", 16);
            btnToggleMenu.FlatStyle = FlatStyle.Flat;
            btnToggleMenu.FlatAppearance.BorderSize = 0;
            btnToggleMenu.Size = new Size(50, HEADER_HEIGHT);
            btnToggleMenu.Location = new Point(0, 0); // Góc trái Header
            btnToggleMenu.Cursor = Cursors.Hand;
            btnToggleMenu.Click += (s, e) => ToggleSidebar();
            pnlHeader.Controls.Add(btnToggleMenu);

            // Page Title
            lblTitle = new Label();
            lblTitle.Text = "TỔNG QUAN";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = ThemeHelper.TextDark;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(60, 15);
            pnlHeader.Controls.Add(lblTitle);

            // User Info (Góc phải)
            Panel pnlUser = new Panel { Size = new Size(250, HEADER_HEIGHT), Dock = DockStyle.Right };
            Label lblUser = new Label
            {
                Text = "Xin chào, Admin",
                AutoSize = false,
                Size = new Size(180, HEADER_HEIGHT),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ThemeHelper.PrimaryColor,
                Location = new Point(0, 0)
            };
            Label lblAvatar = new Label
            {
                Text = "👤",
                Font = new Font("Segoe UI Emoji", 20),
                AutoSize = true,
                Location = new Point(190, 10)
            };
            pnlUser.Controls.Add(lblUser);
            pnlUser.Controls.Add(lblAvatar);
            pnlHeader.Controls.Add(pnlUser);

            this.Controls.Add(pnlHeader);

            // 4. Content Area
            pnlContent = new Panel();
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.BackColor = ThemeHelper.SecondaryColor; // Màu xám nhạt
            pnlContent.Padding = new Padding(20);
            this.Controls.Add(pnlContent);

            // Z-Order: Sidebar đè lên Content và Header (nếu cần), Header nằm trên Content
            pnlHeader.BringToFront();
            pnlSidebar.BringToFront();

            // Load Default
            if (_serviceProvider != null)
                OpenForm<LoanDashboard>();
        }

        // --- LOGIC RESPONSIVE SIDEBAR ---
        private void ToggleSidebar()
        {
            if (isSidebarExpanded)
            {
                // Thu nhỏ
                pnlSidebar.Width = SIDEBAR_WIDTH_COLLAPSED;
                lblLogoText.Visible = false; // Ẩn chữ Logo

                // Duyệt qua các nút để ẩn text, chỉ hiện icon
                foreach (Control c in pnlSidebar.Controls)
                {
                    if (c is Button btn)
                    {
                        btn.Text = ""; // Xóa text
                        btn.ImageAlign = ContentAlignment.MiddleCenter; // Icon ra giữa
                        btn.Padding = new Padding(0); // Bỏ padding

                        // Tooltip (Optional: Hiển thị tên khi hover nút bé)
                        // ToolTip tt = new ToolTip(); tt.SetToolTip(btn, btn.Tag.ToString());
                    }
                }
            }
            else
            {
                // Mở rộng
                pnlSidebar.Width = SIDEBAR_WIDTH_EXPANDED;
                lblLogoText.Visible = true; // Hiện chữ Logo

                // Khôi phục text cho các nút
                foreach (Control c in pnlSidebar.Controls)
                {
                    if (c is Button btn && btn.Tag != null)
                    {
                        btn.Text = $"   {btn.Tag.ToString()}"; // Lấy lại text từ Tag
                        btn.ImageAlign = ContentAlignment.MiddleLeft; // Icon về trái
                        btn.Padding = new Padding(15, 0, 0, 0); // Trả lại padding

                        // Set lại TextImageRelation để icon nằm trước text
                        btn.TextImageRelation = TextImageRelation.ImageBeforeText;
                    }
                }
            }
            isSidebarExpanded = !isSidebarExpanded;
        }

        // --- BUTTON CREATION ---
        private Button CreateMenuButton(string text, string icon)
        {
            Button btn = new Button();
            btn.Height = 60; // Chiều cao nút
            btn.Dock = DockStyle.Top;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = ThemeHelper.PrimaryColor;
            btn.ForeColor = Color.Gainsboro;
            btn.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            btn.Cursor = Cursors.Hand;

            // Quan trọng: Lưu text gốc vào Tag để dùng khi toggle
            btn.Tag = text;

            // Icon Setup (Dùng Emoji làm icon text giả lập)
            // Nếu dùng Image thật: btn.Image = Resources.IconName;
            // Ở đây dùng mẹo ghép chuỗi: "Icon   Text"
            btn.Text = $"   {text}";

            // Icon giả lập bằng Image Property (Để có thể Align độc lập với Text)
            // Tạo Bitmap từ Emoji icon
            btn.Image = TextToImage(icon, new Font("Segoe UI Emoji", 14), Color.White);
            btn.ImageAlign = ContentAlignment.MiddleLeft;
            btn.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn.Padding = new Padding(15, 0, 0, 0); // Cách lề trái

            // Events
            btn.MouseEnter += (s, e) => { if (currentBtn != btn) btn.BackColor = ThemeHelper.DarkPrimary; };
            btn.MouseLeave += (s, e) => { if (currentBtn != btn) btn.BackColor = ThemeHelper.PrimaryColor; };

            return btn;
        }

        private void AddMenuItem(string text, string icon, Action? action)
        {
            Button btn = CreateMenuButton(text, icon);

            btn.Click += (s, e) =>
            {
                ActivateButton(btn);
                lblTitle.Text = text.ToUpper();
                action?.Invoke();
            };

            pnlSidebar.Controls.Add(btn);
            pnlSidebar.Controls.SetChildIndex(btn, 0); // Đẩy xuống dưới để giữ thứ tự
        }

        private void ActivateButton(Button senderBtn)
        {
            if (senderBtn == null) return;

            // Reset nút cũ
            if (currentBtn != null)
            {
                currentBtn.BackColor = ThemeHelper.PrimaryColor;
                currentBtn.ForeColor = Color.Gainsboro;
                currentBtn.Font = new Font("Segoe UI", 11, FontStyle.Regular);
                currentBtn.Controls.Remove(leftBorderBtn); // Gỡ vệt màu
            }

            // Active nút mới
            currentBtn = senderBtn;
            currentBtn.BackColor = ThemeHelper.DarkPrimary;
            currentBtn.ForeColor = Color.White;
            currentBtn.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            // Gắn vệt màu (Highlight Border)
            leftBorderBtn.BackColor = ThemeHelper.AccentColor;
            leftBorderBtn.Size = new Size(5, 60); // Fix chiều cao
            leftBorderBtn.Location = new Point(0, 0); // Luôn nằm sát trái của nút
            leftBorderBtn.Visible = true;
            leftBorderBtn.BringToFront();

            currentBtn.Controls.Add(leftBorderBtn); // Add border vào trong nút
        }

        // Helper: Chuyển Emoji thành Image để dùng cho thuộc tính .Image của Button
        // Giúp icon không bị mất khi ẩn text (thu nhỏ sidebar)
        private Image TextToImage(string text, Font font, Color color)
        {
            Bitmap bmp = new Bitmap(40, 40); // Size icon chuẩn
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.Clear(Color.Transparent);
                using (Brush brush = new SolidBrush(color))
                {
                    // Căn giữa icon trong khung ảnh 40x40
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (bmp.Width - textSize.Width) / 2;
                    float y = (bmp.Height - textSize.Height) / 2;
                    g.DrawString(text, font, brush, x, y);
                }
            }
            return bmp;
        }

        private void OpenForm<T>() where T : Form
        {
            if (_serviceProvider == null) return;

            pnlContent.Controls.Clear();
            try
            {
                var form = _serviceProvider.GetRequiredService<T>();
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                pnlContent.Controls.Add(form);
                pnlContent.Tag = form;
                form.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải form: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}