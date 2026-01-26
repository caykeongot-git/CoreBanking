using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace CoreBanking.DAL.Entities
{
    public class Customer : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string IdentityNumber { get; set; } = string.Empty; // CCCD/CMND

        [EmailAddress]
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public bool IsKycVerified { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public virtual CreditScore? CreditScore { get; set; }
    }
}
