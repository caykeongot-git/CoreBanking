using System;
using System.Windows.Forms;
using System.Drawing;
using CoreBanking.WinUI.Helpers;
using Microsoft.Extensions.DependencyInjection;

// ĐỔI NAMESPACE
namespace CoreBanking.WinUI.UI
{
    public partial class MainForm : Form
    {
        private Panel pnlSidebar;
        private Panel pnlContent;
        private IServiceProvider _serviceProvider;
        private Button currentBtn;

        public MainForm() { SetupUI(); }

        public MainForm(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Core Banking Enterprise";
            this.Size = new Size(1280, 800);
            this.WindowState = FormWindowState.Maximized;

            pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = ThemeHelper.PrimaryColor };
            this.Controls.Add(pnlSidebar);

            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = ThemeHelper.SecondaryColor };
            this.Controls.Add(pnlContent);

            // Menu Buttons
            AddMenuButton("Dashboard", OpenForm<LoanDashboard>);
            AddMenuButton("Logout", () => this.Close());

            OpenForm<LoanDashboard>();
        }

        private void AddMenuButton(string text, Action? action)
        {
            Button btn = new Button { Dock = DockStyle.Top, Height = 55, Text = text, FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            btn.Click += (s, e) => action?.Invoke();
            pnlSidebar.Controls.Add(btn);
            pnlSidebar.Controls.SetChildIndex(btn, 0);
        }

        private void OpenForm<T>() where T : Form
        {
            pnlContent.Controls.Clear();
            if (_serviceProvider == null) return;
            var form = _serviceProvider.GetRequiredService<T>();
            form.TopLevel = false;
            form.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(form);
            form.Show();
        }
    }
}