namespace YDs_AwesomeDataGrid
{
    sealed class CellVisual
    {
#if NET10_0_OR_GREATER
        public object? Value { get; init; }
        public string Text { get; init; } = string.Empty;
        public CellStyle Style { get; init; } = CellStyle.Default;
#else
        public object Value { get; private set; }
        public string Text { get; private set; } = string.Empty;
        public CellStyle Style { get; private set; } = CellStyle.Default;
#endif

        public CellVisual(object value, string text, CellStyle style)
        {
            this.Value = value;
            this.Text = text;
            this.Style = style;
        }
    }
}