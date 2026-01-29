using System;
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
        // Property để nhận ID từ LoanControl truyền sang
        public int? LoanId { get; set; }

        private ComboBox _cbCustomer;
        private NumericUpDown _numAmount;
        private NumericUpDown _numInterest;
        private NumericUpDown _numDuration;
        private ComboBox _cbStatus;
        private Button _btnSave;
        private Button _btnCancel;

        public LoanDetailForm()
        {
            this.Text = "Loan Details";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeUI();
            this.Load += LoanDetailForm_Load;
        }

        private void InitializeUI()
        {
            var layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(30);
            layout.ColumnCount = 1;
            layout.RowCount = 7;

            // Định nghĩa chiều cao các hàng
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Title
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Customer
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Amount
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Interest
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Duration
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Status
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Buttons

            // Title
            Label lblTitle = new Label { Text = "Loan Information", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = ThemeHelper.PrimaryColor, AutoSize = true };
            layout.Controls.Add(lblTitle);

            // 1. Customer Selection
            _cbCustomer = new ComboBox { Dock = DockStyle.Bottom, Height = 35, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            layout.Controls.Add(CreateField("Customer (*)", _cbCustomer));

            // 2. Amount
            _numAmount = new NumericUpDown { Dock = DockStyle.Bottom, Height = 35, Font = new Font("Segoe UI", 11), Maximum = 10000000000, Increment = 1000000, ThousandsSeparator = true };
            layout.Controls.Add(CreateField("Loan Amount ($) (*)", _numAmount));

            // 3. Interest Rate
            _numInterest = new NumericUpDown { Dock = DockStyle.Bottom, Height = 35, Font = new Font("Segoe UI", 11), Maximum = 100, DecimalPlaces = 2 };
            layout.Controls.Add(CreateField("Interest Rate (%)", _numInterest));

            // 4. Duration
            _numDuration = new NumericUpDown { Dock = DockStyle.Bottom, Height = 35, Font = new Font("Segoe UI", 11), Maximum = 360, Minimum = 1, Value = 12 };
            layout.Controls.Add(CreateField("Duration (Months)", _numDuration));

            // 5. Status
            _cbStatus = new ComboBox { Dock = DockStyle.Bottom, Height = 35, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            _cbStatus.DataSource = Enum.GetValues(typeof(LoanStatus));
            layout.Controls.Add(CreateField("Status", _cbStatus));

            // Buttons
            FlowLayoutPanel pnlBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 20, 0, 0) };
            _btnSave = new Button { Text = "SAVE LOAN", Width = 140, Height = 45 };
            ThemeHelper.ApplyPrimaryButtonStyle(_btnSave);
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button { Text = "Cancel", Width = 100, Height = 45, Margin = new Padding(10, 0, 0, 0) };
            ThemeHelper.ApplySecondaryButtonStyle(_btnCancel);
            _btnCancel.Click += (s, e) => this.Close();

            pnlBtn.Controls.Add(_btnSave);
            pnlBtn.Controls.Add(_btnCancel);
            layout.Controls.Add(pnlBtn);

            this.Controls.Add(layout);
        }

        private Panel CreateField(string label, Control input)
        {
            Panel p = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15) };
            Label l = new Label { Text = label, Dock = DockStyle.Top, Height = 25, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9) };
            p.Controls.Add(input);
            p.Controls.Add(l);
            return p;
        }

        private async void LoanDetailForm_Load(object sender, EventArgs e)
        {
            try
            {
                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    // Load Customers for ComboBox
                    var customers = await uow.Customers.GetAllAsync();
                    _cbCustomer.DataSource = customers.ToList();
                    _cbCustomer.DisplayMember = "FullName";
                    _cbCustomer.ValueMember = "Id";

                    // Load Loan Data if Edit Mode
                    if (LoanId.HasValue)
                    {
                        var loan = await uow.Loans.GetByIdAsync(LoanId.Value);
                        if (loan != null)
                        {
                            _cbCustomer.SelectedValue = loan.CustomerId;
                            _numAmount.Value = loan.Amount;
                            _numInterest.Value = loan.InterestRate;
                            _numDuration.Value = loan.DurationMonth;
                            _cbStatus.SelectedItem = loan.Status;

                            _btnSave.Text = "UPDATE LOAN";
                            this.Text = $"Edit Loan #{loan.Id}";
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading data: " + ex.Message); }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (_cbCustomer.SelectedValue == null)
            {
                MessageBox.Show("Please select a customer.");
                return;
            }

            try
            {
                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    Loan loan;

                    if (LoanId.HasValue)
                    {
                        loan = await uow.Loans.GetByIdAsync(LoanId.Value);
                        if (loan == null) return;
                    }
                    else
                    {
                        loan = new Loan { StartDate = DateTime.Now };
                        await uow.Loans.AddAsync(loan);
                    }

                    // Update properties
                    loan.CustomerId = (int)_cbCustomer.SelectedValue;
                    loan.Amount = _numAmount.Value;
                    loan.InterestRate = _numInterest.Value;
                    loan.DurationMonth = (int)_numDuration.Value;
                    loan.Status = (LoanStatus)_cbStatus.SelectedItem;

                    // Simple logic: EndDate = StartDate + Duration
                    loan.EndDate = loan.StartDate.AddMonths(loan.DurationMonth);

                    if (LoanId.HasValue) uow.Loans.Update(loan);

                    await uow.CompleteAsync();

                    MessageBox.Show("Loan saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error saving: " + ex.Message); }
        }
    }
}