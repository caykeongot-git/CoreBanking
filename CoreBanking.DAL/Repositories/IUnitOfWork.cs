using CoreBanking.DAL.Entities;

namespace CoreBanking.DAL.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Customer> Customers { get; }
        IRepository<Account> Accounts { get; }
        IRepository<Transaction> Transactions { get; }
        IRepository<Loan> Loans { get; }
        IRepository<CreditScore> CreditScores { get; }

        Task<int> CompleteAsync();
    }
}
