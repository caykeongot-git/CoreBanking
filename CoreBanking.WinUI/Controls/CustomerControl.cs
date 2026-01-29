using System;
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
            this.BackColor = ThemeColor.Background;
            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            // --- TOP BAR ---
            _pnlTop = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(20) };

            // Search
            var lblSearch = new Label { Text = "Search (Name / ID):", Location = new Point(20, 20), AutoSize = true, ForeColor = Color.Gray };
            _txtSearch = new TextBox { Location = new Point(20, 45), Width = 300, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            _txtSearch.TextChanged += (s, e) => LoadData();

            // Add Button
            _btnAdd = new Button
            {
                Text = "+ Add Customer",
                BackColor = ThemeColor.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(150, 35),
                Location = new Point(this.Width - 190, 40),
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 45 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };

            // Style
            _dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 247, 251), ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Bold), Padding = new Padding(10) };
            _dgv.DefaultCellStyle = new DataGridViewCellStyle { SelectionBackColor = ThemeColor.Primary, SelectionForeColor = Color.White, Font = new Font("Segoe UI", 10), Padding = new Padding(10, 0, 0, 0), ForeColor = Color.FromArgb(64, 64, 64) };

            // Columns
            _dgv.Columns.Add("Id", "#");
            _dgv.Columns.Add("Name", "FULL NAME");
            _dgv.Columns.Add("Identity", "IDENTITY NO.");
            _dgv.Columns.Add("Phone", "PHONE");
            _dgv.Columns.Add("Income", "INCOME");
            _dgv.Columns.Add("Balance", "TOTAL BALANCE"); // Tính tổng tiền

            // Context Menu (Sửa/Xóa)
            var menu = new ContextMenuStrip();
            menu.Items.Add("Edit Customer", null, (s, e) => EditSelected());
            menu.Items.Add("Refresh List", null, (s, e) => LoadData());
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
                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var list = await uow.Customers.GetAllAsync();

                    // Search Filter
                    string k = _txtSearch.Text.ToLower();
                    if (!string.IsNullOrEmpty(k))
                    {
                        list = list.Where(c => c.FullName.ToLower().Contains(k) || c.IdentityNumber.Contains(k));
                    }

                    _dgv.Rows.Clear();
                    foreach (var c in list.OrderByDescending(x => x.Id))
                    {
                        // Tính tổng số dư (Cần load Accounts của customer này nếu chưa include)
                        // Ở đây ta giả định lazy loading hoặc query riêng nếu cần chính xác
                        var accounts = await uow.Accounts.FindAsync(a => a.CustomerId == c.Id);
                        decimal totalBalance = accounts.Sum(a => a.Balance);

                        _dgv.Rows.Add(
                            c.Id,
                            c.FullName,
                            c.IdentityNumber,
                            c.PhoneNumber,
                            c.MonthlyIncome.ToString("C0"),
                            totalBalance.ToString("C0")
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void EditSelected()
        {
            if (_dgv.SelectedRows.Count > 0)
            {
                int id = int.Parse(_dgv.SelectedRows[0].Cells[0].Value.ToString());
                OpenDetailForm(id);
            }
        }

        private void OpenDetailForm(int? id)
        {
            using (var scope = Program.ServiceProvider.CreateScope())
            {
                var form = scope.ServiceProvider.GetRequiredService<CustomerDetailForm>();
                form.CustomerId = id; // Truyền ID vào form
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }
    }
}