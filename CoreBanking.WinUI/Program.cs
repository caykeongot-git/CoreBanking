using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using CoreBanking.DAL.Data;
using CoreBanking.DAL.Repositories;
using CoreBanking.BLL.Services;
using CoreBanking.BLL.Interfaces;
using System;
using System.Windows.Forms;

// QUAN TRỌNG: Dùng namespace UI thay vì Forms
//using CoreBanking.WinUI.UI;

namespace CoreBanking.WinUI
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        [STAThread]
        static void Main()
        {
            //ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Chạy Form Login
            //var loginForm = ServiceProvider.GetRequiredService<LoginForm>();
            //Application.Run(loginForm);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            string connectionString = "Server=.;Database=CoreBankingDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
            services.AddDbContext<CoreBankingDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICreditScoreService, CreditScoreService>();
            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped<ITransactionService, TransactionService>();

            // Đăng ký các Form với Namespace mới
            //services.AddTransient<LoginForm>();
            services.AddTransient<MainForm>();
            //services.AddTransient<LoanDashboard>();
        }
    }
}