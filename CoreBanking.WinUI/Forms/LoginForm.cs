using System;
using System.Windows.Forms;
using System.Drawing;
using CoreBanking.WinUI.Helpers;
using Microsoft.Extensions.DependencyInjection;

// ĐỔI NAMESPACE ĐỂ TRÁNH XUNG ĐỘT
namespace CoreBanking.WinUI.UI
{
    public partial class LoginForm : Form
    {
        private TextBox txtUser;
        private TextBox txtPass;
        private Button btnLogin;
        private IServiceProvider _serviceProvider;

        public LoginForm() { SetupUI(); }

        public LoginForm(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            SetupUI();
        }

        private void SetupUI()
        {
            // (Giữ nguyên code UI cũ của bạn, không cần copy lại hết nếu đã có)
            // ... Code SetupUI ...
            // Nếu bạn lỡ xóa thì báo tôi gửi lại, còn không thì chỉ cần sửa dòng namespace ở trên cùng thôi.

            // Code rút gọn để bạn dễ paste đè lên nếu cần:
            this.Text = "Secure Login";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            // ... (Phần còn lại giữ nguyên) ...

            // Code demo nhanh để chạy được:
            Panel pnlLeft = new Panel { Dock = DockStyle.Left, Width = 400, BackColor = ThemeHelper.PrimaryColor };
            this.Controls.Add(pnlLeft);

            Panel pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            txtUser = new TextBox { Location = new Point(50, 160), Size = new Size(300, 30) };
            txtPass = new TextBox { Location = new Point(50, 220), Size = new Size(300, 30), PasswordChar = '•' };
            btnLogin = new Button { Text = "LOGIN", Location = new Point(50, 300), Size = new Size(300, 45), BackColor = ThemeHelper.AccentColor };
            btnLogin.Click += BtnLogin_Click;

            pnlRight.Controls.Add(txtUser);
            pnlRight.Controls.Add(txtPass);
            pnlRight.Controls.Add(btnLogin);
            this.Controls.Add(pnlRight);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (txtUser.Text == "admin" && txtPass.Text == "admin")
            {
                this.Hide();
                if (_serviceProvider == null) return;
                var mainForm = _serviceProvider.GetRequiredService<MainForm>();
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("User: admin / Pass: admin");
            }
        }
    }
}