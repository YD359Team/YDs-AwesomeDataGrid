using System.Drawing;

namespace YDs_AwesomeDataGrid
{
    public sealed class CellStyle
    {
        public static readonly CellStyle Default = new CellStyle();

#if NET10_0_OR_GREATER
        public Color ForeColor { get; init; } = SystemColors.ControlText;
        public Color BackColor { get; init; } = Color.Transparent;
        public Font? Font { get; init; } = null;
#else
        public Color ForeColor { get; set; } = SystemColors.ControlText;
        public Color BackColor { get; set; } = Color.Transparent;
        public Font Font { get; set; } = null;
#endif

        // позже:
        // public Pen? Border { get; init; }
    }
}