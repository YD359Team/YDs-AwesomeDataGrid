namespace YDs_AwesomeDataGrid
{
    internal sealed class GridLayoutEngine
    {
        private const int DEFAULT_ROW_HEIGHT = 25;
        private const int DEFAULT_COLUMN_WIDTH = 130;
        private const int DEFAULT_ROW_HEADER_WIDTH = 40;

        public Rectangle GridRect { get; private set; }
        public Rectangle RowHeaderRect { get; private set; }
        public Rectangle VertScrollRect { get; private set; }
        public Rectangle HorScrollRect { get; private set; }

        public int VisibleRowCount { get; private set; }

        public bool NeedVertScroll { get; private set; }
        public bool NeedHorScroll { get; private set; }

        public int RowHeight => DEFAULT_ROW_HEIGHT;
        public int ColumnWidth => DEFAULT_COLUMN_WIDTH;
        public int HeaderHeight => DEFAULT_ROW_HEIGHT;
        public int RowHeaderWidth => DEFAULT_ROW_HEADER_WIDTH;

        public void Recalc(
            Size controlSize,
            int rowCount,
            int columnCount,
            bool isRowHeaderVisible)
        {
            int rowHeaderWidth = isRowHeaderVisible ? DEFAULT_ROW_HEADER_WIDTH : 0;

            // 1️⃣ рабочая область (без скроллов)
            Rectangle workArea = new Rectangle(
                0,
                0,
                controlSize.Width,
                controlSize.Height
            );

            // 2️⃣ учитываем header
            workArea.Y += HeaderHeight;
            workArea.Height -= HeaderHeight;

            // 3️⃣ предварительный GridRect
            GridRect = new Rectangle(
                rowHeaderWidth,
                workArea.Y,
                workArea.Width - rowHeaderWidth,
                workArea.Height
            );

            VisibleRowCount = Math.Max(1, GridRect.Height / RowHeight);

            NeedVertScroll = rowCount > VisibleRowCount;
            NeedHorScroll = columnCount * ColumnWidth > GridRect.Width;

            // 4️⃣ урезаем под скроллы
            int vertWidth = NeedVertScroll ? SystemInformation.VerticalScrollBarWidth : 0;
            int horHeight = NeedHorScroll ? SystemInformation.HorizontalScrollBarHeight : 0;

            GridRect = new Rectangle(
                rowHeaderWidth,
                workArea.Y,
                workArea.Width - rowHeaderWidth - vertWidth,
                workArea.Height - horHeight
            );

            // 5️⃣ RowHeader — СТРОГО ПО GridRect.Y
            RowHeaderRect = isRowHeaderVisible
                ? new Rectangle(
                    0,
                    GridRect.Y,
                    rowHeaderWidth,
                    GridRect.Height)
                : Rectangle.Empty;

            // 6️⃣ финальный пересчёт строк
            VisibleRowCount = Math.Max(1, GridRect.Height / RowHeight);
            NeedVertScroll = rowCount > VisibleRowCount;

            // 7️⃣ ScrollBars
            VertScrollRect = NeedVertScroll
                ? new Rectangle(
                    GridRect.Right,
                    GridRect.Y,
                    SystemInformation.VerticalScrollBarWidth,
                    GridRect.Height)
                : Rectangle.Empty;

            HorScrollRect = NeedHorScroll
                ? new Rectangle(
                    GridRect.X,
                    GridRect.Bottom,
                    GridRect.Width,
                    SystemInformation.HorizontalScrollBarHeight)
                : Rectangle.Empty;
        }

        public Rectangle GetCellRect(
            int row,
            int col,
            int firstVisibleRow,
            int firstVisibleCol)
        {
            if (row < firstVisibleRow || row >= firstVisibleRow + VisibleRowCount)
                return Rectangle.Empty;

            if (col < firstVisibleCol)
                return Rectangle.Empty;

            int x = GridRect.X + (col - firstVisibleCol) * ColumnWidth;
            int y = GridRect.Y + (row - firstVisibleRow) * RowHeight;

            return new Rectangle(x, y, ColumnWidth, RowHeight);
        }

        public bool TryGetCellByPoint(
            Point p,
            int firstVisibleRow,
            int firstVisibleCol,
            out int row,
            out int col)
        {
            row = -1;
            col = -1;

            if (!GridRect.Contains(p))
                return false;

            int localX = p.X - GridRect.X;
            int localY = p.Y - GridRect.Y;

            row = firstVisibleRow + localY / RowHeight;
            col = firstVisibleCol + localX / ColumnWidth;

            return true;
        }

        public bool TryGetColumnHeaderByPoint(
            Point p,
            int firstVisibleCol,
            out int col)
        {
            col = -1;

            if (p.Y > HeaderHeight)
                return false;

            if (p.X < GridRect.X)
                return false;

            col = firstVisibleCol + (p.X - GridRect.X) / ColumnWidth;
            return true;
        }
    }
}