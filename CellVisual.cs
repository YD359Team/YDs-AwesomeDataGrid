namespace YDs_AwesomeDataGrid
{
    sealed class CellVisual
    {
        public string Text { get; init; } = string.Empty;

        // залог будущих стилей
        public CellStyle Style { get; init; } = CellStyle.Default;

        // на будущее
        // public Image? Icon { get; init; }
        // public TextAlignment Alignment { get; init; }
    }
}