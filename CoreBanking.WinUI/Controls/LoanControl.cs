using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoreBanking.DAL.Entities; // Entity mới
using CoreBanking.DAL.Repositories; // Repository mới
using CoreBanking.WinUI.Forms;
using CoreBanking.WinUI.Helpers; // ThemeHelper mới

namespace CoreBanking.WinUI.Controls
{
    public class LoanControl : UserControl
    {
        private FlowLayoutPanel _flowLoans;
        private TextBox _txtSearch;
        private ContextMenuStrip _contextMenu;
        private Loan _selectedLoan;

        public LoanControl()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ThemeHelper.Background;

            InitializeUI();

            // Hook event Load để load dữ liệu khi control hiển thị
            this.Load += (s, e) => LoadDataFromDB();
        }

        private void InitializeUI()
        {
            // --- HEADER ---
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 10, 20, 10)
            };

            Label lblTitle = new Label
            {
                Text = "Loan Management",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = ThemeHelper.TextDark,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Add Button
            Button btnAdd = new Button
            {
                Text = "+ New Loan",
                BackColor = ThemeHelper.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(150, 45),
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Location = new Point(this.Width - 190, 20); // Anchor Right logic
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Click += BtnAdd_Click;

            // Search Box Panel
            Panel pnlSearch = new Panel
            {
                Size = new Size(300, 45),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.Width - 510, 20)
            };
            // Bo tròn panel search một chút cho đẹp (Optional)
            // ThemeHelper.SetRoundedCorners(pnlSearch, 10); 

            _txtSearch = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11),
                Location = new Point(15, 12),
                Width = 270,
                PlaceholderText = "Search by Customer or ID..."
            };
            _txtSearch.TextChanged += (s, e) => LoadDataFromDB();
            pnlSearch.Controls.Add(_txtSearch);

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(pnlSearch);
            pnlHeader.Controls.Add(btnAdd);

            // --- CONTEXT MENU ---
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("✏ View Details / Edit", null, OnEditLoan);
            var itemDel = _contextMenu.Items.Add("🗑 Delete Loan", null, OnDeleteLoan);
            itemDel.ForeColor = ThemeHelper.DangerColor;

            // --- BODY (FLOW LAYOUT) ---
            _flowLoans = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(30, 10, 0, 0),
                BackColor = ThemeHelper.Background
            };

            this.Controls.Add(_flowLoans);
            this.Controls.Add(pnlHeader);

            // Responsive logic (giống code cũ)
            this.Resize += (s, e) =>
            {
                btnAdd.Location = new Point(this.Width - 190, 20);
                pnlSearch.Location = new Point(this.Width - 510, 20);
            };
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Mở form Detail ở chế độ Add (Id = null)
            OpenLoanDetails(null);
        }

        private void OnEditLoan(object sender, EventArgs e)
        {
            if (_selectedLoan == null) return;
            OpenLoanDetails(_selectedLoan.Id);
        }

        private void OpenLoanDetails(int? loanId)
        {
            try
            {
                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    // Resolve Form từ DI
                    // Lưu ý: Bạn cần tạo LoanDetailForm ở bước sau
                    var form = scope.ServiceProvider.GetRequiredService<LoanDetailForm>();
                    form.LoanId = loanId;

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadDataFromDB(); // Reload data sau khi đóng form
                        MessageBox.Show("Operation completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening form: " + ex.Message);
            }
        }

        private async void OnDeleteLoan(object sender, EventArgs e)
        {
            if (_selectedLoan == null) return;

            if (MessageBox.Show($"Are you sure you want to delete Loan #{_selectedLoan.Id}?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (var scope = Program.ServiceProvider.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var loan = await uow.Loans.GetByIdAsync(_selectedLoan.Id);

                        if (loan != null)
                        {
                            uow.Loans.Remove(loan);
                            await uow.CompleteAsync();

                            LoadDataFromDB();
                            // ToastForm.Show("Success", "Loan deleted!"); // Nếu có ToastForm
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot delete loan. It might have related transactions.\nError: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // --- LOAD DATA (Refactored to use UnitOfWork) ---
        private async void LoadDataFromDB()
        {
            _flowLoans.Controls.Clear();
            _flowLoans.SuspendLayout(); // Tối ưu hiệu năng render

            try
            {
                // Hiển thị cursor loading
                this.Cursor = Cursors.WaitCursor;

                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    // 1. Lấy dữ liệu kèm Include Customer
                    // Lưu ý: Repository Generic cần hỗ trợ Include, hoặc ta dùng logic code
                    // Ở đây giả định GetAllAsync lấy raw, ta sẽ filter sau hoặc repo đã include
                    var allLoans = await uow.Loans.GetAllAsync();
                    var customers = await uow.Customers.GetAllAsync(); // Load Customers để map tên

                    // Join logic (Client-side join cho đơn giản, nếu data lớn nên join ở DB)
                    var query = from l in allLoans
                                join c in customers on l.CustomerId equals c.Id
                                select new { Loan = l, CustomerName = c.FullName };

                    // 2. Filter Search
                    string key = _txtSearch.Text.Trim().ToLower();
                    if (!string.IsNullOrEmpty(key))
                    {
                        query = query.Where(x =>
                            x.CustomerName.ToLower().Contains(key) ||
                            x.Loan.Id.ToString().Contains(key)
                        );
                    }

                    var resultList = query.OrderByDescending(x => x.Loan.Id).ToList();

                    // 3. Render Cards
                    foreach (var item in resultList)
                    {
                        AddLoanCard(item.Loan, item.CustomerName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading loans: " + ex.Message);
            }
            finally
            {
                _flowLoans.ResumeLayout();
                this.Cursor = Cursors.Default;
            }
        }

        private void AddLoanCard(Loan loan, string customerName)
        {
            // --- MAPPING MÀU SẮC TRẠNG THÁI ---
            Color statusColor = ThemeHelper.SecondaryColor;
            string statusText = loan.Status.ToString();

            switch (loan.Status)
            {
                case LoanStatus.Active:
                    statusColor = ThemeHelper.PrimaryColor; // Xanh dương (Đang vay)
                    break;
                case LoanStatus.Pending:
                    statusColor = ThemeHelper.WarningColor; // Vàng (Chờ duyệt)
                    break;
                case LoanStatus.Paid:
                    statusColor = ThemeHelper.SuccessColor; // Xanh lá (Đã trả)
                    break;
                case LoanStatus.BadDebt:
                    statusColor = ThemeHelper.DangerColor;  // Đỏ (Nợ xấu)
                    break;
                case LoanStatus.Rejected:
                    statusColor = Color.Gray;
                    break;
            }

            // --- BUILD CARD UI ---
            // Card Container
            Panel card = new Panel
            {
                Size = new Size(240, 150), // Size thẻ to hơn xíu để chứa thông tin tiền
                BackColor = Color.White,
                Margin = new Padding(0, 0, 25, 25), // Margin giữa các thẻ
                Tag = loan // Lưu object để xử lý click
            };

            // Hàm helper để gắn sự kiện click cho mọi control con
            void AttachEvent(Control c)
            {
                c.ContextMenuStrip = _contextMenu;
                c.Cursor = Cursors.Hand;
                c.MouseDown += (s, e) => {
                    if (e.Button == MouseButtons.Right) _selectedLoan = loan;
                    else if (e.Button == MouseButtons.Left) OpenLoanDetails(loan.Id);
                };
            }

            AttachEvent(card);

            // Dải màu bên trái (Status Strip)
            Panel pnlStatus = new Panel
            {
                Dock = DockStyle.Left,
                Width = 8,
                BackColor = statusColor
            };
            AttachEvent(pnlStatus);

            // Loan ID (Góc trên)
            Label lblId = new Label
            {
                Text = $"LOAN #{loan.Id}",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(20, 15),
                AutoSize = true
            };
            AttachEvent(lblId);

            // Money Amount (Nổi bật nhất)
            Label lblAmount = new Label
            {
                Text = loan.Amount.ToString("C0"), // Format tiền tệ
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ThemeHelper.TextDark,
                Location = new Point(18, 35),
                AutoSize = true
            };
            AttachEvent(lblAmount);

            // Customer Name
            Label lblCustomer = new Label
            {
                Text = customerName.ToUpper(),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = ThemeHelper.PrimaryColor,
                Location = new Point(20, 75),
                AutoSize = true
            };
            AttachEvent(lblCustomer);

            // Status Badge (Text)
            Label lblStatus = new Label
            {
                Text = statusText.ToUpper(),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = statusColor,
                Location = new Point(20, 110),
                AutoSize = false,
                Width = 200,
                TextAlign = ContentAlignment.MiddleRight
            };
            AttachEvent(lblStatus);

            // Thêm controls vào card
            card.Controls.Add(lblId);
            card.Controls.Add(lblAmount);
            card.Controls.Add(lblCustomer);
            card.Controls.Add(lblStatus);
            card.Controls.Add(pnlStatus);

            _flowLoans.Controls.Add(card);
        }
    }
}