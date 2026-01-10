using System.Drawing;
using YDs_AwesomeDataGrid.Enums;

namespace YDs_AwesomeDataGrid.Columns
{
    public readonly struct HeaderContext
    {
        public int ColumnIndex { get; }
        public Rectangle Bounds { get; }

        public bool IsHovered { get; }
        public bool IsPressed { get; }

        public bool AllowSort { get; }
        public ADGSortingDirection SortingDirection { get; }

        public HeaderContext(
            int columnIndex,
            Rectangle bounds,
            bool isHovered,
            bool isPressed,
            bool allowSort,
            ADGSortingDirection sortingDirection)
        {
            ColumnIndex = columnIndex;
            Bounds = bounds;
            IsHovered = isHovered;
            IsPressed = isPressed;
            AllowSort = allowSort;
            SortingDirection = sortingDirection;
        }
    }
}
