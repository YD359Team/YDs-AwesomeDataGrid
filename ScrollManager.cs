using System;
using System.Drawing;
using YDs_AwesomeDataGrid.Helpers;

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
            if (!layout.NeedVertScroll || rowCount <= 0)
            {
                data.VertThumb = Rectangle.Empty;
                return;
            }

            int visibleRows = layout.VisibleRowCount;
            int maxFirstRow = Math.Max(0, rowCount - visibleRows);

            float ratio = (float)visibleRows / rowCount;
            int thumbHeight = Math.Max(
                MIN_THUMB_SIZE,
                (int)(layout.VertScrollRect.Height * ratio));

            int scrollRange = layout.VertScrollRect.Height - thumbHeight;
            if (scrollRange < 0)
                scrollRange = 0;

            float t = maxFirstRow == 0
                ? 0f
                : viewport.FirstVisibleRow / (float)maxFirstRow;

            int offsetY = (int)(t * scrollRange);

            int thumbY = layout.VertScrollRect.Y + offsetY;

#if NET10_0_OR_GREATER
            thumbY = Math.Clamp(
                thumbY,
                layout.VertScrollRect.Top,
                layout.VertScrollRect.Bottom - thumbHeight);
#else
            thumbY = MathHelper.Clamp(
                thumbY,
                layout.VertScrollRect.Top,
                layout.VertScrollRect.Bottom - thumbHeight);
#endif

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

            int totalWidth = columnCount * layout.ColumnWidth;
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