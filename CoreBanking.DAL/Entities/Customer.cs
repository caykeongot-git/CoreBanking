using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreBanking.DAL.Entities
{
    public class Account : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        public int CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        // Navigation
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        // OPTIMISTIC CONCURRENCY CONTROL
        // EF Core sẽ dùng field này để detect conflict khi update Balance
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
