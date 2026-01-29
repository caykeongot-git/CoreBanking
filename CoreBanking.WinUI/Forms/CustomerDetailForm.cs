using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using CoreBanking.DAL.Entities;
using CoreBanking.DAL.Repositories;
using CoreBanking.WinUI.Helpers;

namespace CoreBanking.WinUI.Forms
{
    public class CustomerDetailForm : Form
    {
        public int? CustomerId { get; set; }

        private TextBox _txtName;
        private TextBox _txtIdentity;
        private TextBox _txtPhone;
        private TextBox _txtEmail;
        private TextBox _txtIncome;
        private Button _btnSave;
        private Button _btnCancel;

        public CustomerDetailForm()
        {
            this.Text = "Customer Details";
            this.Size = new Size(550, 600); // Tăng kích thước form
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeUI();
            this.Load += CustomerDetailForm_Load;
        }

        private void InitializeUI()
        {
            // Sử dụng TableLayoutPanel để layout tự động, tránh chồng chéo
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(30);
            layout.ColumnCount = 1;
            layout.RowCount = 8; // Title + 5 Fields + Buttons

            // Cấu hình chiều cao các hàng
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Title
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Name
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Identity
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Phone
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Email
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Income
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Spacer
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Buttons

            // Title
            Label lblTitle = new Label
            {
                Text = "Customer Information",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ThemeColor.Primary,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            layout.Controls.Add(lblTitle, 0, 0);

            // Fields
            _txtName = AddFieldToLayout(layout, "Full Name (*)", 1);
            _txtIdentity = AddFieldToLayout(layout, "Identity Number (CMND/CCCD) (*)", 2);
            _txtPhone = AddFieldToLayout(layout, "Phone Number", 3);
            _txtEmail = AddFieldToLayout(layout, "Email Address", 4);
            _txtIncome = AddFieldToLayout(layout, "Monthly Income ($)", 5);

            // Buttons Panel
            FlowLayoutPanel pnlButtons = new FlowLayoutPanel();
            pnlButtons.Dock = DockStyle.Fill;
            pnlButtons.FlowDirection = FlowDirection.RightToLeft;
            pnlButtons.Margin = new Padding(0);

            _btnSave = new Button
            {
                Text = "SAVE",
                Width = 150,
                Height = 45,
                BackColor = ThemeColor.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button
            {
                Text = "Cancel",
                Width = 120,
                Height = 45,
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) => this.Close();

            pnlButtons.Controls.Add(_btnSave);
            pnlButtons.Controls.Add(_btnCancel);

            layout.Controls.Add(pnlButtons, 0, 7);

            this.Controls.Add(layout);
        }

        private TextBox AddFieldToLayout(TableLayoutPanel layout, string labelText, int row)
        {
            Panel container = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };

            Label lbl = new Label
            {
                Text = labelText,
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.BottomLeft,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9)
            };

            TextBox txt = new TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 35, // Chiều cao TextBox
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };

            container.Controls.Add(txt);
            container.Controls.Add(lbl);

            layout.Controls.Add(container, 0, row);
            return txt;
        }

        private async void CustomerDetailForm_Load(object sender, EventArgs e)
        {
            if (CustomerId.HasValue)
            {
                this.Text = $"Edit Customer #{CustomerId}";
                _btnSave.Text = "UPDATE";

                try
                {
                    using (var scope = Program.ServiceProvider.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var c = await uow.Customers.GetByIdAsync(CustomerId.Value);
                        if (c != null)
                        {
                            _txtName.Text = c.FullName;
                            _txtIdentity.Text = c.IdentityNumber;
                            _txtPhone.Text = c.PhoneNumber;
                            _txtEmail.Text = c.Email;
                            _txtIncome.Text = c.MonthlyIncome.ToString("0.##");
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error loading data: " + ex.Message); }
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text) || string.IsNullOrWhiteSpace(_txtIdentity.Text))
            {
                MessageBox.Show("Name and Identity Number are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal.TryParse(_txtIncome.Text, out decimal income);

            try
            {
                using (var scope = Program.ServiceProvider.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (CustomerId.HasValue) // Update
                    {
                        var c = await uow.Customers.GetByIdAsync(CustomerId.Value);
                        if (c != null)
                        {
                            c.FullName = _txtName.Text.Trim();
                            c.IdentityNumber = _txtIdentity.Text.Trim();
                            c.PhoneNumber = _txtPhone.Text.Trim();
                            c.Email = _txtEmail.Text.Trim();
                            c.MonthlyIncome = income;
                            c.UpdatedDate = DateTime.Now;

                            uow.Customers.Update(c);
                        }
                    }
                    else // Insert
                    {
                        // Check duplicate Identity logic here...
                        var newC = new Customer
                        {
                            FullName = _txtName.Text.Trim(),
                            IdentityNumber = _txtIdentity.Text.Trim(),
                            PhoneNumber = _txtPhone.Text.Trim(),
                            Email = _txtEmail.Text.Trim(),
                            MonthlyIncome = income,
                            CreatedDate = DateTime.Now
                        };
                        await uow.Customers.AddAsync(newC);
                    }

                    await uow.CompleteAsync();
                    MessageBox.Show("Saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving: " + ex.Message);
            }
        }
    }
}