using System.Drawing.Drawing2D;

namespace CoreBanking.WinUI.Helpers
{
    public static class ThemeHelper
    {
        public static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float curveSize = radius * 2F;
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public static class ThemeColor
    {
        // Primary Colors
        public static Color Primary = Color.FromArgb(0, 122, 204);    // Blue
        public static Color Secondary = Color.FromArgb(108, 117, 125); // Gray

        // Semantic Colors
        public static Color Success = Color.FromArgb(40, 167, 69);    // Green
        public static Color Danger = Color.FromArgb(220, 53, 69);     // Red
        public static Color Warning = Color.FromArgb(255, 193, 7);    // Yellow/Orange
        public static Color Info = Color.FromArgb(23, 162, 184);      // Cyan

        // Backgrounds
        public static Color Background = Color.FromArgb(245, 247, 251); // Light Gray
        public static Color White = Color.White;
    }
}