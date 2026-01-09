using System;
using System.Drawing;
using System.Windows.Forms;

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
            NeedHorScroll = columnCount * ColumnWidth > gridRect.Width;

            if (NeedVertScroll)
                gridRect.Width -= SystemInformation.VerticalScrollBarWidth;

            if (NeedHorScroll)
                gridRect.Height -= SystemInformation.HorizontalScrollBarHeight;

            gridRect.Width = Math.Max(0, gridRect.Width);
            gridRect.Height = Math.Max(0, gridRect.Height);

            VisibleRowCount = Math.Max(1, gridRect.Height / RowHeight);
            NeedVertScroll = rowCount > VisibleRowCount;

            GridRect = gridRect;

            RowHeaderRect = isRowHeaderVisible
                ? new Rectangle(
                    0,
                    GridRect.Y,
                    rowHeaderWidth,
                    GridRect.Height)
                : Rectangle.Empty;

            VertScrollRect = NeedVertScroll
                ? new Rectangle(
                    GridRect.Right,
                    HeaderHeight,
                    SystemInformation.VerticalScrollBarWidth,
                    controlSize.Height - HeaderHeight
                )
                : Rectangle.Empty;

            HorScrollRect = NeedHorScroll
                ? new Rectangle(
                    GridRect.X,
                    GridRect.Bottom,
                    GridRect.Width,
                    SystemInformation.HorizontalScrollBarHeight)
                : Rectangle.Empty;

            _columnWidths = new ColumnWidthManager(columnCount, DEFAULT_COLUMN_WIDTH);
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
            col = firstVisibleCol + localX / ColumnWidth;

            return true;
        }

        public bool TryGetColumnHeaderByPoint(
            Point p,
            int firstVisibleCol,
            out int col)
        {
            col = -1;

            if (p.Y < 0 || p.Y >= HeaderHeight)
                return false;

            if (p.X < GridRect.X || p.X >= GridRect.Right)
                return false;

            col = firstVisibleCol + (p.X - GridRect.X) / ColumnWidth;
            return true;
        }
    }
}