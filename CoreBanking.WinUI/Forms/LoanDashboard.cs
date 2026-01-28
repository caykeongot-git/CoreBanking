using System;
using System.Windows.Forms;
using System.Drawing;
using CoreBanking.WinUI.Helpers;
using CoreBanking.WinUI.Controls;
using CoreBanking.DAL.Repositories;

// ĐỔI NAMESPACE
namespace CoreBanking.WinUI.UI
{
    public partial class LoanDashboard : Form
    {
        private readonly IUnitOfWork _unitOfWork;
        private DataGridView dgvLoans;

        public LoanDashboard() { SetupUI(); }

        public LoanDashboard(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.BackColor = ThemeHelper.SecondaryColor;

            dgvLoans = new DataGridView { Dock = DockStyle.Fill };
            GridHelper.StyleGrid(dgvLoans);
            this.Controls.Add(dgvLoans);
        }

        private async void LoadData()
        {
            if (_unitOfWork == null) return;
            var loans = await _unitOfWork.Loans.GetAllAsync();
            dgvLoans.DataSource = loans.ToList();
        }
    }
}