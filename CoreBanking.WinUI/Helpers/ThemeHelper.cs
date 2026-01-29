using System.Drawing;

namespace CoreBanking.WinUI.Helpers
{
    public static class ThemeHelper
    {
        // Màu chủ đạo (Banking Blue)
        public static Color PrimaryColor = Color.FromArgb(13, 71, 161); // Xanh đậm
        public static Color DarkPrimary = Color.FromArgb(0, 33, 113);   // Xanh rất đậm (cho Hover)

        // Màu nền
        public static Color BackgroundColor = Color.FromArgb(240, 242, 245); // Xám nhạt hiện đại
        public static Color White = Color.White;

        // Màu nhấn (Accent)
        public static Color AccentColor = Color.FromArgb(255, 111, 0); // Cam đậm (Tạo tương phản)

        // Màu text
        public static Color TextDark = Color.FromArgb(33, 33, 33);
        public static Color TextGray = Color.FromArgb(117, 117, 117);

        // Fonts (Segoe UI chuẩn Windows)
        public static Font HeaderFont = new Font("Segoe UI", 18, FontStyle.Bold);
        public static Font SubHeaderFont = new Font("Segoe UI", 12, FontStyle.Bold);
        public static Font NormalFont = new Font("Segoe UI", 10, FontStyle.Regular);
    }
}