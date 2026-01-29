using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using CoreBanking.DAL.Data;
using CoreBanking.DAL.Repositories;
using CoreBanking.BLL.Interfaces;
using CoreBanking.BLL.Services;
using CoreBanking.WinUI.Forms;
using CoreBanking.WinUI.UI;    // Namespace chứa MainForm

namespace CoreBanking.WinUI
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        [STAThread]
        static void Main()
        {
            // 1. Khởi tạo cấu hình ứng dụng WinForms
            ApplicationConfiguration.Initialize();

            // 2. Cấu hình Dependency Injection (DI) Container
            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            // 3. Luồng chạy ứng dụng: Login -> Main
            try
            {
                // Lấy LoginForm từ DI
                // Kiểm tra xem LoginForm có tồn tại và được đăng ký hay không
                // Nếu chưa có file LoginForm.cs, đoạn này sẽ lỗi biên dịch
                // Tạm thời comment lại nếu chưa có LoginForm
                var loginForm = ServiceProvider.GetRequiredService<LoginForm>();

                // Hiển thị Login Form dưới dạng Dialog (Cửa sổ chặn)
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // Nếu Login thành công (DialogResult.OK), giải phóng Login Form
                    loginForm.Dispose();

                    // Lấy MainForm từ DI và chạy
                    var mainForm = ServiceProvider.GetRequiredService<MainForm>();
                    Application.Run(mainForm);
                }
                else
                {
                    // Người dùng tắt Login Form hoặc Login thất bại -> Thoát App
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi động ứng dụng: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {

                    // --- 1. DATABASE CONNECTION ---
                    // Lưu ý: Đổi Server=. nếu máy bạn dùng SQL Express hoặc (localdb)\\mssqllocaldb
                    string connectionString = "Server=.;Database=CoreBankingDb;Trusted_Connection=True;TrustServerCertificate=True;";
                    services.AddDbContext<CoreBankingDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    // --- 2. REPOSITORIES (DAL) ---
                    services.AddScoped<IUnitOfWork, UnitOfWork>();
                    // Đăng ký Generic Repository nếu cần dùng riêng lẻ, nhưng thường qua UnitOfWork là đủ
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

                    // --- 3. SERVICES (BLL) ---
                    services.AddScoped<IUserService, UserService>(); // Quan trọng cho Login
                    services.AddScoped<ICustomerService, CustomerService>();
                    services.AddScoped<IAccountService, AccountService>();
                    services.AddScoped<ILoanService, LoanService>();
                    services.AddScoped<ICreditScoreService, CreditScoreService>();
                    services.AddScoped<ITransactionService, TransactionService>();

                    // --- 4. FORMS (UI) ---
                    // Đăng ký Transient để mỗi lần gọi là tạo mới
                    // Đảm bảo LoginForm và MainForm đã được tạo trong project
                    services.AddTransient<LoginForm>();
                    services.AddTransient<MainForm>();
                });
        }
    }
}