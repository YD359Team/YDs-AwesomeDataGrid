namespace YDs_AwesomeDataGrid
{
    sealed class CellVisual
    {
#if NET10_0_OR_GREATER
        public object? Value { get; }
        public string Text { get; } = string.Empty;
        public CellStyle Style { get; } = CellStyle.DefaultCell;
#else
        public object Value { get; }
        public string Text { get; } = string.Empty;
        public CellStyle Style { get; } = CellStyle.DefaultCell;
#endif

        public CellVisual(object value, string text, CellStyle style)
        {
            this.Value = value;
            this.Text = text;
            this.Style = style;
        }
    }
}