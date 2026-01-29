using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreBanking.DAL.Entities
{
    public class Customer : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string IdentityNumber { get; set; } = string.Empty; // CMND/CCCD

        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        // --- QUAN TRỌNG: Thuộc tính này đang bị thiếu hoặc sai tên ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyIncome { get; set; }

        public string KycInfo { get; set; } = string.Empty; // JSON hoặc Path ảnh KYC

        // Thêm thuộc tính IsKycVerified để sửa lỗi
        public bool IsKycVerified { get; set; } = false;

        // Navigation
        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public virtual CreditScore? CreditScore { get; set; }
    }
}