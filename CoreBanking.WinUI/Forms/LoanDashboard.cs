using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CoreBanking.DAL.Repositories;
using CoreBanking.WinUI.Controls;
using CoreBanking.WinUI.Helpers;

namespace CoreBanking.WinUI.UI
{
    public partial class LoanDashboard : Form
    {
        private readonly IUnitOfWork _unitOfWork;

        // UI Layout Containers
        private Panel pnlContainer;
        private FlowLayoutPanel pnlTopStats;
        private TableLayoutPanel tblMainContent;

        // Chart & Grid
        private ModernLiveChart chartRevenue;
        private DataGridView dgvLoans;

        // Data & Timer
        private System.Windows.Forms.Timer timerUpdate;
        private Random _rnd = new Random();

        public LoanDashboard() { InitializeComponent(); }

        public LoanDashboard(IUnitOfWork unitOfWork) : this()
        {
            _unitOfWork = unitOfWork;
            this.Load += async (s, e) => await LoadDataAsync();

            // Timer giả lập real-time chart
            timerUpdate = new System.Windows.Forms.Timer { Interval = 1000 };
            timerUpdate.Tick += (s, e) => chartRevenue.PushValue(_rnd.Next(50, 200));
            timerUpdate.Start();
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(240, 242, 245); // Nền xám nhạt chuẩn HMS
            this.AutoScroll = true; // Cho phép cuộn nếu màn hình bé

            // 1. Container chính (để tránh bị che bởi menu)
            pnlContainer = new Panel();
            pnlContainer.Dock = DockStyle.Top; // Dock Top để AutoScroll hoạt động đúng
            pnlContainer.AutoSize = true;
            pnlContainer.Padding = new Padding(20);
            this.Controls.Add(pnlContainer);

            // 2. Tiêu đề Dashboard
            Label lblTitle = new Label();
            lblTitle.Text = "TỔNG QUAN TÍN DỤNG";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = ThemeHelper.PrimaryColor;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 40;
            pnlContainer.Controls.Add(lblTitle);

            // 3. Hàng Thống kê (Stat Cards)
            pnlTopStats = new FlowLayoutPanel();
            pnlTopStats.Dock = DockStyle.Top;
            pnlTopStats.Height = 130;
            pnlTopStats.FlowDirection = FlowDirection.LeftToRight;
            pnlTopStats.WrapContents = false;
            pnlTopStats.AutoSize = true;
            pnlContainer.Controls.Add(pnlTopStats);

            // 4. Nội dung chính (Chia cột: Chart bên trái, Grid bên phải)
            tblMainContent = new TableLayoutPanel();
            tblMainContent.Dock = DockStyle.Top;
            tblMainContent.Height = 500;
            tblMainContent.ColumnCount = 2;
            tblMainContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); // Chart chiếm 60%
            tblMainContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); // Grid chiếm 40%
            tblMainContent.Padding = new Padding(0, 20, 0, 0); // Cách stat card 20px
            pnlContainer.Controls.Add(tblMainContent);

            // --- CHART SECTION (Trái) ---
            Panel pnlChartWrapper = new RoundedPanel();
            pnlChartWrapper.Dock = DockStyle.Fill;
            pnlChartWrapper.BackColor = Color.White;
            pnlChartWrapper.Padding = new Padding(15);
            pnlChartWrapper.Margin = new Padding(0, 0, 10, 0); // Margin phải

            Label lblChart = new Label { Text = "Biểu đồ giải ngân (Real-time)", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 30, ForeColor = Color.Gray };
            chartRevenue = new ModernLiveChart { Dock = DockStyle.Fill };

            pnlChartWrapper.Controls.Add(chartRevenue);
            pnlChartWrapper.Controls.Add(lblChart);
            tblMainContent.Controls.Add(pnlChartWrapper, 0, 0);

            // --- GRID SECTION (Phải) ---
            Panel pnlGridWrapper = new RoundedPanel();
            pnlGridWrapper.Dock = DockStyle.Fill;
            pnlGridWrapper.BackColor = Color.White;
            pnlGridWrapper.Padding = new Padding(15);
            pnlGridWrapper.Margin = new Padding(10, 0, 0, 0); // Margin trái

            Label lblGrid = new Label { Text = "Hồ sơ mới nhất", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 30, ForeColor = Color.Gray };
            dgvLoans = new DataGridView();
            dgvLoans.Dock = DockStyle.Fill;
            GridHelper.StyleGrid(dgvLoans); // Style chuẩn HMS

