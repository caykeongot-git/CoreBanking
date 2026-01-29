using CoreBanking.DAL.Entities;
using CoreBanking.DAL.Repositories;
using CoreBanking.WinUI.Controls;
using CoreBanking.WinUI.Helpers;
// --- THƯ VIỆN LIVECHARTS (Hybrid WinForms + WPF) ---
using LiveCharts;
using LiveCharts.WinForms;
using LiveCharts.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CoreBanking.WinUI.Forms
{
    public partial class MainForm : Form
    {
        private Panel _pnlSidebar;
        private Panel _pnlMenu;
        private Panel _pnlHeader;
        private Panel _pnlBody;
        private Label _lblPageTitle;
        private Button _btnMenuToggle;
        private bool _isSidebarExpanded = true;
        private const int SIDEBAR_WIDTH_EXPANDED = 260;
        private const int SIDEBAR_WIDTH_COLLAPSED = 70;

        // Resize Logic
        private const int cGrip = 16;      // Grip size
        private const int cCaption = 32;   // Caption bar height

        public MainForm()
        {
            // Cấu hình Form cơ bản
            string username = Session.CurrentUser?.FullName ?? "Admin";
            Text = $"Core Banking - Logged in as: {username}";
            Size = new Size(1366, 768); // Kích thước khởi tạo lớn hơn
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None; // Vẫn giữ None để custom UI
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true); // Cho phép vẽ lại khi resize

            InitUI();
        }

        // --- HỖ TRỢ RESIZE CỬA SỔ KHI KHÔNG CÓ VIỀN (MAGIC CODE) ---
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84) // Trap WM_NCHITTEST
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if (pos.Y < cCaption)
                {
                    m.Result = (IntPtr)2;  // HTCAPTION
                    return;
                }
                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void InitUI()
        {
            SetupSidebar();
            SetupHeader();

            _pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColor.Background,
                Padding = new Padding(20),
                AutoScroll = true
            };
            Controls.Add(_pnlBody);
            _pnlBody.BringToFront();

            // Mặc định tải Dashboard
            LoadDashboard();
        }

        private void SetupHeader()
        {
            _pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ThemeColor.Primary };
            _btnMenuToggle = new Button { Text = "☰", Dock = DockStyle.Left, Width = 50, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, Cursor = Cursors.Hand };
            _btnMenuToggle.FlatAppearance.BorderSize = 0;
            _btnMenuToggle.Click += ToggleSidebar;

            _lblPageTitle = new Label { Text = "Dashboard Overview", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Regular), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Padding = new Padding(10, 0, 0, 0) };

            // --- WINDOW CONTROLS (Min, Max, Close) ---
            Panel pnlControls = new Panel { Dock = DockStyle.Right, Width = 150, BackColor = Color.Transparent };

            Label lblClose = CreateControlBtn("✕", (s, e) => Application.Exit());
            lblClose.MouseEnter += (s, e) => lblClose.ForeColor = Color.Red;
            lblClose.MouseLeave += (s, e) => lblClose.ForeColor = Color.White;

            Label lblMax = CreateControlBtn("❐", (s, e) => {
                this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            });

            Label lblMin = CreateControlBtn("—", (s, e) => this.WindowState = FormWindowState.Minimized);

            pnlControls.Controls.Add(lblClose);
            pnlControls.Controls.Add(lblMax);
            pnlControls.Controls.Add(lblMin);

            // Sắp xếp lại vị trí (Dock Right thì cái nào add trước sẽ nằm bên phải cùng)
            lblClose.Dock = DockStyle.Right;
            lblMax.Dock = DockStyle.Right;
            lblMin.Dock = DockStyle.Right;

            _pnlHeader.Controls.Add(pnlControls);
            _pnlHeader.Controls.Add(_lblPageTitle);
            _pnlHeader.Controls.Add(_btnMenuToggle);
            Controls.Add(_pnlHeader);
            _pnlHeader.BringToFront();

            // Kéo thả cửa sổ
            _pnlHeader.MouseDown += DragWindow;
            _lblPageTitle.MouseDown += DragWindow;
        }

        private Label CreateControlBtn(string text, EventHandler onClick)
        {
            Label lbl = new Label
            {
                Text = text,
                Width = 50,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right
            };
            lbl.Click += onClick;
            return lbl;
        }

        // ... (Giữ nguyên các hàm LoadDashboard, SetupSidebar, CreateStatCard, v.v. từ code cũ) ...
        // Copy lại toàn bộ logic LoadDashboard và các Helper method ở đây để đảm bảo file hoàn chỉnh

        // --- NAVIGATION HANDLER ---
        private void LoadView(Control viewControl, string title)
        {
            _pnlBody.Controls.Clear();
            viewControl.Dock = DockStyle.Fill;
            _pnlBody.Controls.Add(viewControl);
            if (_lblPageTitle != null) _lblPageTitle.Text = title;
        }

        private async void LoadDashboard()
        {
            _pnlBody.Controls.Clear();
            if (_lblPageTitle != null) _lblPageTitle.Text = "Dashboard Overview";

            decimal totalAssets = 0;
            int totalCustomers = 0;
            int totalLoans = 0;
            int pendingLoans = 0;
            int loanPending = 0, loanActive = 0, loanPaid = 0, loanBadDebt = 0;

            try
            {
                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var accounts = await uow.Accounts.GetAllAsync();
                    totalAssets = accounts.Sum(a => a.Balance);
                    var customers = await uow.Customers.GetAllAsync();
                    totalCustomers = customers.Count();
                    var loans = await uow.Loans.GetAllAsync();
                    totalLoans = loans.Count();
                    pendingLoans = loans.Count(l => l.Status == LoanStatus.Pending);
                    loanPending = pendingLoans;
                    loanActive = loans.Count(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Approved);
                    loanPaid = loans.Count(l => l.Status == LoanStatus.PaidOff);
                    loanBadDebt = loans.Count(l => l.Status == LoanStatus.BadDebt);
                }
            }
            catch { }

            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 4, Width = _pnlBody.Width - 40 };
            _pnlBody.Resize += (s, e) => { mainLayout.Width = _pnlBody.Width - 40; };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 420));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Row 1
            TableLayoutPanel row1 = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Margin = new Padding(0, 0, 0, 20) };
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            row1.Controls.Add(CreateStatCard("TOTAL ASSETS", totalAssets.ToString("C0"), "Customer Deposits", ThemeColor.Success), 0, 0);
            row1.Controls.Add(CreateStatCard("CUSTOMERS", totalCustomers.ToString(), "Active Users", ThemeColor.Primary), 1, 0);
            row1.Controls.Add(CreateStatCard("TOTAL LOANS", totalLoans.ToString(), "All Applications", ThemeColor.Warning), 2, 0);
            row1.Controls.Add(CreateStatCard("PENDING", pendingLoans.ToString(), "Need Approval", ThemeColor.Danger), 3, 0);
            mainLayout.Controls.Add(row1, 0, 0);

            // Row 2: Charts
            TableLayoutPanel row2 = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(0, 0, 0, 20) };
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            Panel pnlTrend = CreateShadowPanel(ThemeColor.Primary);
            Label lblTrend = new Label { Text = "Transaction Volume (7 Days)", ForeColor = Color.White, Dock = DockStyle.Top, Font = new Font("Segoe UI", 12, FontStyle.Bold), Height = 40, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
            var chartTrend = new LiveCharts.WinForms.CartesianChart { Dock = DockStyle.Fill, BackColor = Color.Transparent, Hoverable = true, DisableAnimations = false };
            chartTrend.AxisX.Add(new Axis { ShowLabels = true, Labels = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" }, Separator = new Separator { Step = 1, StrokeThickness = 0 }, Foreground = System.Windows.Media.Brushes.White });
            chartTrend.AxisY.Add(new Axis { ShowLabels = false, Separator = new Separator { StrokeThickness = 0 } });
            chartTrend.Series = new SeriesCollection { new LineSeries { Title = "Volume ($)", Values = new ChartValues<double> { 5000, 7500, 6000, 8500, 12000, 9500, 11000 }, LineSmoothness = 1, StrokeThickness = 3, Stroke = System.Windows.Media.Brushes.White, Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 255, 255)), PointGeometrySize = 10 } };
            pnlTrend.Controls.Add(chartTrend); pnlTrend.Controls.Add(lblTrend);
            row2.Controls.Add(pnlTrend, 0, 0);

            Panel pnlPie = CreateShadowPanel(Color.White);
            Label lblPie = new Label { Text = "Loan Portfolio", ForeColor = Color.Gray, Dock = DockStyle.Top, Font = new Font("Segoe UI", 12, FontStyle.Bold), Height = 40, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
            var chartPie = new LiveCharts.WinForms.PieChart { Dock = DockStyle.Fill, InnerRadius = 60, LegendLocation = LegendLocation.Bottom, DisableAnimations = false };
            chartPie.Series = new SeriesCollection { new PieSeries { Title = "Personal", Values = new ChartValues<double> { 45 }, Fill = new System.Windows.Media.SolidColorBrush(ToMediaColor(ThemeColor.Primary)), DataLabels = false }, new PieSeries { Title = "Business", Values = new ChartValues<double> { 35 }, Fill = new System.Windows.Media.SolidColorBrush(ToMediaColor(ThemeColor.Warning)), DataLabels = false }, new PieSeries { Title = "Mortgage", Values = new ChartValues<double> { 20 }, Fill = new System.Windows.Media.SolidColorBrush(ToMediaColor(ThemeColor.Success)), DataLabels = false } };
            pnlPie.Controls.Add(chartPie); pnlPie.Controls.Add(lblPie);
            row2.Controls.Add(pnlPie, 1, 0);

            Panel pnlHealth = CreateShadowPanel(Color.White);
            Label lblHealth = new Label { Text = "Customer Credit Health", ForeColor = Color.Gray, Dock = DockStyle.Top, Font = new Font("Segoe UI", 12, FontStyle.Bold), Height = 40, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
            FlowLayoutPanel flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10) };
            flow.Controls.Add(CreateProgressItem("Excellent (750+)", 30, ThemeColor.Success));
            flow.Controls.Add(CreateProgressItem("Good (650-749)", 50, ThemeColor.Primary));
            flow.Controls.Add(CreateProgressItem("Fair (600-649)", 15, ThemeColor.Warning));
            flow.Controls.Add(CreateProgressItem("Poor (<600)", 5, ThemeColor.Danger));
            pnlHealth.Controls.Add(flow); pnlHealth.Controls.Add(lblHealth);
            row2.Controls.Add(pnlHealth, 2, 0);
            mainLayout.Controls.Add(row2, 0, 1);

            // Row 3: Grid
            Panel pnlGrid = CreateShadowPanel(Color.White);
            Label lblGrid = new Label { Text = "Recent Transactions", ForeColor = Color.Gray, Dock = DockStyle.Top, Font = new Font("Segoe UI", 12, FontStyle.Bold), Height = 40, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
            DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackgroundColor = Color.White, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None, EnableHeadersVisualStyles = false, GridColor = Color.FromArgb(240, 240, 240), RowHeadersVisible = false, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowTemplate = { Height = 40 }, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 247, 251), ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Bold), Padding = new Padding(10) };
            dgv.ColumnHeadersHeight = 40;
            dgv.DefaultCellStyle = new DataGridViewCellStyle { SelectionBackColor = ThemeColor.Primary, SelectionForeColor = Color.White, Font = new Font("Segoe UI", 9), Padding = new Padding(10, 0, 0, 0), ForeColor = Color.FromArgb(64, 64, 64) };
            dgv.Columns.Add("ID", "TRX ID"); dgv.Columns.Add("Account", "ACCOUNT NUMBER"); dgv.Columns.Add("Type", "TYPE"); dgv.Columns.Add("Date", "DATE"); dgv.Columns.Add("Amount", "AMOUNT");
            dgv.Rows.Add("TRX-9981", "190203001", "Deposit", "Today, 10:30", "+ $1,200.00");
            dgv.Rows.Add("TRX-9982", "190203002", "Transfer", "Today, 11:15", "- $500.00");
            pnlGrid.Controls.Add(dgv); pnlGrid.Controls.Add(lblGrid);
            mainLayout.Controls.Add(pnlGrid, 0, 2);

            // Row 4
            TableLayoutPanel row4 = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Height = 250, Margin = new Padding(0, 20, 0, 0) };
            row4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            row4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            Panel pnlLoanStats = CreateShadowPanel(Color.White);
            Label lblLoanStats = new Label { Text = "Loan Application Overview", ForeColor = Color.Gray, Dock = DockStyle.Top, Font = new Font("Segoe UI", 12, FontStyle.Bold), Height = 40, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
            TableLayoutPanel loanLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(10) };
            loanLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            loanLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            loanLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            loanLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            loanLayout.Controls.Add(CreateStatusBox("Active", loanActive.ToString(), ThemeColor.Primary));
            loanLayout.Controls.Add(CreateStatusBox("Pending", loanPending.ToString(), ThemeColor.Warning));
            loanLayout.Controls.Add(CreateStatusBox("Paid Off", loanPaid.ToString(), ThemeColor.Success));
            loanLayout.Controls.Add(CreateStatusBox("Bad Debt", loanBadDebt.ToString(), ThemeColor.Danger));
            pnlLoanStats.Controls.Add(loanLayout); pnlLoanStats.Controls.Add(lblLoanStats);
            row4.Controls.Add(pnlLoanStats, 0, 0);
            Panel pnlTasks = CreateShadowPanel(Color.White);
            Label lblTask = new Label { Text = "System Alerts & Tasks", ForeColor = Color.Gray, Dock = DockStyle.Top, Font = new Font("Segoe UI", 12, FontStyle.Bold), Height = 40, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
            FlowLayoutPanel flowTasks = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10), AutoScroll = true };
            flowTasks.Controls.Add(CreateTaskItem("High Value Transaction > $10k", "5 mins ago", ThemeColor.Danger));
            flowTasks.Controls.Add(CreateTaskItem("New Loan Request: Nguyen Van A", "15 mins ago", ThemeColor.Warning));
            flowTasks.Controls.Add(CreateTaskItem("Daily Backup Completed", "1 hour ago", ThemeColor.Success));
            pnlTasks.Controls.Add(flowTasks); pnlTasks.Controls.Add(lblTask);
            row4.Controls.Add(pnlTasks, 1, 0);
            mainLayout.Controls.Add(row4, 0, 3);
            _pnlBody.Controls.Add(mainLayout);
        }

        private void SetupSidebar()
        {
            _pnlSidebar = new Panel { Dock = DockStyle.Left, Width = SIDEBAR_WIDTH_EXPANDED, BackColor = Color.White };
            var pnlLogo = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ThemeColor.Primary };
            var lblLogo = new Label { Name = "lblLogo", Text = "CoreBanking", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            pnlLogo.Controls.Add(lblLogo);
            _pnlSidebar.Controls.Add(pnlLogo);
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = Color.Transparent };
            Button btnLogout = new Button { Text = "   🚪    Logout", Dock = DockStyle.Top, Height = 50, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = ThemeColor.Danger, BackColor = Color.FromArgb(255, 240, 240), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            pnlBottom.Controls.Add(btnLogout);
            _pnlSidebar.Controls.Add(pnlBottom);
            _pnlMenu = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, AutoScroll = true };
            _pnlSidebar.Controls.Add(_pnlMenu);
            _pnlMenu.BringToFront();
            AddSidebarButton("🏠", "Dashboard", true, (s, e) => LoadDashboard());
            AddSidebarButton("👥", "Customers", false, (s, e) => LoadView(new CoreBanking.WinUI.Controls.CustomerControl(), "Customer Management"));
            AddSidebarButton("💰", "Loans", false, (s, e) => LoadView(new CoreBanking.WinUI.Controls.LoanControl(), "Loan Management"));
            AddSidebarButton("💳", "Accounts", false, (s, e) => ShowPlaceholder("Account Management"));
            AddSidebarButton("💸", "Transactions", false, (s, e) => ShowPlaceholder("Transactions History"));
            AddSidebarButton("⚙️", "Settings", false, (s, e) => ShowPlaceholder("System Config"));
            Controls.Add(_pnlSidebar);
        }

        private void ToggleSidebar(object sender, EventArgs e)
        {
            _isSidebarExpanded = !_isSidebarExpanded;
            _pnlSidebar.Width = _isSidebarExpanded ? SIDEBAR_WIDTH_EXPANDED : SIDEBAR_WIDTH_COLLAPSED;
            foreach (Control c in _pnlMenu.Controls)
            {
                if (c is Button btn && btn.Tag != null)
                {
                    string originalText = btn.Tag.ToString();
                    string icon = originalText.Split(" ", StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                    if (_isSidebarExpanded)
                    {
                        btn.Text = originalText;
                        btn.TextAlign = ContentAlignment.MiddleLeft;
                        btn.Font = new Font("Segoe UI", 11, btn.Font.Style);
                    }
                    else
                    {
                        var parts = originalText.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0) btn.Text = parts[0];
                        btn.TextAlign = ContentAlignment.MiddleCenter;
                        btn.Font = new Font("Segoe UI", 14, btn.Font.Style);
                    }
                }
            }
            Control logoControl = _pnlSidebar.Controls.Find("lblLogo", true).FirstOrDefault();
            if (logoControl is Label lblLogo)
            {
                lblLogo.Text = _isSidebarExpanded ? "CoreBanking" : "CB";
                lblLogo.Font = new Font("Segoe UI", _isSidebarExpanded ? 16 : 12, FontStyle.Bold);
            }
        }

        private void ShowPlaceholder(string title)
        {
            _pnlBody.Controls.Clear();
            _lblPageTitle.Text = title;
            Label lbl = new Label { Text = $"Module: {title}\n(Under Construction)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 20, FontStyle.Italic), ForeColor = Color.Gray };
            _pnlBody.Controls.Add(lbl);
        }

        private void AddSidebarButton(string icon, string text, bool isActive, EventHandler onClick)
        {
            string fullText = $" {icon}    {text}";
            Button btn = new Button { Tag = fullText, Text = fullText, Dock = DockStyle.Top, Height = 50, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, isActive ? FontStyle.Bold : FontStyle.Regular), ForeColor = isActive ? ThemeColor.Primary : Color.Gray, BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, AutoEllipsis = true };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => { if (!isActive) btn.BackColor = Color.FromArgb(245, 247, 251); };
            btn.MouseLeave += (s, e) => { if (!isActive) btn.BackColor = Color.Transparent; };
            if (onClick != null) btn.Click += onClick;
            _pnlMenu.Controls.Add(btn);
            btn.SendToBack();
        }

        private StatCard CreateStatCard(string title, string value, string footer, Color color) { return new StatCard { Title = title, Value = value, FooterText = footer, CardColor = color, Dock = DockStyle.Fill, Margin = new Padding(5) }; }
        private Panel CreateShadowPanel(Color bg) { return new Panel { BackColor = bg, Dock = DockStyle.Fill, Margin = new Padding(10), Padding = new Padding(0) }; }
        private Panel CreateStatusBox(string status, string count, Color color) { Panel p = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5), BackColor = Color.White }; Label lblCount = new Label { Text = count, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = color }; Label lblStatus = new Label { Text = status, Dock = DockStyle.Bottom, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Regular), Height = 30, ForeColor = Color.Gray }; p.Controls.Add(lblCount); p.Controls.Add(lblStatus); return p; }
        private Panel CreateProgressItem(string label, int val, Color c) { Panel p = new Panel { Size = new Size(250, 50) }; Label lbl = new Label { Text = label, Location = new Point(0, 0), AutoSize = true, Font = new Font("Segoe UI", 9) }; FlatProgressBar pg = new FlatProgressBar { Value = val, Maximum = 100, ProgressColor = c, Size = new Size(250, 10), Location = new Point(0, 25) }; p.Controls.Add(lbl); p.Controls.Add(pg); return p; }
        private Panel CreateTaskItem(string task, string time, Color dotColor) { Panel p = new Panel { Size = new Size(380, 40), Margin = new Padding(0, 0, 0, 5) }; Panel dot = new Panel { Size = new Size(10, 10), Location = new Point(5, 15), BackColor = dotColor }; GraphicsPath gp = new GraphicsPath(); gp.AddEllipse(0, 0, 10, 10); dot.Region = new Region(gp); Label lblTask = new Label { Text = task, Location = new Point(25, 8), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(64, 64, 64) }; Label lblTime = new Label { Text = time, Location = new Point(280, 8), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Italic), ForeColor = Color.Gray, Anchor = AnchorStyles.Right }; p.Controls.Add(dot); p.Controls.Add(lblTask); p.Controls.Add(lblTime); return p; }

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); }
        }

        private System.Windows.Media.Color ToMediaColor(Color c) { return System.Windows.Media.Color.FromRgb(c.R, c.G, c.B); }

        [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool ReleaseCapture();
    }
}