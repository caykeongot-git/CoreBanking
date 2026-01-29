using System.Windows.Forms;
using System.Drawing;
using CoreBanking.WinUI.Helpers;
using System.ComponentModel;

namespace CoreBanking.WinUI.Controls
{
    // Đảm bảo cùng namespace CoreBanking.WinUI.Controls
    public class StatCard : RoundedPanel
    {
        private Label lblTitle;
        private Label lblValue;
        private PictureBox iconBox;

        public StatCard(string title, string value, Color color)
        {
            this.Size = new Size(260, 120);
            this.BackColor = Color.White;
            this.BorderRadius = 15;
            this.Padding = new Padding(20);

            // Icon Placeholder
            iconBox = new PictureBox();
            iconBox.Size = new Size(50, 50);
            iconBox.Location = new Point(this.Width - 70, 20);
            iconBox.BackColor = ThemeHelper.SecondaryColor;
            this.Controls.Add(iconBox);

            // Label Title
            lblTitle = new Label();
            lblTitle.Text = title.ToUpper();
            lblTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblTitle.ForeColor = ThemeHelper.TextGray;
            lblTitle.Location = new Point(20, 25);
            lblTitle.AutoSize = true;
            this.Controls.Add(lblTitle);

            // Label Value
            lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblValue.ForeColor = ThemeHelper.PrimaryColor;
            lblValue.Location = new Point(15, 55);
            lblValue.AutoSize = true;
            this.Controls.Add(lblValue);
        }
    }
}