using CoreBanking.BLL.DTOs;
using CoreBanking.DAL.Entities;
using CoreBanking.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.BLL.Services
{
    public interface ITransactionService
    {
        Task TransferMoneyWithConcurrencyCheckAsync(TransferRequestDto request);
    }

    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task TransferMoneyWithConcurrencyCheckAsync(TransferRequestDto request)
        {
            // Retry policy đơn giản: Thử tối đa 3 lần nếu gặp tranh chấp
            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await ExecuteTransferAsync(request);
                    return; // Thành công thì thoát
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (i == maxRetries - 1) throw; // Hết lượt retry thì ném lỗi ra ngoài

                    // Nếu gặp lỗi, clear ChangeTracker để load lại dữ liệu mới nhất từ DB
                    // Lưu ý: Trong thực tế cần reload lại entity
                    await Task.Delay(100); // Backoff nhẹ
                }
            }
        }

        private async Task ExecuteTransferAsync(TransferRequestDto request)
        {
            var fromAcc = await _unitOfWork.Accounts.GetByIdAsync(request.FromAccountId);
            var toAcc = await _unitOfWork.Accounts.GetByIdAsync(request.ToAccountId);

            if (fromAcc == null || toAcc == null) throw new Exception("Account not found");
            if (fromAcc.Balance < request.Amount) throw new Exception("Insufficient balance");

            // Trừ tiền
            fromAcc.Balance -= request.Amount;

            // Cộng tiền
            toAcc.Balance += request.Amount;

            // Ghi log
            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                AccountId = fromAcc.Id,
                Amount = request.Amount,
                Type = TransactionType.TransferOut,
                Description = $"Transfer to {toAcc.AccountNumber}",
                TransactionDate = DateTime.UtcNow
            });

            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                AccountId = toAcc.Id,
                Amount = request.Amount,
                Type = TransactionType.TransferIn,
                Description = $"Received from {fromAcc.AccountNumber}",
                TransactionDate = DateTime.UtcNow
            });

            // EF Core sẽ tự kiểm tra RowVersion tại đây
            await _unitOfWork.CompleteAsync();
        }
    }
}