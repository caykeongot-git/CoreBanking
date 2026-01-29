using System.ComponentModel.DataAnnotations.Schema;

namespace CoreBanking.DAL.Entities
{
    public enum LoanStatus
    {
        Pending,
        Approved,
        Rejected,
        Active,   // Trạng thái đang vay
        PaidOff,  // Trạng thái đã trả hết
        BadDebt   // Trạng thái nợ xấu
    }

    public class Loan : BaseEntity
    {
        public int CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrincipalAmount { get; set; }

        public double InterestRate { get; set; }
        public int TermMonths { get; set; }

        public LoanStatus Status { get; set; } = LoanStatus.Pending;

        public virtual ICollection<RepaymentSchedule> Schedules { get; set; } = new List<RepaymentSchedule>();
    }

    // Đảm bảo class này tồn tại vì Loan có tham chiếu đến nó
    public class RepaymentSchedule : BaseEntity
    {
        public int LoanId { get; set; }
        public virtual Loan? Loan { get; set; }
        public int Month { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Principal { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Interest { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; }
    }
}