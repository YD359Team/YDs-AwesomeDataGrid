using System;
using System.Drawing;
using YDs_AwesomeDataGrid.Managers;

namespace YDs_AwesomeDataGrid
{
    public sealed class CellStyle : IEquatable<CellStyle>
    {
        public static CellStyle DefaultCell => new CellStyle(SystemColors.ControlText, Color.Transparent, FontManager.ModernCommon);
        public static CellStyle DefaultHeader => new CellStyle(SystemColors.ControlText, Color.Transparent, FontManager.ModernTitle);

#if NET10_0_OR_GREATER
        public Color ForeColor { get; }
        public Color BackColor { get; }
        public Font? Font { get; }
#else
        public Color ForeColor { get; }
        public Color BackColor { get; }
        public Font Font { get; }
#endif

        public CellStyle(Color foreColor, Color backColor, Font font)
        {
            ForeColor = foreColor;
            BackColor = backColor;
            Font = font;
        }

        public bool Equals(CellStyle other)
        {
            return this.ForeColor == other.ForeColor
                && this.BackColor == other.BackColor
                && this.Font.FontFamily == other.Font.FontFamily
                && this.Font.Size == other.Font.Size;
        }
    }
}