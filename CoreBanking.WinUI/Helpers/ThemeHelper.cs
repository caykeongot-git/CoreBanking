using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoreBanking.WinUI.Helpers
{
    public static class ThemeHelper
    {
        // --- BẢNG MÀU CHÍNH ---
        public static Color PrimaryColor = Color.FromArgb(0, 122, 204);    // Xanh dương
        public static Color SecondaryColor = Color.FromArgb(108, 117, 125);  // Xám
        public static Color SuccessColor = Color.FromArgb(40, 167, 69);    // Xanh lá
        public static Color WarningColor = Color.FromArgb(255, 193, 7);    // Vàng
        public static Color DangerColor = Color.FromArgb(220, 53, 69);     // Đỏ

        public static Color Background = Color.FromArgb(245, 247, 251);    // Nền App
        public static Color White = Color.White;
        public static Color TextMain = Color.FromArgb(64, 64, 64);
        public static Color TextDark = Color.FromArgb(30, 30, 45);

        // --- GRAPHICS ---
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

        // --- BUTTON STYLES ---
        public static void ApplyPrimaryButtonStyle(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = PrimaryColor;
            btn.ForeColor = White;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplySecondaryButtonStyle(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.WhiteSmoke;
            btn.ForeColor = TextMain;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            btn.Cursor = Cursors.Hand;
        }
    }

    // FIX: Class này map các tên gọi cũ sang ThemeHelper mới
    public static class ThemeColor
    {
        public static Color Primary => ThemeHelper.PrimaryColor;
        public static Color Secondary => ThemeHelper.SecondaryColor;
        public static Color Background => ThemeHelper.Background;
        public static Color White => ThemeHelper.White;
        public static Color Sidebar => Color.FromArgb(30, 30, 45);

        // CÁC MÀU BỊ THIẾU ĐÃ ĐƯỢC BỔ SUNG:
        public static Color Danger => ThemeHelper.DangerColor;
        public static Color Warning => ThemeHelper.WarningColor;
        public static Color Success => ThemeHelper.SuccessColor;

        // Alias cho các thẻ Dashboard cũ
        public static Color CardEarnings => ThemeHelper.WarningColor;
        public static Color CardTask => ThemeHelper.DangerColor;
        public static Color CardViews => ThemeHelper.SuccessColor;
        public static Color CardDownloads => Color.FromArgb(54, 185, 204); // Cyan
        public static Color TextDark => ThemeHelper.TextDark;
    }
}