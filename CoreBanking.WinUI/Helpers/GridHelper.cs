using System.Windows.Forms;
using System.Drawing;

namespace CoreBanking.WinUI.Helpers
{
    public static class GridHelper
    {
        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Header Style
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = ThemeHelper.PrimaryColor;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = ThemeHelper.BoldFont;
            grid.ColumnHeadersHeight = 45;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);

            // Row Style
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 240, 254); // Light Blue selection
            grid.DefaultCellStyle.SelectionForeColor = ThemeHelper.TextDark;
            grid.DefaultCellStyle.Font = ThemeHelper.NormalFont;
            grid.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            grid.RowTemplate.Height = 40;

            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AllowUserToAddRows = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
        }
    }
}