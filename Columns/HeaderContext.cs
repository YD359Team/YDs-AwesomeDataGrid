using System.Drawing;
using YDs_AwesomeDataGrid.Enums;

namespace YDs_AwesomeDataGrid.Columns
{
    public readonly struct HeaderContext
    {
        public int ColumnIndex { get; }
        public Rectangle Bounds { get; }

        public string Text { get; }

        public bool IsHovered { get; }
        public bool IsPressed { get; }

        public bool IsSorted { get; }
        public ADGSortingDirection SortDirection { get; }

        public CellStyle Style { get; }

        public HeaderContext(
            int columnIndex,
            Rectangle bounds,
            string text,
            bool isHovered,
            bool isPressed,
            bool isSorted,
            ADGSortingDirection sortDirection,
            CellStyle style)
        {
            ColumnIndex = columnIndex;
            Bounds = bounds;
            Text = text;
            IsHovered = isHovered;
            IsPressed = isPressed;
            IsSorted = isSorted;
            SortDirection = sortDirection;
            Style = style;
        }
    }

}
