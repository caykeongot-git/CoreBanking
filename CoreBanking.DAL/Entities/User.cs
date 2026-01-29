using System.ComponentModel.DataAnnotations;

namespace CoreBanking.DAL.Entities
{
    public enum UserRole
    {
        Admin,
        Manager,
        Teller // Giao dịch viên
    }

    public class User : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty; // Lưu mật khẩu (nên mã hóa MD5/BCrypt)

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public UserRole Role { get; set; } = UserRole.Teller;
    }
}