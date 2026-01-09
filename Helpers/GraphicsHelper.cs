using System.Drawing;

namespace YDs_AwesomeDataGrid.Helpers
{
    internal static class GraphicsHelper
    {
        private static readonly StringFormat DefaultStringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };
        private static readonly StringFormat DebugStringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near,
        };

        public static void DrawString(Graphics g, string text, Font font, Brush brush, RectangleF layoutRectangle)
        {
            g.DrawString(text, font, brush, layoutRectangle, DefaultStringFormat);
        }

        public static void DrawDebug(Graphics g, object obj, Font font, Brush brush, RectangleF layoutRectangle)
        {
            if (layoutRectangle == Rectangle.Empty) return;

            if (obj is Rectangle rect)
            {
                g.DrawString($"[{rect.X},{rect.Y},{rect.Width},{rect.Height}]", font, brush, layoutRectangle, DebugStringFormat);
            }
            else
            {
                g.DrawString(obj.ToString(), font, brush, layoutRectangle, DebugStringFormat);
            }
        }
    }
}