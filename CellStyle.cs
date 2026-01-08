namespace YDs_AwesomeDataGrid
{
    sealed class CellStyle
    {
        public static readonly CellStyle Default = new();

        public Color ForeColor { get; init; } = SystemColors.ControlText;
        public Color BackColor { get; init; } = Color.Transparent;
        public Font? Font { get; init; } = null;

        // позже:
        // public Pen? Border { get; init; }
    }
}