using System.ComponentModel.DataAnnotations;

namespace CoreBanking.DAL.Entities
{
    public class CreditScore : BaseEntity
    {
        public int CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        [Range(0, 850)]
        public int Score { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}