using System.Drawing;

namespace CoreBanking.WinUI.Helpers
{
    public static class ThemeHelper
    {
        // Banking Color Palette
        public static Color PrimaryColor = Color.FromArgb(0, 51, 102); // Navy Blue (Ngân hàng)
        public static Color DarkPrimary = Color.FromArgb(0, 35, 75);    // Darker Navy
        public static Color SecondaryColor = Color.FromArgb(240, 242, 245); // Light Gray BG
        public static Color AccentColor = Color.FromArgb(0, 123, 255); // Blue Highlight

        public static Color TextDark = Color.FromArgb(33, 37, 41);
        public static Color TextLight = Color.White;
        public static Color TextGray = Color.Gray;

        // Fonts
        public static Font HeaderFont = new Font("Segoe UI", 16, FontStyle.Bold);
        public static Font SubHeaderFont = new Font("Segoe UI", 12, FontStyle.Bold);
        public static Font NormalFont = new Font("Segoe UI", 10, FontStyle.Regular);
        public static Font BoldFont = new Font("Segoe UI", 10, FontStyle.Bold);
    }
}