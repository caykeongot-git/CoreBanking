namespace CoreBanking.BLL.DTOs
{
    public class LoanApplicationDto
    {
        public int CustomerId { get; set; }
        public decimal PrincipalAmount { get; set; }
        public int TermMonths { get; set; }
    }

    public class AmortizationScheduleDto
    {
        public int Period { get; set; } // Kỳ trả nợ (tháng thứ mấy)
        public decimal PrincipalPayment { get; set; } // Tiền gốc phải trả
        public decimal InterestPayment { get; set; } // Tiền lãi phải trả
        public decimal TotalPayment { get; set; } // Tổng gốc + lãi
        public decimal RemainingBalance { get; set; } // Dư nợ còn lại
    }

    public class LoanDetailDto : LoanApplicationDto
    {
        public int LoanId { get; set; }
        public double InterestRate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<AmortizationScheduleDto> Schedule { get; set; } = new();
    }
}