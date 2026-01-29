using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CoreBanking.BLL.Interfaces;
using CoreBanking.DAL.Repositories;
using CoreBanking.WinUI.Controls; // Sử dụng StatCard và RoundedPanel từ namespace này
using CoreBanking.WinUI.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBanking.WinUI.UI
{
    public partial class LoanDashboard : Form
    {
        private readonly IUnitOfWork _unitOfWork;
        // Nếu muốn dùng Service layer thay vì UnitOfWork trực tiếp thì inject ILoanService
        // private readonly ILoanService _loanService;

        // UI Components
        private FlowLayoutPanel pnlStatsContainer;
        private Panel pnlGridContainer;
        private DataGridView dgvLoans;
        private Label lblHeader;
        private Button btnRefresh;
        private Button btnNewLoan;

        // Constructor cho Designer (tránh lỗi Visual Studio)
        public LoanDashboard()
        {
            InitializeComponent();
        }

        // Constructor chính với DI
        public LoanDashboard(IUnitOfWork unitOfWork) : this()
        {
            _unitOfWork = unitOfWork;
            // Load dữ liệu khi form hiện lên
            this.Load += async (s, e) => await LoadDashboardData();
        }

        private void InitializeComponent()
        {
            // 1. Form Settings
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.BackColor = ThemeHelper.SecondaryColor; // Màu nền xám nhạt
            this.Padding = new Padding(20);

            // 2. Header Area (Tiêu đề + Nút tác vụ)
            Panel pnlHeaderArea = new Panel();
            pnlHeaderArea.Dock = DockStyle.Top;
            pnlHeaderArea.Height = 60;
            pnlHeaderArea.Padding = new Padding(0, 0, 0, 10); // Spacing bottom

            lblHeader = new Label();
            lblHeader.Text = "Tổng Quan Khoản Vay";
            lblHeader.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblHeader.ForeColor = ThemeHelper.TextDark;
            lblHeader.AutoSize = true;
            lblHeader.Location = new Point(0, 10);
            pnlHeaderArea.Controls.Add(lblHeader);

            // Nút Refresh
            btnRefresh = CreateActionButton("Làm mới", Color.Gray);
            btnRefresh.Location = new Point(pnlHeaderArea.Width - 250, 10); // Tạm tính, sẽ neo phải
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += async (s, e) => await LoadDashboardData();
            pnlHeaderArea.Controls.Add(btnRefresh);

            // Nút Thêm mới
            btnNewLoan = CreateActionButton("Đăng ký Vay", ThemeHelper.AccentColor);
            btnNewLoan.Location = new Point(pnlHeaderArea.Width - 120, 10);
            btnNewLoan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNewLoan.Click += (s, e) => MessageBox.Show("Tính năng đang phát triển!");
            pnlHeaderArea.Controls.Add(btnNewLoan);

            this.Controls.Add(pnlHeaderArea);

            // 3. Stats Area (Các thẻ thống kê)
            pnlStatsContainer = new FlowLayoutPanel();
            pnlStatsContainer.Dock = DockStyle.Top;
            pnlStatsContainer.Height = 160; // Chiều cao đủ cho thẻ
            pnlStatsContainer.FlowDirection = FlowDirection.LeftToRight;
            pnlStatsContainer.WrapContents = false; // Không xuống dòng nếu không cần thiết
            pnlStatsContainer.AutoScroll = true; // Cho phép scroll ngang nếu màn hình bé
            pnlStatsContainer.Padding = new Padding(0, 10, 0, 20); // Spacing
            
            // Add Sample Cards (Sẽ update số liệu thật sau)
            pnlStatsContainer.Controls.Add(new StatCard("Tổng Dư Nợ", "0 VND", Color.FromArgb(0, 123, 255)));
            pnlStatsContainer.Controls.Add(new StatCard("Hồ Sơ Chờ", "0", Color.FromArgb(255, 193, 7)));
            pnlStatsContainer.Controls.Add(new StatCard("Khách Hàng", "0", Color.FromArgb(40, 167, 69)));
            pnlStatsContainer.Controls.Add(new StatCard("Nợ Quá Hạn", "0", Color.FromArgb(220, 53, 69)));

            this.Controls.Add(pnlStatsContainer);

            // 4. Grid Area (Danh sách chi tiết)
            pnlGridContainer = new Panel();
            pnlGridContainer.Dock = DockStyle.Fill;
            pnlGridContainer.Padding = new Padding(0, 20, 0, 0); // Cách stat card ra

            // Label tiêu đề bảng
            Label lblGridTitle = new Label();
            lblGridTitle.Text = "Danh Sách Khoản Vay Gần Đây";
            lblGridTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblGridTitle.ForeColor = Color.Gray;
            lblGridTitle.Dock = DockStyle.Top;
            lblGridTitle.Height = 30;
            pnlGridContainer.Controls.Add(lblGridTitle);

            // DataGridView
            dgvLoans = new DataGridView();
            dgvLoans.Dock = DockStyle.Fill;
            GridHelper.StyleGrid(dgvLoans); // Áp dụng Style chuẩn HMS
            
            // Thêm cột thủ công (nếu không auto-generate)
            dgvLoans.Columns.Add("Id", "Mã HS");
            dgvLoans.Columns.Add("Customer", "Khách Hàng");
            dgvLoans.Columns.Add("Amount", "Số Tiền");
            dgvLoans.Columns.Add("Rate", "Lãi Suất");
            dgvLoans.Columns.Add("Term", "Kỳ Hạn");
            dgvLoans.Columns.Add("Status", "Trạng Thái");
            
            // Custom format column
            dgvLoans.Columns["Amount"].DefaultCellStyle.Format = "N0";
            dgvLoans.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            pnlGridContainer.Controls.Add(dgvLoans);
            this.Controls.Add(pnlGridContainer);

            // Z-Order
            pnlHeaderArea.SendToBack(); // Top
            pnlStatsContainer.SendToBack(); // Top (sau header)
            pnlGridContainer.BringToFront(); // Fill
        }

        private Button CreateActionButton(string text, Color color)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Size = new Size(110, 35);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private async Task LoadDashboardData()
        {
            if (_unitOfWork == null) return; // Design mode safety

            try
            {
                // Show loading state (optional)
                this.Cursor = Cursors.WaitCursor;

                // 1. Fetch Data
                var loans = await _unitOfWork.Loans.GetAllAsync();
                var customers = await _unitOfWork.Customers.GetAllAsync();

                // 2. Tính toán thống kê
                decimal totalDisbursed = loans.Where(l => l.Status == DAL.Entities.LoanStatus.Approved).Sum(l => l.PrincipalAmount);
                int pendingCount = loans.Count(l => l.Status == DAL.Entities.LoanStatus.Pending);
                int customerCount = customers.Count();
                int overdueCount = loans.Count(l => l.Status == DAL.Entities.LoanStatus.Overdue);

                // 3. Update StatCards
                // Lưu ý: StatCard phải có phương thức UpdateData (hoặc tạo mới lại)
                // Ở đây mình clear đi add lại cho đơn giản, hoặc cast sang StatCard để update
                pnlStatsContainer.Controls.Clear();
                pnlStatsContainer.Controls.Add(new StatCard("Tổng Dư Nợ", $"{totalDisbursed:N0}", Color.FromArgb(0, 123, 255))); // Blue
                pnlStatsContainer.Controls.Add(new StatCard("Hồ Sơ Chờ", pendingCount.ToString(), Color.FromArgb(255, 193, 7))); // Yellow
                pnlStatsContainer.Controls.Add(new StatCard("Khách Hàng", customerCount.ToString(), Color.FromArgb(40, 167, 69))); // Green
                pnlStatsContainer.Controls.Add(new StatCard("Nợ Quá Hạn", overdueCount.ToString(), Color.FromArgb(220, 53, 69))); // Red

                // 4. Update Grid
                dgvLoans.Rows.Clear();
                // Join data in memory (đơn giản hóa)
                var query = from l in loans
                            join c in customers on l.CustomerId equals c.Id
                            orderby l.CreatedDate descending
                            select new { l, c };

                foreach (var item in query.Take(20)) // Lấy 20 bản ghi mới nhất
                {
                    dgvLoans.Rows.Add(
                        item.l.Id,
                        item.c.FullName,
                        item.l.PrincipalAmount,
                        $"{item.l.InterestRate}%",
                        $"{item.l.TermMonths} tháng",
                        item.l.Status.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}