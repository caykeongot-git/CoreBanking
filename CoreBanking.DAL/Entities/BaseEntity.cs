using System.ComponentModel.DataAnnotations;

namespace CoreBanking.DAL.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}