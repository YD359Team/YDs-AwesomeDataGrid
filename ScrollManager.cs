namespace YDs_AwesomeDataGrid
{
    internal sealed class ScrollManager
    {
        private const int MIN_THUMB_SIZE = 10;
        private const int DEFAULT_COLUMN_WIDTH = 130;

        public void Update(
            ViewPort viewport,
            GridLayoutEngine layout,
            int rowCount,
            int columnCount,
            ScrollBarData scrollBarData)
        {
            UpdateVertical(viewport, layout, rowCount, scrollBarData);
            UpdateHorizontal(viewport, layout, columnCount, scrollBarData);
        }

        private static void UpdateVertical(
            ViewPort viewport,
            GridLayoutEngine layout,
            int rowCount,
            ScrollBarData data)
        {
            if (!layout.NeedVertScroll)
            {
                data.VertThumb = Rectangle.Empty;
                return;
            }

            int visibleRows = layout.VisibleRowCount;
            int maxFirstRow = Math.Max(1, rowCount - visibleRows);

            float ratio = (float)visibleRows / rowCount;
            int thumbHeight = Math.Max(
                MIN_THUMB_SIZE,
                (int)(layout.VertScrollRect.Height * ratio));

            int thumbY = layout.VertScrollRect.Y +
                (int)((viewport.FirstVisibleRow / (float)maxFirstRow)
                * (layout.VertScrollRect.Height - thumbHeight));

            data.VertThumb = new Rectangle(
                layout.VertScrollRect.X,
                thumbY,
                layout.VertScrollRect.Width,
                thumbHeight);
        }

        private static void UpdateHorizontal(
            ViewPort viewport,
            GridLayoutEngine layout,
            int columnCount,
            ScrollBarData data)
        {
            if (!layout.NeedHorScroll)
            {
                data.HorThumb = Rectangle.Empty;
                return;
            }

            int totalWidth = columnCount * DEFAULT_COLUMN_WIDTH;
            float ratio = (float)layout.GridRect.Width / totalWidth;

            int thumbWidth = Math.Max(
                MIN_THUMB_SIZE,
                (int)(layout.HorScrollRect.Width * ratio));

            int maxFirstCol = Math.Max(
                1,
                columnCount - (layout.GridRect.Width / DEFAULT_COLUMN_WIDTH));

            int thumbX = layout.HorScrollRect.X +
                (int)((viewport.FirstVisibleColumn / (float)maxFirstCol)
                * (layout.HorScrollRect.Width - thumbWidth));

            data.HorThumb = new Rectangle(
                thumbX,
                layout.HorScrollRect.Y,
                thumbWidth,
                layout.HorScrollRect.Height);
        }
    }
}