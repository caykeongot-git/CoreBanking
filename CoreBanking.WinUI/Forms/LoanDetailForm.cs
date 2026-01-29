using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using CoreBanking.DAL.Entities;
using CoreBanking.DAL.Repositories;
using CoreBanking.WinUI.Helpers;

namespace CoreBanking.WinUI.Forms
{
    public class LoanDetailForm : Form
    {
        private ComboBox _cboCustomer;
        private TextBox _txtAmount;
        private NumericUpDown _numRate;
        private NumericUpDown _numTerm;
        private DataGridView _dgvSchedule;
        private Label _lblTotalInterest;
        private Label _lblMonthlyPay;
        private Button _btnSave;

        // Data
        private List<Customer> _customers;

        public LoanDetailForm()
        {
            this.Text = "New Loan Application";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeUI();
            LoadCustomers();
        }

        private void InitializeUI()
        {
            // --- LEFT PANEL: INPUT ---
            Panel pnlLeft = new Panel { Dock = DockStyle.Left, Width = 350, Padding = new Padding(30), BackColor = Color.WhiteSmoke };

            Label lblTitle = new Label { Text = "Loan Info", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = ThemeColor.Primary, Location = new Point(25, 20), AutoSize = true };

            // Customer
            Label lblCus = new Label { Text = "Customer", Location = new Point(30, 80), AutoSize = true };
            _cboCustomer = new ComboBox { Location = new Point(30, 105), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

            // Amount
            Label lblAmt = new Label { Text = "Principal Amount ($)", Location = new Point(30, 150), AutoSize = true };
            _txtAmount = new TextBox { Location = new Point(30, 175), Width = 280, Font = new Font("Segoe UI", 10), Text = "10000" };
            _txtAmount.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };

            // Interest
            Label lblRate = new Label { Text = "Interest Rate (% / Year)", Location = new Point(30, 220), AutoSize = true };
            _numRate = new NumericUpDown { Location = new Point(30, 245), Width = 280, Font = new Font("Segoe UI", 10), DecimalPlaces = 2, Value = 12 };

            // Term
            Label lblTerm = new Label { Text = "Term (Months)", Location = new Point(30, 290), AutoSize = true };
            _numTerm = new NumericUpDown { Location = new Point(30, 315), Width = 280, Font = new Font("Segoe UI", 10), Maximum = 360, Value = 12 };

            // Calc Button
            Button btnCalc = new Button { Text = "Calculate Schedule", Location = new Point(30, 370), Width = 280, Height = 40, BackColor = ThemeColor.Secondary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCalc.FlatAppearance.BorderSize = 0;
            btnCalc.Click += CalculateSchedule;

            // Save Button
            _btnSave = new Button { Text = "SUBMIT APPLICATION", Location = new Point(30, 480), Width = 280, Height = 50, BackColor = ThemeColor.Success, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += SaveLoan;

            pnlLeft.Controls.AddRange(new Control[] { lblTitle, lblCus, _cboCustomer, lblAmt, _txtAmount, lblRate, _numRate, lblTerm, _numTerm, btnCalc, _btnSave });

            // --- RIGHT PANEL: SCHEDULE PREVIEW ---
            Panel pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            Label lblPreview = new Label { Text = "Repayment Schedule Preview", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 40 };

            // Summary Stats
            Panel pnlStats = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.FromArgb(240, 248, 255) };
            _lblMonthlyPay = new Label { Text = "Monthly: $0", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = ThemeColor.Primary };
            _lblTotalInterest = new Label { Text = "Total Interest: $0", Location = new Point(250, 20), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = ThemeColor.Danger };
            pnlStats.Controls.Add(_lblMonthlyPay);
            pnlStats.Controls.Add(_lblTotalInterest);

            // Grid
            _dgvSchedule = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            _dgvSchedule.Columns.Add("Month", "Month");
            _dgvSchedule.Columns.Add("Principal", "Principal");
            _dgvSchedule.Columns.Add("Interest", "Interest");
            _dgvSchedule.Columns.Add("Total", "Total Pay");
            _dgvSchedule.Columns.Add("Balance", "Remaining");

            pnlRight.Controls.Add(_dgvSchedule);
            pnlRight.Controls.Add(pnlStats);
            pnlRight.Controls.Add(lblPreview);

            this.Controls.Add(pnlRight);
            this.Controls.Add(pnlLeft);
        }

        private async void LoadCustomers()
        {
            using (var scope = Program.ServiceProvider.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var list = await uow.Customers.GetAllAsync();
                _customers = list.ToList();

                _cboCustomer.Items.Clear();
                foreach (var c in _customers)
                    _cboCustomer.Items.Add($"{c.FullName} ({c.IdentityNumber})");

                if (_cboCustomer.Items.Count > 0) _cboCustomer.SelectedIndex = 0;
            }
        }

        // --- THUẬT TOÁN TÍNH LỊCH TRẢ NỢ (PMT Formula) ---
        private void CalculateSchedule(object sender, EventArgs e)
        {
            if (!decimal.TryParse(_txtAmount.Text, out decimal principal)) return;
            double rateYear = (double)_numRate.Value;
            int months = (int)_numTerm.Value;

            double rateMonth = rateYear / 12 / 100;

            // Công thức PMT: P * r * (1+r)^n / ((1+r)^n - 1)
            double pmt = (double)principal * rateMonth * Math.Pow(1 + rateMonth, months) / (Math.Pow(1 + rateMonth, months) - 1);

            decimal monthlyPay = (decimal)pmt;
            decimal balance = principal;
            decimal totalInterest = 0;

            _dgvSchedule.Rows.Clear();

            for (int i = 1; i <= months; i++)
            {
                decimal interest = balance * (decimal)rateMonth;
                decimal principalPay = monthlyPay - interest;
                balance -= principalPay;
                if (balance < 0) balance = 0; // Làm tròn số cuối

                totalInterest += interest;

                _dgvSchedule.Rows.Add(i, principalPay.ToString("N2"), interest.ToString("N2"), monthlyPay.ToString("N2"), balance.ToString("N2"));
            }

            _lblMonthlyPay.Text = $"Monthly: {monthlyPay:C2}";
            _lblTotalInterest.Text = $"Total Interest: {totalInterest:C2}";

            _btnSave.Enabled = true; // Cho phép lưu sau khi tính toán
        }

        private async void SaveLoan(object sender, EventArgs e)
        {
            if (_cboCustomer.SelectedIndex < 0) return;
            var customer = _customers[_cboCustomer.SelectedIndex];

            try
            {
                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var loan = new Loan
                    {
                        CustomerId = customer.Id,
                        PrincipalAmount = decimal.Parse(_txtAmount.Text),
                        InterestRate = (double)_numRate.Value,
                        TermMonths = (int)_numTerm.Value,
                        Status = LoanStatus.Pending,
                        CreatedDate = DateTime.Now
                    };

                    await uow.Loans.AddAsync(loan);
                    await uow.CompleteAsync(); // Save để lấy LoanId

                    // Lưu lịch trả nợ vào bảng RepaymentSchedule (Tùy chọn, để đơn giản ta chỉ lưu Loan Header trước)
                    // foreach (DataGridViewRow row in _dgvSchedule.Rows) { ... }

                    MessageBox.Show("Application submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving loan: " + ex.Message);
            }
        }
    }
}