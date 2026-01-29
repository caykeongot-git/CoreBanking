using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using CoreBanking.DAL.Repositories;
using CoreBanking.WinUI.Helpers;
using CoreBanking.WinUI.Forms;

namespace CoreBanking.WinUI.Controls
{
    public class CustomerControl : UserControl
    {
        private Panel _pnlTop;
        private DataGridView _dgv;
        private TextBox _txtSearch;
        private Button _btnAdd;

        public CustomerControl()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.WhiteSmoke; // Màu nền sáng nhẹ
            InitializeUI();

            // Đăng ký sự kiện Load để lấy dữ liệu khi control hiển thị
            this.Load += (s, e) => LoadData();
        }

        private void InitializeUI()
        {
            // --- TOP BAR ---
            _pnlTop = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };

            // Search Label & Box
            var lblSearch = new Label
            {
                Text = "Search (Name / Identity):",
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9)
            };

            _txtSearch = new TextBox
            {
                Location = new Point(20, 40),
                Width = 350,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtSearch.TextChanged += (s, e) => LoadData(); // Real-time search

            // Add Button
            _btnAdd = new Button
            {
                Text = "+ NEW CUSTOMER",
                BackColor = ThemeHelper.PrimaryColor, // Đồng bộ màu chủ đạo
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(180, 40),
                Location = new Point(this.Width - 220, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            _btnAdd.FlatAppearance.BorderSize = 0;
            _btnAdd.Click += (s, e) => OpenDetailForm(null); // Null = Add Mode

            _pnlTop.Controls.AddRange(new Control[] { lblSearch, _txtSearch, _btnAdd });

            // --- GRID VIEW ---
            Panel pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };

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
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 50 }, // Tăng chiều cao dòng cho thoáng
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                MultiSelect = false
            };

            // Style cho Grid
            _dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(240, 242, 245),
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding = new Padding(10),
                SelectionBackColor = Color.FromArgb(240, 242, 245), // Không đổi màu header khi select
                SelectionForeColor = Color.DimGray
            };

            _dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = ThemeHelper.PrimaryColor, // Màu select dòng
                SelectionForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Padding = new Padding(10, 0, 0, 0),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            // Định nghĩa cột
            _dgv.Columns.Add("Id", "# ID");
            _dgv.Columns.Add("Name", "FULL NAME");
            _dgv.Columns.Add("Identity", "IDENTITY NO.");
            _dgv.Columns.Add("Phone", "PHONE NUMBER");
            _dgv.Columns.Add("Income", "MONTHLY INCOME");
            _dgv.Columns.Add("Balance", "TOTAL BALANCE");

            // Format cột tiền tệ & căn lề
            _dgv.Columns["Income"].DefaultCellStyle.Format = "C0";
            _dgv.Columns["Balance"].DefaultCellStyle.Format = "C0";
            _dgv.Columns["Balance"].DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Context Menu (Chuột phải)
            var menu = new ContextMenuStrip();
            menu.Items.Add("Edit Details", null, (s, e) => EditSelected());
            menu.Items.Add("Refresh Data", null, (s, e) => LoadData());
            _dgv.ContextMenuStrip = menu;

            _dgv.DoubleClick += (s, e) => EditSelected();

            pnlBody.Controls.Add(_dgv);
            this.Controls.Add(pnlBody);
            this.Controls.Add(_pnlTop);
        }

        private async void LoadData()
        {
            try
            {
                // Hiển thị trạng thái loading nếu cần (ví dụ đổi con trỏ)
                this.Cursor = Cursors.WaitCursor;

                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    // 1. Lấy tất cả khách hàng
                    var list = await uow.Customers.GetAllAsync();

                    // 2. Lấy tất cả tài khoản (Chỉ lấy Id và Balance để tối ưu nếu cần, nhưng ở đây ta lấy hết cho đơn giản)
                    // PERFORMANCE FIX: Tránh gọi DB trong vòng lặp
                    var allAccounts = await uow.Accounts.GetAllAsync();

                    // 3. Filter Search (Client-side filtering)
                    string k = _txtSearch.Text.ToLower().Trim();
                    if (!string.IsNullOrEmpty(k))
                    {
                        list = list.Where(c =>
                            c.FullName.ToLower().Contains(k) ||
                            c.IdentityNumber.Contains(k) ||
                            (c.PhoneNumber != null && c.PhoneNumber.Contains(k))
                        );
                    }

                    // 4. Binding dữ liệu lên Grid
                    _dgv.Rows.Clear();

                    foreach (var c in list.OrderByDescending(x => x.Id))
                    {
                        // Tính toán trên RAM (Rất nhanh)
                        decimal totalBalance = allAccounts
                            .Where(a => a.CustomerId == c.Id)
                            .Sum(a => a.Balance);

                        _dgv.Rows.Add(
                            c.Id,
                            c.FullName,
                            c.IdentityNumber,
                            c.PhoneNumber ?? "N/A",
                            c.MonthlyIncome, // Grid sẽ tự format C0
                            totalBalance     // Grid sẽ tự format C0
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void EditSelected()
        {
            if (_dgv.SelectedRows.Count > 0)
            {
                // Lấy ID từ cột đầu tiên
                if (int.TryParse(_dgv.SelectedRows[0].Cells[0].Value?.ToString(), out int id))
                {
                    OpenDetailForm(id);
                }
            }
        }

        private void OpenDetailForm(int? id)
        {
            try
            {
                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    // Resolve Form từ DI Container để đảm bảo các dependencies được tiêm vào (nếu có)
                    var form = scope.ServiceProvider.GetRequiredService<CustomerDetailForm>();

                    form.CustomerId = id; // Truyền ID (Null = Add, Value = Edit)

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadData(); // Reload lại grid nếu có thay đổi

                        // Hiển thị thông báo nhỏ (Toast) nếu muốn
                        // MessageBox.Show("Data updated successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open form: " + ex.Message);
            }
        }
    }
}