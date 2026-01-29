using CoreBanking.DAL.Entities;

namespace CoreBanking.WinUI
{
    // Lưu trữ thông tin người dùng đăng nhập toàn cục
    public static class Session
    {
        public static User? CurrentUser { get; set; }

        public static bool IsLoggedIn => CurrentUser != null;

        // Helper check quyền
        public static bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        public static bool IsManager => CurrentUser?.Role == UserRole.Manager;
        public static bool IsTeller => CurrentUser?.Role == UserRole.Teller;
    }
}