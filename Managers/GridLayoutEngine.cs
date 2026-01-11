using System;
using System.Drawing;
using System.Windows.Forms;

namespace YDs_AwesomeDataGrid.Managers
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

        public int GetLastVisibleColumn(int firstVisibleColumn)
        {
            int x = 0;
            int col = firstVisibleColumn;

            while (col < ColumnCount && x < GridRect.Width)
            {
                x += GetColumnWidth(col);
                col++;
            }

            return Math.Max(firstVisibleColumn, col - 1);
        }

        public int ColumnCount { get; private set; }
        public int VisibleRowCount { get; private set; }

        public bool NeedVertScroll { get; private set; }
        public bool NeedHorScroll { get; private set; }

        public int RowHeight => DEFAULT_ROW_HEIGHT;
        public int ColumnWidth => DEFAULT_COLUMN_WIDTH;
        public int HeaderHeight => DEFAULT_ROW_HEIGHT;
        public int RowHeaderWidth => DEFAULT_ROW_HEADER_WIDTH;
        private ColumnWidthManager _columnWidths;

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

        public void Recalc(
            Size controlSize,
            int rowCount,
            int columnCount,
            bool isRowHeaderVisible)
        {
            if (_columnWidths == null || _columnWidths.ColumnCount != columnCount)
            {
                _columnWidths = new ColumnWidthManager(columnCount, DEFAULT_COLUMN_WIDTH);
            }

            ColumnCount = columnCount;
            int rowHeaderWidth = isRowHeaderVisible ? RowHeaderWidth : 0;

            Rectangle workArea = new Rectangle(
                0,
                0,
                controlSize.Width,
                controlSize.Height
            );

            workArea.Y += HeaderHeight;
            workArea.Height -= HeaderHeight;

            Rectangle gridRect = new Rectangle(
                rowHeaderWidth,
                workArea.Y,
                Math.Max(0, workArea.Width - rowHeaderWidth),
                Math.Max(0, workArea.Height)
            );

            VisibleRowCount = Math.Max(1, gridRect.Height / RowHeight);

            NeedVertScroll = rowCount > VisibleRowCount;
            NeedHorScroll = _columnWidths.TotalWidth > gridRect.Width;

            if (NeedVertScroll)
                gridRect.Width -= SystemInformation.VerticalScrollBarWidth;

            if (NeedHorScroll)
                gridRect.Height -= SystemInformation.HorizontalScrollBarHeight;

            gridRect.Width = Math.Max(0, gridRect.Width);
            gridRect.Height = Math.Max(0, gridRect.Height);

            VisibleRowCount = Math.Max(1, gridRect.Height / RowHeight);
            NeedVertScroll = rowCount > VisibleRowCount;

            GridRect = gridRect;

            NeedVertScroll = rowCount > VisibleRowCount;
            if (NeedVertScroll)
            {
                GridRect = new Rectangle(
                    GridRect.X,
                    GridRect.Y,
                    GridRect.Width - SystemInformation.VerticalScrollBarWidth,
                    GridRect.Height
                );
            }

            NeedHorScroll = _columnWidths.TotalWidth > GridRect.Width;
            if (NeedHorScroll)
            {
                GridRect = new Rectangle(
                    GridRect.X,
                    GridRect.Y,
                    GridRect.Width,
                    GridRect.Height - SystemInformation.HorizontalScrollBarHeight
                );
            }

            RowHeaderRect = new Rectangle(
                0,
                GridRect.Y,
                GridRect.X,
                GridRect.Height
            );

            // Vertical scrollbar
            VertScrollRect = NeedVertScroll
                ? new Rectangle(
                    controlSize.Width - SystemInformation.VerticalScrollBarWidth,
                    HeaderHeight,
                    SystemInformation.VerticalScrollBarWidth,
                    controlSize.Height - HeaderHeight - (NeedHorScroll ? SystemInformation.HorizontalScrollBarHeight : 0)
                )
                : Rectangle.Empty;

            // Horizontal scrollbar
            HorScrollRect = NeedHorScroll
                ? new Rectangle(
                    GridRect.X,
                    controlSize.Height - SystemInformation.HorizontalScrollBarHeight,
                    GridRect.Width,
                    SystemInformation.HorizontalScrollBarHeight
                )
                : Rectangle.Empty;
        }

        public Rectangle GetHeaderRect(int col, int firstVisibleColumn)
        {
            if (_columnWidths == null)
                return Rectangle.Empty;

            int lastVisible = GetLastVisibleColumn(firstVisibleColumn);

            if (col < firstVisibleColumn || col > lastVisible)
                return Rectangle.Empty;

            int x = GridRect.X;
            for (int i = firstVisibleColumn; i < col; i++)
                x += _columnWidths[i];

            return Rectangle.Intersect(
                new Rectangle(x, 0, _columnWidths[col], RowHeight),
                new Rectangle(GridRect.X, 0, GridRect.Width, RowHeight)
            );
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

            int w = _columnWidths[col];

            return new Rectangle(x, y, w, RowHeight);
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

            if (localX < 0 || localY < 0)
                return false;

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

            if (_columnWidths == null)
                return false;

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


        public int GetColumnWidth(int col) => _columnWidths[col];

        public void SetColumnWidth(int col, int width)
        {
            _columnWidths[col] = width;
        }

        private void EnsureColumnWidths(int columnCount)
        {
            if (_columnWidths == null || _columnWidths.ColumnCount != columnCount)
            {
                _columnWidths = new ColumnWidthManager(columnCount, DEFAULT_COLUMN_WIDTH);
            }
        }
    }
}