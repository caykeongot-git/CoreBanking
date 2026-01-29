using CoreBanking.BLL.Interfaces;
using CoreBanking.BLL.Services;
using CoreBanking.DAL.Data;
using CoreBanking.DAL.Repositories;
using CoreBanking.WinUI;
using CoreBanking.WinUI.Forms;
using CoreBanking.WinUI.UI; // Namespace chứa LoginForm/MainForm
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoreBanking.WinUI
{
    static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Xây dựng Host
            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            try
            {
                // Mở Login Form trước
                var loginForm = ServiceProvider.GetRequiredService<LoginForm>();

                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // Login OK -> Mở Main Form
                    var mainForm = ServiceProvider.GetRequiredService<MainForm>();
                    Application.Run(mainForm);
                }
                else
                {
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Start Error: {ex.Message}");
            }
        }

        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {

                    // 1. DB (Sửa connection string nếu cần)
                    services.AddDbContext<CoreBankingDbContext>(options =>
                        options.UseSqlServer("Server=.;Database=CoreBankingDb;Trusted_Connection=True;TrustServerCertificate=True;"));

                    // 2. Repositories
                    services.AddScoped<IUnitOfWork, UnitOfWork>();

                    // 3. Services
                    services.AddScoped<IUserService, UserService>();
                    services.AddScoped<ICustomerService, CustomerService>();
                    services.AddScoped<IAccountService, AccountService>();
                    services.AddScoped<ILoanService, LoanService>();
                    services.AddScoped<ICreditScoreService, CreditScoreService>();
                    services.AddScoped<ITransactionService, TransactionService>();
                    services.AddTransient<CoreBanking.WinUI.Forms.LoanDetailForm>();
                    services.AddTransient<CoreBanking.WinUI.Forms.CustomerDetailForm>();
                    // 4. Forms
                    services.AddTransient<LoginForm>();
                    services.AddTransient<MainForm>();
                });
        }
    }
}