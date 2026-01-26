namespace CoreBanking.BLL.DTOs
{
    // DTO dùng để tạo mới khách hàng
    public class CreateCustomerDto
    {
        public string FullName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    // DTO dùng để hiển thị thông tin
    public class CustomerDto : CreateCustomerDto
    {
        public int Id { get; set; }
        public bool IsKycVerified { get; set; }
        public decimal TotalBalance { get; set; } // Tổng tài sản
    }
}