            pnlGridWrapper.Controls.Add(dgvLoans);
            pnlGridWrapper.Controls.Add(lblGrid);
            tblMainContent.Controls.Add(pnlGridWrapper, 1, 0);
        }

        private async Task LoadDataAsync()
        {
            if (_unitOfWork == null) return;

            var loans = await _unitOfWork.Loans.GetAllAsync();
            var customers = await _unitOfWork.Customers.GetAllAsync();

            // 1. Fill Stat Cards
            pnlTopStats.Controls.Clear();
            AddStatCard("TỔNG DƯ NỢ", $"{loans.Sum(x => x.PrincipalAmount):N0} $", Color.FromArgb(0, 123, 255));
            AddStatCard("LÃI SUẤT TB", "12.5 %", Color.FromArgb(255, 193, 7));
            AddStatCard("KHÁCH HÀNG", $"{customers.Count()}", Color.FromArgb(40, 167, 69));
            AddStatCard("HỒ SƠ CHỜ", $"{loans.Count(l => l.Status == DAL.Entities.LoanStatus.Pending)}", Color.FromArgb(220, 53, 69));

            // 2. Fill Grid
            dgvLoans.DataSource = loans.OrderByDescending(x => x.CreatedDate).Take(15).Select(x => new
            {
                ID = x.Id,
                Amount = x.PrincipalAmount.ToString("N0"),
                Term = x.TermMonths + "T",
                Status = x.Status.ToString()
            }).ToList();
        }

        private void AddStatCard(string title, string value, Color color)
        {
            var card = new RoundedPanel { Size = new Size(220, 110), BackColor = Color.White, Margin = new Padding(0, 0, 15, 0) };

            Panel pnlColor = new Panel { Size = new Size(5, 60), BackColor = color, Location = new Point(0, 25) };
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(15, 20), AutoSize = true };
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = ThemeHelper.TextDark, Location = new Point(15, 45), AutoSize = true };

            card.Controls.Add(pnlColor);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            pnlTopStats.Controls.Add(card);
        }
    }

    // --- MODERN CHART CONTROL (GDI+ NÂNG CAO) ---
    public class ModernLiveChart : Control
    {
        private List<int> dataPoints = new List<int>();
        private int maxPoints = 30;

        public ModernLiveChart()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            // Init data giả
            var r = new Random();
            for (int i = 0; i < maxPoints; i++) dataPoints.Add(r.Next(50, 150));
        }

        public void PushValue(int val)
        {
            dataPoints.Add(val);
            if (dataPoints.Count > maxPoints) dataPoints.RemoveAt(0);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(BackColor);

            if (dataPoints.Count < 2) return;

            float w = Width;
            float h = Height;
            float stepX = w / (maxPoints - 1);
            float maxVal = dataPoints.Max() * 1.1f; // Buffer 10% đỉnh
            if (maxVal == 0) maxVal = 100;

            // 1. Vẽ Grid ngang
            using (Pen penGrid = new Pen(Color.FromArgb(240, 240, 240), 1))
            {
                for (int i = 0; i <= 5; i++)
                {
                    float y = h - (i * (h / 5));
                    e.Graphics.DrawLine(penGrid, 0, y, w, y);
                }
            }

            // 2. Tính toán điểm
            PointF[] points = new PointF[dataPoints.Count];
            for (int i = 0; i < dataPoints.Count; i++)
            {
                points[i] = new PointF(i * stepX, h - (dataPoints[i] / maxVal * h));
            }

            // 3. Vẽ vùng Gradient dưới đường line (Area Chart)
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddLines(points);
                path.AddLine(points.Last().X, h, points[0].X, h); // Đóng path xuống đáy
                path.CloseFigure();

                using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle,
                    Color.FromArgb(100, 0, 120, 215), Color.FromArgb(10, 0, 120, 215), 90F))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }

            // 4. Vẽ đường Line chính (Bezier Curve cho mượt)
            using (Pen penLine = new Pen(Color.FromArgb(0, 120, 215), 2.5f))
            {
                // Mẹo: Vẽ Curve thay vì Line gãy khúc
                if (points.Length > 2) e.Graphics.DrawCurve(penLine, points);
                else e.Graphics.DrawLines(penLine, points);
            }
        }
    }
}