using System.Drawing;
using YDs_AwesomeDataGrid.Styles;

namespace YDs_AwesomeDataGrid.Columns
{
    public readonly struct CellContext
    {
        public int Row { get; }
        public int ColumnIndex { get; }

        public Rectangle Bounds { get; }
        public Rectangle ContentBounds { get; }

        public object Value { get; }
        public string Pres { get; }

        public bool IsSelected { get; }
        public bool IsHovered { get; }

        public GridStyle GridStyle { get; }
        public CellStyle CellStyle { get; }

        public CellContext(
            int row,
            int columnIndex,
            Rectangle bounds,
            Rectangle contentBounds,
            object value,
            string pres,
            bool isSelected,
            bool isHovered,
            GridStyle gridStyle,
            CellStyle cellStyle)
        {
            Row = row;
            ColumnIndex = columnIndex;
            Bounds = bounds;
            ContentBounds = contentBounds;
            Value = value;
            Pres = pres;
            IsSelected = isSelected;
            IsHovered = isHovered;
            GridStyle = gridStyle;
            CellStyle = cellStyle;
        }
    }
}
