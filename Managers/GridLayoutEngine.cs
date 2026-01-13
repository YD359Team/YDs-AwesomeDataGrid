using System;
using System.Drawing;
using System.Windows.Forms;
using YDs_AwesomeDataGrid.Columns;

namespace YDs_AwesomeDataGrid.Managers
{
    internal sealed class GridLayoutEngine
    {
        private const int DEFAULT_ROW_HEIGHT = 25;
        private const int DEFAULT_COLUMN_WIDTH = 130;
        private const int DEFAULT_ROW_HEADER_WIDTH = 40;

        public Rectangle GridRect { get; private set; }
        public Rectangle HeaderRect { get; private set; }
        public Rectangle RowHeaderRect { get; private set; }
        public Rectangle VertScrollRect { get; private set; }
        public Rectangle HorScrollRect { get; private set; }

        public int ColumnCount { get; private set; }
        public int VisibleRowCount { get; private set; }

        public bool NeedVertScroll { get; private set; }
        public bool NeedHorScroll { get; private set; }

        public int RowHeight => DEFAULT_ROW_HEIGHT;
        public int HeaderHeight => DEFAULT_ROW_HEIGHT;
        public int RowHeaderWidth => DEFAULT_ROW_HEADER_WIDTH;

        public int TotalColumnsWidth =>
            _columnWidths?.TotalWidth ?? 0;

        private ColumnWidthManager _columnWidths;

        public void Recalc(
            Size controlSize,
            int rowCount,
            int columnCount,
            bool isRowHeaderVisible)
        {
            if (_columnWidths == null || _columnWidths.ColumnCount != columnCount)
                _columnWidths = new ColumnWidthManager(columnCount, DEFAULT_COLUMN_WIDTH);

            ColumnCount = columnCount;

            int rowHeaderWidth = isRowHeaderVisible ? RowHeaderWidth : 0;

            Rectangle gridRect = new Rectangle(
                rowHeaderWidth,
                HeaderHeight,
                Math.Max(0, controlSize.Width - rowHeaderWidth),
                Math.Max(0, controlSize.Height - HeaderHeight)
            );

            int visibleRows = Math.Max(1, gridRect.Height / RowHeight);

            bool needVert = rowCount > visibleRows;
            bool needHor = _columnWidths.TotalWidth > gridRect.Width;

            if (needVert)
                gridRect.Width -= SystemInformation.VerticalScrollBarWidth;

            if (needHor)
                gridRect.Height -= SystemInformation.HorizontalScrollBarHeight;

            gridRect.Width = Math.Max(0, gridRect.Width);
            gridRect.Height = Math.Max(0, gridRect.Height);

            VisibleRowCount = Math.Max(1, gridRect.Height / RowHeight);
            NeedVertScroll = rowCount > VisibleRowCount;
            NeedHorScroll = _columnWidths.TotalWidth > gridRect.Width;

            GridRect = gridRect;

            HeaderRect = new Rectangle(
                0,
                0,
                controlSize.Width - (NeedVertScroll ? SystemInformation.VerticalScrollBarWidth : 0),
                HeaderHeight
            );

            RowHeaderRect = new Rectangle(
                0,
                GridRect.Y,
                GridRect.X,
                GridRect.Height
            );

            VertScrollRect = NeedVertScroll
                ? new Rectangle(
                    controlSize.Width - SystemInformation.VerticalScrollBarWidth,
                    HeaderHeight,
                    SystemInformation.VerticalScrollBarWidth,
                    controlSize.Height - HeaderHeight
                        - (NeedHorScroll ? SystemInformation.HorizontalScrollBarHeight : 0)
                )
                : Rectangle.Empty;

            HorScrollRect = NeedHorScroll
                ? new Rectangle(
                    GridRect.X,
                    controlSize.Height - SystemInformation.HorizontalScrollBarHeight,
                    GridRect.Width,
                    SystemInformation.HorizontalScrollBarHeight
                )
                : Rectangle.Empty;
        }

        public void InitColumnWidths(int columnCount, Func<int, int> widthProvider)
        {
            _columnWidths = new ColumnWidthManager(columnCount, DEFAULT_COLUMN_WIDTH);

            for (int i = 0; i < columnCount; i++)
                _columnWidths[i] = widthProvider(i);
        }

        public int GetColumnWidth(int col) => _columnWidths[col];

        public void SetColumnWidth(int col, int width)
        {
            _columnWidths[col] = width;
        }

        public int GetLastVisibleColumn(int firstVisibleColumn)
        {
            int x = 0;
            int col = firstVisibleColumn;

            while (col < ColumnCount &&
                   x + _columnWidths[col] <= GridRect.Width)
            {
                x += _columnWidths[col];
                col++;
            }

            return Math.Max(firstVisibleColumn, col - 1);
        }

        public int VisibleColumnCount(int firstVisibleCol)
        {
            int width = 0;
            int count = 0;

            for (int i = firstVisibleCol; i < _columnWidths.ColumnCount; i++)
            {
                width += _columnWidths[i];
                if (width > GridRect.Width)
                    break;

                count++;
            }

            return Math.Max(1, count);
        }

        public Rectangle GetHeaderRect(int col, int firstVisibleColumn)
        {
            int lastVisible = GetLastVisibleColumn(firstVisibleColumn);

            if (col < firstVisibleColumn || col > lastVisible)
                return Rectangle.Empty;

            int x = GridRect.X;
            for (int i = firstVisibleColumn; i < col; i++)
                x += _columnWidths[i];

            return new Rectangle(x, 0, _columnWidths[col], HeaderHeight);
        }

        public Rectangle GetCellRect(
            int row,
            int col,
            int firstVisibleRow,
            int firstVisibleCol)
        {
            if (row < firstVisibleRow || row >= firstVisibleRow + VisibleRowCount)
                return Rectangle.Empty;

            int x = GridRect.X;
            for (int i = firstVisibleCol; i < col; i++)
                x += _columnWidths[i];

            int y = GridRect.Y + (row - firstVisibleRow) * RowHeight;

            return new Rectangle(x, y, _columnWidths[col], RowHeight);
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

            int x = 0;
            for (int i = firstVisibleCol; i < _columnWidths.ColumnCount; i++)
            {
                int w = _columnWidths[i];
                if (localX >= x && localX < x + w)
                {
                    col = i;
                    return true;
                }
                x += w;
            }

            return false;
        }

        public bool TryGetColumnHeaderByPoint(
            Point p,
            int firstVisibleCol,
            out int col)
        {
            col = -1;

            if (p.Y < 0 || p.Y >= HeaderHeight)
                return false;

            int x = GridRect.X;

            for (int i = firstVisibleCol; i < _columnWidths.ColumnCount; i++)
            {
                int w = _columnWidths[i];
                Rectangle r = new Rectangle(x, 0, w, HeaderHeight);

                if (r.Contains(p))
                {
                    col = i;
                    return true;
                }

                x += w;
                if (x >= GridRect.Right)
                    break;
            }

            return false;
        }
    }
}