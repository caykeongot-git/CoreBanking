using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreBanking.DAL.Entities
{
    public enum TransactionType { Deposit, Withdrawal, TransferIn, TransferOut }

    public class Transaction : BaseEntity
    {
        public int AccountId { get; set; }
        public virtual Account? Account { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }
        public string? Description { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
}
