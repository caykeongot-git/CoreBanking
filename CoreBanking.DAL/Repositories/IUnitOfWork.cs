using CoreBanking.DAL.Entities;

namespace CoreBanking.DAL.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Customer> Customers { get; }
        IRepository<Account> Accounts { get; }
        IRepository<Loan> Loans { get; }
        IRepository<Transaction> Transactions { get; }
        IRepository<CreditScore> CreditScores { get; }

        // --- QUAN TRỌNG: Thêm dòng này để Login hoạt động ---
        IRepository<User> Users { get; }

        Task<int> CompleteAsync();
    }
}