using CoreBanking.BLL.DTOs;
using CoreBanking.DAL.Entities;
using CoreBanking.DAL.Repositories;

namespace CoreBanking.BLL.Services
{
    public interface ILoanService
    {
        Task<LoanDetailDto> RegisterLoanAsync(LoanApplicationDto request);
        Task<bool> DisburseLoanAsync(int loanId); // Giải ngân
        List<AmortizationScheduleDto> CalculateAmortizationSchedule(decimal principal, double rate, int months);
    }

    public class LoanService : ILoanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private const double DEFAULT_INTEREST_RATE = 12.0; // 12% / năm

        public LoanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<LoanDetailDto> RegisterLoanAsync(LoanApplicationDto request)
        {
            // 1. Tạo khoản vay (Status = Pending)
            var loan = new Loan
            {
                CustomerId = request.CustomerId,
                PrincipalAmount = request.PrincipalAmount,
                TermMonths = request.TermMonths,
                InterestRate = DEFAULT_INTEREST_RATE,
                Status = LoanStatus.Pending
            };

            await _unitOfWork.Loans.AddAsync(loan);
            await _unitOfWork.CompleteAsync();

            // 2. Tính lịch trả nợ dự kiến
            var schedule = CalculateAmortizationSchedule(loan.PrincipalAmount, loan.InterestRate, loan.TermMonths);

            return new LoanDetailDto
            {
                LoanId = loan.Id,
                CustomerId = loan.CustomerId,
                PrincipalAmount = loan.PrincipalAmount,
                TermMonths = loan.TermMonths,
                InterestRate = loan.InterestRate,
                Status = loan.Status.ToString(),
                Schedule = schedule
            };
        }

        public async Task<bool> DisburseLoanAsync(int loanId)
        {
            var loan = await _unitOfWork.Loans.GetByIdAsync(loanId);
            if (loan == null || loan.Status != LoanStatus.Pending) return false;

            // Tìm tài khoản chính của khách để bơm tiền
            var account = (await _unitOfWork.Accounts.FindAsync(a => a.CustomerId == loan.CustomerId)).FirstOrDefault();
            if (account == null) throw new Exception("Customer has no account to receive funds");

            // LOGIC GIẢI NGÂN (Transaction)
            loan.Status = LoanStatus.Approved;
            _unitOfWork.Loans.Update(loan);

            account.Balance += loan.PrincipalAmount;
            _unitOfWork.Accounts.Update(account);

            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Amount = loan.PrincipalAmount,
                Type = TransactionType.Deposit,
                Description = $"Loan Disbursement #{loan.Id}",
                TransactionDate = DateTime.UtcNow
            });

            await _unitOfWork.CompleteAsync();
            return true;
        }

        // THUẬT TOÁN: Dư nợ giảm dần (Trả gốc đều + Lãi theo dư nợ thực tế)
        public List<AmortizationScheduleDto> CalculateAmortizationSchedule(decimal principal, double rate, int months)
        {
            var schedule = new List<AmortizationScheduleDto>();
            decimal balance = principal;
            decimal monthlyPrincipal = principal / months; // Gốc trả đều hàng tháng
            double monthlyRate = rate / 100 / 12;

            for (int i = 1; i <= months; i++)
            {
                decimal interest = balance * (decimal)monthlyRate;
                decimal totalPayment = monthlyPrincipal + interest;
                balance -= monthlyPrincipal;

                // Fix số lẻ cuối cùng
                if (balance < 0) balance = 0;

                schedule.Add(new AmortizationScheduleDto
                {
                    Period = i,
                    PrincipalPayment = Math.Round(monthlyPrincipal, 2),
                    InterestPayment = Math.Round(interest, 2),
                    TotalPayment = Math.Round(totalPayment, 2),
                    RemainingBalance = Math.Round(balance, 2)
                });
            }
            return schedule;
        }
    }
}
