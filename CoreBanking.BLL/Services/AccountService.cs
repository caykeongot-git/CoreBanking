using CoreBanking.BLL.DTOs;
using CoreBanking.BLL.Interfaces;
using CoreBanking.DAL.Entities;
using CoreBanking.DAL.Repositories;

namespace CoreBanking.BLL.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AccountDto> CreateAccountAsync(CreateAccountDto request)
        {
            // Tạo số tài khoản ngẫu nhiên 10 số
            var random = new Random();
            string accNumber = "1900" + random.Next(100000, 999999).ToString();

            var account = new Account
            {
                CustomerId = request.CustomerId,
                AccountNumber = accNumber,
                Balance = request.InitialDeposit
            };

            await _unitOfWork.Accounts.AddAsync(account);

            // Nếu có tiền nạp ban đầu, tạo transaction log
            if (request.InitialDeposit > 0)
            {
                await _unitOfWork.Transactions.AddAsync(new Transaction
                {
                    Account = account,
                    Amount = request.InitialDeposit,
                    Type = TransactionType.Deposit,
                    Description = "Initial Deposit",
                    TransactionDate = DateTime.UtcNow
                });
            }

            await _unitOfWork.CompleteAsync();

            return new AccountDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                CustomerName = "N/A" // Cần query thêm nếu muốn hiển thị
            };
        }

        public async Task DepositAsync(DepositWithdrawRequestDto request)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId);
            if (account == null) throw new Exception("Account not found");

            if (request.Amount <= 0) throw new Exception("Amount must be positive");

            // Logic: Cộng tiền
            account.Balance += request.Amount;
            _unitOfWork.Accounts.Update(account);

            // Logic: Ghi log
            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Amount = request.Amount,
                Type = TransactionType.Deposit,
                Description = request.Note ?? "Deposit",
                TransactionDate = DateTime.UtcNow
            });

            await _unitOfWork.CompleteAsync();
        }

        public async Task WithdrawAsync(DepositWithdrawRequestDto request)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId);
            if (account == null) throw new Exception("Account not found");

            if (account.Balance < request.Amount) throw new Exception("Insufficient balance");

            // Logic: Trừ tiền
            account.Balance -= request.Amount;
            _unitOfWork.Accounts.Update(account);

            // Logic: Ghi log
            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Amount = request.Amount,
                Type = TransactionType.Withdrawal,
                Description = request.Note ?? "Withdrawal",
                TransactionDate = DateTime.UtcNow
            });

            await _unitOfWork.CompleteAsync();
        }

        public async Task TransferAsync(TransferRequestDto request)
        {
            // 1. Validate
            if (request.FromAccountId == request.ToAccountId)
                throw new Exception("Cannot transfer to self"); // Kiểm tra không được chuyển cho chính mình

            var fromAcc = await _unitOfWork.Accounts.GetByIdAsync(request.FromAccountId);
            var toAcc = await _unitOfWork.Accounts.GetByIdAsync(request.ToAccountId);

            if (fromAcc == null || toAcc == null) throw new Exception("Invalid account(s)"); // Có tồn tại hay không?
            if (fromAcc.Balance < request.Amount) throw new Exception("Insufficient balance"); // Có đủ tiền hay không?

            // 2. Trừ tiền người gửi
            fromAcc.Balance -= request.Amount;
            _unitOfWork.Accounts.Update(fromAcc); // Đánh dấu là "thằng này bị sửa tiền rồi nhé"

            // 3. Cộng tiền người nhận
            toAcc.Balance += request.Amount;
            _unitOfWork.Accounts.Update(toAcc); // Tương tự như trên

            // 4. Ghi log Transaction (2 bản ghi)
            var now = DateTime.UtcNow;

            // Log cho người gửi
            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                AccountId = fromAcc.Id,
                Amount = request.Amount,
                Type = TransactionType.TransferOut,
                Description = $"Transfer to {toAcc.AccountNumber}: {request.Note}",
                TransactionDate = now
            });

            // Log cho người nhận
            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                AccountId = toAcc.Id,
                Amount = request.Amount,
                Type = TransactionType.TransferIn,
                Description = $"Received from {fromAcc.AccountNumber}: {request.Note}",
                TransactionDate = now
            });

            // 5. Commit Transaction (UnitOfWork đảm bảo ACID)
            // Nếu 1 trong các bước trên lỗi, DB sẽ không lưu gì cả
            await _unitOfWork.CompleteAsync();
        }

        public async Task<AccountDto?> GetAccountByIdAsync(int id)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(id);
            if (account == null) return null;
            return new AccountDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance
            };
        }
    }
}
