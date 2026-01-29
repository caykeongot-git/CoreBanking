using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBanking.BLL.DTOs;
using CoreBanking.DAL.Entities;

namespace CoreBanking.BLL.Interfaces // Chú ý Namespace này
{
    public interface ILoanService
    {
        Task<Loan> RegisterLoanAsync(LoanDto dto);
        Task ApproveLoanAsync(int loanId);
        Task RepayLoanAsync(int loanId, decimal amount);
        List<RepaymentScheduleDto> CalculateAmortization(decimal amount, decimal interestRate, int months);
    }
}