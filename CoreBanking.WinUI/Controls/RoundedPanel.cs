using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace CoreBanking.WinUI.Controls
{
    public class RoundedPanel : Panel
    {
        // Thêm Browsable(false) để Visual Studio không cố đọc thuộc tính này -> Hết lỗi
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius { get; set; } = 20;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float GradientAngle { get; set; } = 90F;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color GradientTopColor { get; set; } = Color.White;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color GradientBottomColor { get; set; } = Color.White;

        public RoundedPanel()
        {
            this.BackColor = Color.Transparent;
            this.ForeColor = Color.Black;
            this.Size = new Size(350, 200);
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF rectF = new RectangleF(0, 0, this.Width, this.Height);
            if (BorderRadius > 2)
            {
                using (GraphicsPath path = GetPath(rectF, BorderRadius))
                using (Pen pen = new Pen(this.Parent?.BackColor ?? Color.White, 2))
                {
                    this.Region = new Region(path);
                    e.Graphics.DrawPath(pen, path);
                }
            }
            else
            {
                this.Region = new Region(rectF);
            }
        }

        private GraphicsPath GetPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float r2 = radius / 2f;
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Width - radius, rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}