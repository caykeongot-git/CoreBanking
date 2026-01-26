using CoreBanking.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CoreBanking.DAL.Data
{
    public class CoreBankingDbContext : DbContext
    {
        public CoreBankingDbContext(DbContextOptions<CoreBankingDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<CreditScore> CreditScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Config Relationships
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.CreditScore)
                .WithOne(cs => cs.Customer)
                .HasForeignKey<CreditScore>(cs => cs.CustomerId);

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.AccountNumber)
                .IsUnique();

            // RowVersion is automatically handled by [Timestamp] attribute in Entity
            // but can be explicit here:
            // modelBuilder.Entity<Account>()
            //    .Property(a => a.RowVersion).IsRowVersion();
        }
    }
}