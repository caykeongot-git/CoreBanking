namespace CoreBanking.BLL.DTOs
{
    public class CreditScoreResultDto
    {
        public int CustomerId { get; set; }
        public int Score { get; set; } // 0 - 850
        public string Rating { get; set; } = string.Empty; // Excellent, Good, Poor...
        public decimal MaxLoanLimit { get; set; } // Hạn mức được vay tối đa
    }
}
