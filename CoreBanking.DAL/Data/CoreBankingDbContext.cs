using CoreBanking.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.DAL.Data
{
    public class CoreBankingDbContext : DbContext
    {
        public CoreBankingDbContext(DbContextOptions<CoreBankingDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Loan> Loans { get; set; }
        // public DbSet<RepaymentSchedule> RepaymentSchedules { get; set; } // (Nếu bạn đã có)
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CreditScore> CreditScores { get; set; }

        // --- MỚI THÊM ---
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique Constraints
            modelBuilder.Entity<Customer>().HasIndex(c => c.IdentityNumber).IsUnique();
            modelBuilder.Entity<Account>().HasIndex(a => a.AccountNumber).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique(); // Username không trùng

            // SEED DATA: Tạo sẵn 1 Admin để đăng nhập
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "admin", // Trong thực tế phải Hash
                    FullName = "System Administrator",
                    Role = UserRole.Admin,
                    CreatedDate = new DateTime(2026, 1, 1)
                }
            );

            // Relationships
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Customer)
                .WithMany(c => c.Loans)
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}