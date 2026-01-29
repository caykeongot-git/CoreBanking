using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace CoreBanking.WinUI.Controls
{
    public class FlatProgressBar : Control
    {
        private int _value = 0;
        private int _maximum = 100;
        private Color _progressColor = Color.Blue;

        public FlatProgressBar()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(200, 10); // Kích thước mặc định
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        [Category("Banking Props")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Value
        {
            get => _value;
            set
            {
                if (value < 0) _value = 0;
                else if (value > _maximum) _value = _maximum;
                else _value = value;
                Invalidate();
            }
        }

        [Category("Banking Props")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Maximum
        {
            get => _maximum;
            set
            {
                if (value < 1) _maximum = 1;
                else _maximum = value;

                if (_value > _maximum) _value = _maximum;
                Invalidate();
            }
        }

        [Category("Banking Props")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color ProgressColor
        {
            get => _progressColor;
            set { _progressColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.None; // Vẽ phẳng, không khử răng cưa cho nét

            // 1. Vẽ nền xám
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(230, 230, 230)))
            {
                e.Graphics.FillRectangle(bgBrush, 0, 0, this.Width, this.Height);
            }

            // 2. Vẽ thanh tiến trình (nếu có giá trị)
            if (_value > 0)
            {
                // Tính chiều dài thanh dựa trên % (Value / Maximum)
                float percent = (float)_value / _maximum;
                int width = (int)(percent * this.Width);

                if (width > this.Width) width = this.Width; // Không vẽ tràn

                using (SolidBrush barBrush = new SolidBrush(_progressColor))
                {
                    e.Graphics.FillRectangle(barBrush, 0, 0, width, this.Height);
                }
            }
        }
    }
}