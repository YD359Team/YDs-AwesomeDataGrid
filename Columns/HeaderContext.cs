using System.Drawing;
using YDs_AwesomeDataGrid.Enums;
using YDs_AwesomeDataGrid.Styles;

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

        public GridStyle GridStyle { get; }
        public CellStyle CellStyle { get; }

        public HeaderContext(
            int columnIndex,
            Rectangle bounds,
            string text,
            bool isHovered,
            bool isPressed,
            bool isSorted,
            ADGSortingDirection sortDirection,
            GridStyle gridStyle,
            CellStyle cellStyle)
        {
            ColumnIndex = columnIndex;
            Bounds = bounds;
            Text = text;
            IsHovered = isHovered;
            IsPressed = isPressed;
            IsSorted = isSorted;
            SortDirection = sortDirection;
            GridStyle = gridStyle;
            CellStyle = cellStyle;
        }
    }
}
