using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreBanking.DAL.Entities
{
    public enum LoanStatus
    {
        Pending = 0,    // Chờ duyệt
        Active = 1,     // Đang vay
        Paid = 2,       // Đã trả hết (FIX: Thêm trạng thái này)
        BadDebt = 3,    // Nợ xấu
        Rejected = 4    // Từ chối
    }

    public class Loan : BaseEntity
    {
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        // FIX: Thêm Amount để khớp với code Control
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRate { get; set; } // Lãi suất %

        public int DurationMonth { get; set; } // Thời hạn vay (tháng)

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public LoanStatus Status { get; set; } = LoanStatus.Pending;
    }
}