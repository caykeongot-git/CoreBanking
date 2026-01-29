using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreBanking.DAL.Entities
{
    public enum UserRole
    {
        Admin,
        Manager,
        Teller
    }

    // --- QUAN TRỌNG: Phải có ": BaseEntity" ---
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public UserRole Role { get; set; } = UserRole.Teller;
    }
}