using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using CoreBanking.DAL.Entities;
using CoreBanking.DAL.Repositories;
using CoreBanking.WinUI.Helpers;
using CoreBanking.WinUI.Forms; // Để gọi LoanDetailForm

namespace CoreBanking.WinUI.Controls
{
    public class LoanControl : UserControl
    {
        private Panel _pnlTop;
        private Panel _pnlBody;
        private DataGridView _dgv;
        private TextBox _txtSearch;
        private ComboBox _cboStatus;
        private Button _btnAdd;

        public LoanControl()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ThemeColor.Background;
            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            // 1. TOP BAR (Search & Filter)
            _pnlTop = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };

            // Search Box
            var lblSearch = new Label { Text = "Search Customer:", Location = new Point(20, 20), AutoSize = true, ForeColor = Color.Gray };
            _txtSearch = new TextBox { Location = new Point(20, 45), Width = 250, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            _txtSearch.TextChanged += (s, e) => LoadData();

            // Status Filter
            var lblStatus = new Label { Text = "Status:", Location = new Point(300, 20), AutoSize = true, ForeColor = Color.Gray };
            _cboStatus = new ComboBox { Location = new Point(300, 45), Width = 150, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            _cboStatus.Items.Add("All");
            foreach (var status in Enum.GetValues(typeof(LoanStatus))) _cboStatus.Items.Add(status);
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => LoadData();

            // Add Button
            _btnAdd = new Button
            {
                Text = "+ New Loan",
                BackColor = ThemeColor.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(120, 35),
                Location = new Point(this.Width - 160, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            _btnAdd.FlatAppearance.BorderSize = 0;
            _btnAdd.Click += (s, e) =>
            {
                using (var form = Program.ServiceProvider.GetRequiredService<LoanDetailForm>())
                {
                    if (form.ShowDialog() == DialogResult.OK) LoadData();
                }
            };

            _pnlTop.Controls.AddRange(new Control[] { lblSearch, _txtSearch, lblStatus, _cboStatus, _btnAdd });

            // 2. GRID VIEW
            _pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };

            _dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                GridColor = Color.WhiteSmoke,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 45 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };

            // Style Header
            _dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(245, 247, 251),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding = new Padding(10)
            };
            _dgv.ColumnHeadersHeight = 45;

            // Style Rows
            _dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = ThemeColor.Primary,
                SelectionForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Padding = new Padding(10, 0, 0, 0),
                ForeColor = Color.FromArgb(64, 64, 64)
            };

            // Columns
            _dgv.Columns.Add("Id", "#");
            _dgv.Columns.Add("Customer", "CUSTOMER");
            _dgv.Columns.Add("Amount", "AMOUNT");
            _dgv.Columns.Add("Rate", "RATE (%)");
            _dgv.Columns.Add("Term", "TERM (M)");
            _dgv.Columns.Add("Date", "CREATED DATE");
            _dgv.Columns.Add("Status", "STATUS"); // Cột này sẽ tô màu

            // Context Menu (Chuột phải để duyệt)
            var menu = new ContextMenuStrip();
            var itemApprove = menu.Items.Add("Approve Loan");
            var itemReject = menu.Items.Add("Reject Loan");
            var itemDetail = menu.Items.Add("View Details");

            itemApprove.Click += (s, e) => ChangeStatus(LoanStatus.Approved);
            itemReject.Click += (s, e) => ChangeStatus(LoanStatus.Rejected);
            _dgv.ContextMenuStrip = menu;

            _pnlBody.Controls.Add(_dgv);

            this.Controls.Add(_pnlBody);
            this.Controls.Add(_pnlTop);
        }

        private async void LoadData()
        {
            try
            {
                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var loans = await uow.Loans.GetAllAsync(); // Nên viết hàm GetAllIncludesCustomerAsync trong Repo

                    // Filter
                    var query = loans.AsQueryable();

                    if (!string.IsNullOrEmpty(_txtSearch.Text))
                    {
                        string k = _txtSearch.Text.ToLower();
                        // Lưu ý: Cần Include Customer ở tầng DAL để search theo tên
                        // Ở đây demo nên giả định load hết rồi filter client
                        query = query.Where(l => l.Id.ToString().Contains(k));
                    }

                    if (_cboStatus.SelectedIndex > 0 && _cboStatus.SelectedItem is LoanStatus status)
                    {
                        query = query.Where(l => l.Status == status);
                    }

                    var list = query.OrderByDescending(x => x.CreatedDate).ToList();

                    _dgv.Rows.Clear();
                    foreach (var l in list)
                    {
                        // Giả sử CustomerId mapping tay nếu chưa có Include
                        var customerName = (await uow.Customers.GetByIdAsync(l.CustomerId))?.FullName ?? "Unknown";

                        int idx = _dgv.Rows.Add(
                            l.Id,
                            customerName,
                            l.PrincipalAmount.ToString("C0"),
                            l.InterestRate + "%",
                            l.TermMonths,
                            l.CreatedDate.ToString("dd/MM/yyyy"),
                            l.Status
                        );

                        // Tô màu trạng thái
                        var cell = _dgv.Rows[idx].Cells["Status"];
                        cell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                        if (l.Status == LoanStatus.Pending) cell.Style.ForeColor = ThemeColor.Warning;
                        else if (l.Status == LoanStatus.Approved || l.Status == LoanStatus.Active) cell.Style.ForeColor = ThemeColor.Success;
                        else if (l.Status == LoanStatus.Rejected || l.Status == LoanStatus.BadDebt) cell.Style.ForeColor = ThemeColor.Danger;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading loans: " + ex.Message);
            }
        }

        private async void ChangeStatus(LoanStatus newStatus)
        {
            if (_dgv.SelectedRows.Count == 0) return;
            int id = int.Parse(_dgv.SelectedRows[0].Cells[0].Value.ToString());

            try
            {
                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var loan = await uow.Loans.GetByIdAsync(id);
                    if (loan != null)
                    {
                        // Logic nghiệp vụ: Chỉ Pending mới được Approve/Reject
                        if (loan.Status != LoanStatus.Pending)
                        {
                            MessageBox.Show("Only pending loans can be processed.");
                            return;
                        }

                        loan.Status = newStatus;
                        if (newStatus == LoanStatus.Approved)
                        {
                            loan.Status = LoanStatus.Active; // Duyệt là giải ngân luôn (giả lập)
                            // TODO: Cộng tiền vào tài khoản khách ở đây
                        }

                        uow.Loans.Update(loan);
                        await uow.CompleteAsync();
                        LoadData();
                        MessageBox.Show($"Loan #{id} has been {newStatus}.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}