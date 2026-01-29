using CoreBanking.DAL.Data;
using CoreBanking.DAL.Entities;

namespace CoreBanking.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CoreBankingDbContext _context;

        public UnitOfWork(CoreBankingDbContext context)
        {
            _context = context;
            Customers = new Repository<Customer>(_context);
            Accounts = new Repository<Account>(_context);
            Transactions = new Repository<Transaction>(_context);
            Loans = new Repository<Loan>(_context);
            CreditScores = new Repository<CreditScore>(_context);
            Users = new Repository<User>(_context);
        }

        public IRepository<Customer> Customers { get; private set; }
        public IRepository<Account> Accounts { get; private set; }
        public IRepository<Transaction> Transactions { get; private set; }
        public IRepository<Loan> Loans { get; private set; }
        public IRepository<CreditScore> CreditScores { get; private set; }
        public IRepository<User> Users { get; private set; }
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}