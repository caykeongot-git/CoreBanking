using CoreBanking.BLL.DTOs;

namespace CoreBanking.BLL.Interfaces
{
    public interface IAccountService
    {
        Task<AccountDto> CreateAccountAsync(CreateAccountDto request);
        Task<AccountDto?> GetAccountByIdAsync(int id);

        // Core Banking Operations
        Task DepositAsync(DepositWithdrawRequestDto request);
        Task WithdrawAsync(DepositWithdrawRequestDto request);
        Task TransferAsync(TransferRequestDto request);
    }
}