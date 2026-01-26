using System.ComponentModel.DataAnnotations.Schema;

namespace CoreBanking.DAL.Entities
{
    public enum LoanStatus { Pending, Approved, Rejected, Paid, Overdue }

    public class Loan : BaseEntity
    {
        public int CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrincipalAmount { get; set; }

        public double InterestRate { get; set; } // % per year
        public int TermMonths { get; set; }

        public LoanStatus Status { get; set; } = LoanStatus.Pending;
    }
}