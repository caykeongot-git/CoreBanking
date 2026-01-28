using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreBanking.DAL.Entities
{
    public class Account : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; } = string.Empty; // Số tài khoản (1900123456)

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; } // Số dư (Tiền Tươi)

        public int CustomerId { get; set; } // Của thằng khách nào đó
        public virtual Customer? Customer { get; set; } // Link tới bảng Customer

        // Navigation
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>(); // Lịch sử giao dịch

        // OPTIMISTIC CONCURRENCY CONTROL
        // EF Core sẽ dùng field này để detect conflict khi update Balance
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>(); // Chống ghi đè
    }
}
