using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBanking.BLL.DTOs;
using CoreBanking.BLL.Interfaces; // FIX: Đã thêm using Interface
using CoreBanking.DAL.Entities;
using CoreBanking.DAL.Repositories;

namespace CoreBanking.BLL.Services
{
    // FIX: Phải kế thừa ILoanService thì mới AddScoped được
    public class LoanService : ILoanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LoanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Loan> RegisterLoanAsync(LoanDtos dto)
        {
            var loan = new Loan
            {
                CustomerId = dto.CustomerId,
                Amount = dto.Amount,
                InterestRate = dto.InterestRate,
                DurationMonth = dto.DurationMonth,
                StartDate = DateTime.Now,
                Status = LoanStatus.Pending // Đúng Enum
            };

            await _unitOfWork.Loans.AddAsync(loan);
            await _unitOfWork.CompleteAsync();
            return loan;
        }

        public async Task ApproveLoanAsync(int loanId)
        {
            var loan = await _unitOfWork.Loans.GetByIdAsync(loanId);
            if (loan == null) throw new Exception("Loan not found");

            if (loan.Status == LoanStatus.Pending)
            {
                loan.Status = LoanStatus.Active; // FIX: Đã đổi từ Approved -> Active

                // Giải ngân
                var account = new Account
                {
                    CustomerId = loan.CustomerId,
                    Balance = loan.Amount,
                    CreatedDate = DateTime.Now
                };
                await _unitOfWork.Accounts.AddAsync(account);

                await _unitOfWork.CompleteAsync();
            }
        }

        public List<RepaymentScheduleDto> CalculateAmortization(decimal amount, decimal interestRate, int months)
        {
            var schedule = new List<RepaymentScheduleDto>();
            decimal balance = amount;

            double r = (double)(interestRate / 100 / 12);
            double n = (double)months;

            // Fix lỗi ép kiểu double/decimal
            double pmtDouble = (double)amount * r * Math.Pow(1 + r, n) / (Math.Pow(1 + r, n) - 1);
            decimal monthlyPayment = (decimal)pmtDouble;

            for (int i = 1; i <= months; i++)
            {
                decimal interest = balance * (decimal)r;
                decimal principal = monthlyPayment - interest;
                balance -= principal;

                if (balance < 0) balance = 0;

                schedule.Add(new RepaymentScheduleDto
                {
                    Month = i,
                    Principal = Math.Round(principal, 2),
                    Interest = Math.Round(interest, 2),
                    Total = Math.Round(monthlyPayment, 2)
                });
            }

            return schedule;
        }

        public async Task RepayLoanAsync(int loanId, decimal amount)
        {
            var loan = await _unitOfWork.Loans.GetByIdAsync(loanId);
            if (loan == null) return;

            if (amount >= loan.Amount)
            {
                loan.Status = LoanStatus.Paid; // FIX: Đã đổi từ PaidOff -> Paid
                loan.EndDate = DateTime.Now;
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}