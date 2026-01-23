using System.Drawing;

namespace YDs_AwesomeDataGrid.Styles
{
    public class GridStyle
    {
        public readonly SolidBrush BackgroundBrush;
        public readonly Pen GridBorderPen;
        public readonly SolidBrush EditMaskBrush;
        public readonly Pen CellBorderPen;
        public readonly Pen CellBorderHoverPen;
        public readonly Pen CellBorderSelectedPen;
        public readonly SolidBrush CellBackgroundBrush;
        public readonly SolidBrush CellBackgroundHoverBrush;
        public readonly SolidBrush CellBackgroundSelectedBrush;
        public readonly SolidBrush HeaderBackgroundBrush;
        public readonly SolidBrush HeaderBackgroundHoverBrush;
        public readonly SolidBrush HeaderBackgroundPressedBrush;
        public readonly SolidBrush TextBrush;
        public readonly SolidBrush HighlightedTextBrush;
        public readonly SolidBrush ScrollBarBackground;
        public readonly SolidBrush ScrollBarThumb;

        public GridStyle(SolidBrush backgroundBrush, Pen gridBorderPen, SolidBrush editMaskBrush, 
            Pen cellBorderPen, Pen cellBorderHoverPen, Pen cellBorderSelectedPen, SolidBrush cellBackgroundBrush, 
            SolidBrush cellBackgroundHoverBrush, SolidBrush cellBackgroundSelectedBrush, SolidBrush headerBackgroundBrush, 
            SolidBrush headerBackgroundHoverBrush, SolidBrush headerBackgroundPressedBrush, SolidBrush textBrush,
            SolidBrush highlightedTextBrush, SolidBrush scrollBarBackground, SolidBrush scrollBarThumb)
        {
            BackgroundBrush = backgroundBrush;
            GridBorderPen = gridBorderPen;
            EditMaskBrush = editMaskBrush;
            CellBorderPen = cellBorderPen;
            CellBorderHoverPen = cellBorderHoverPen;
            CellBorderSelectedPen = cellBorderSelectedPen;
            CellBackgroundBrush = cellBackgroundBrush;
            CellBackgroundHoverBrush = cellBackgroundHoverBrush;
            CellBackgroundSelectedBrush = cellBackgroundSelectedBrush;
            HeaderBackgroundBrush = headerBackgroundBrush;
            HeaderBackgroundHoverBrush = headerBackgroundHoverBrush;
            HeaderBackgroundPressedBrush = headerBackgroundPressedBrush;
            TextBrush = textBrush;
            HighlightedTextBrush = highlightedTextBrush;
            ScrollBarBackground = scrollBarBackground;
            ScrollBarThumb = scrollBarThumb;
        }
    }
}