namespace YDs_AwesomeDataGrid
{
    sealed class CellVisual
    {
#if NET10_0_OR_GREATER
        public string Text { get; init; } = string.Empty;

        // залог будущих стилей
        public CellStyle Style { get; init; } = CellStyle.Default;
#else
        public string Text { get; set; } = string.Empty;

        // залог будущих стилей
        public CellStyle Style { get; set; } = CellStyle.Default;
#endif

        // на будущее
        // public Image? Icon { get; init; }
        // public TextAlignment Alignment { get; init; }
    }
}