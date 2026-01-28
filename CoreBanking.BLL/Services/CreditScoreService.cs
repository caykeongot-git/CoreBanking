using CoreBanking.BLL.DTOs;
using CoreBanking.DAL.Repositories;
using CoreBanking.DAL.Entities;

namespace CoreBanking.BLL.Services
{
    public interface ICreditScoreService
    {
        Task<CreditScoreResultDto> CalculateScoreAsync(int customerId);
    }

    public class CreditScoreService : ICreditScoreService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreditScoreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CreditScoreResultDto> CalculateScoreAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null) throw new Exception("Customer not found");

            // Lấy tất cả tài khoản để tính tổng tài sản
            var accounts = await _unitOfWork.Accounts.FindAsync(a => a.CustomerId == customerId);
            decimal totalBalance = accounts.Sum(a => a.Balance);

            // --- THUẬT TOÁN CHẤM ĐIỂM (Ma trận trọng số) ---
            int baseScore = 300; // Điểm sàn

            // 1. Trọng số tài sản (Max 300 điểm)
            int assetScore = (int)(totalBalance / 1000000) * 10;
            if (assetScore > 300) assetScore = 300;

            // 2. Trọng số KYC (100 điểm)
            int kycScore = customer.IsKycVerified ? 100 : 0;

            // 3. Trọng số thâm niên (Giả lập: ID nhỏ là khách cũ -> 50 điểm)
            int loyaltyScore = customer.Id < 100 ? 50 : 10;

            int finalScore = baseScore + assetScore + kycScore + loyaltyScore;
            if (finalScore > 850) finalScore = 850;

            // Đánh giá xếp hạng
            string rating = finalScore switch
            {
                >= 750 => "Excellent",
                >= 650 => "Good",
                >= 550 => "Fair",
                _ => "Poor"
            };

            // Tính hạn mức vay (Ví dụ: 10 lần tổng tài sản nếu điểm tốt)
            decimal limit = rating == "Excellent" ? totalBalance * 10 : totalBalance * 2;

            // Lưu hoặc cập nhật vào DB
            var existingScore = (await _unitOfWork.CreditScores.FindAsync(c => c.CustomerId == customerId)).FirstOrDefault();
            if (existingScore == null)
            {
                await _unitOfWork.CreditScores.AddAsync(new CreditScore { CustomerId = customerId, Score = finalScore });
            }
            else
            {
                existingScore.Score = finalScore;
                existingScore.LastUpdated = DateTime.UtcNow;
                _unitOfWork.CreditScores.Update(existingScore);
            }
            await _unitOfWork.CompleteAsync();

            return new CreditScoreResultDto
            {
                CustomerId = customerId,
                Score = finalScore,
                Rating = rating,
                MaxLoanLimit = limit
            };
        }
    }
}
