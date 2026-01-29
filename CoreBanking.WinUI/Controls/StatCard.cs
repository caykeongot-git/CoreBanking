using System.ComponentModel;
using System.Drawing.Drawing2D;
using CoreBanking.WinUI.Helpers;

namespace CoreBanking.WinUI.Controls
{
    public class StatCard : UserControl
    {
        private string _title = "TITLE";
        private string _value = "0";
        private string _footerText = "Footer";
        private Color _cardColor = ThemeColor.Primary;

        public StatCard()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(200, 100);
            this.BackColor = Color.Transparent;
        }

        [Category("Banking Props")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        [Category("Banking Props")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Value
        {
            get => _value;
            set { _value = value; Invalidate(); }
        }

        [Category("Banking Props")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string FooterText
        {
            get => _footerText;
            set { _footerText = value; Invalidate(); }
        }

        [Category("Banking Props")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color CardColor
        {
            get => _cardColor;
            set { _cardColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            using (var path = ThemeHelper.GetRoundedPath(rect, 15))
            using (var brush = new SolidBrush(_cardColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            using (var titleBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            using (var valueBrush = new SolidBrush(Color.White))
            using (var titleFont = new Font("Segoe UI", 9, FontStyle.Regular))
            using (var valueFont = new Font("Segoe UI", 18, FontStyle.Bold))
            using (var footerFont = new Font("Segoe UI", 8, FontStyle.Regular))
            {
                e.Graphics.DrawString(_title, titleFont, titleBrush, new PointF(15, 15));
                e.Graphics.DrawString(_value, valueFont, valueBrush, new PointF(15, 35));
                e.Graphics.DrawString(_footerText, footerFont, titleBrush, new PointF(15, 70));
            }
        }
    }
}