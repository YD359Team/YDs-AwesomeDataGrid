using System.Drawing;

namespace YDs_AwesomeDataGrid.Columns
{
    public readonly struct CellContext
    {
        // Позиция
        public int Row { get; }
        public int ColumnIndex { get; }

        // Геометрия
        public Rectangle Bounds { get; }
        public Rectangle ContentBounds { get; }

        // Данные
        public object Value { get; }
        public string Pres { get; }

        // Состояние
        public bool IsSelected { get; }
        public bool IsHovered { get; }

        // Стиль
        public CellStyle Style { get; }

        public CellContext(
            int row,
            int columnIndex,
            Rectangle bounds,
            Rectangle contentBounds,
            object value,
            string pres,
            bool isSelected,
            bool isHovered,
            CellStyle style)
        {
            Row = row;
            ColumnIndex = columnIndex;
            Bounds = bounds;
            ContentBounds = contentBounds;
            Value = value;
            Pres = pres;
            IsSelected = isSelected;
            IsHovered = isHovered;
            Style = style;
        }
    }
}
