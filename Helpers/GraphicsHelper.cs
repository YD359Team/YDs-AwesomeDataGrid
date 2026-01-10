using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using YDs_AwesomeDataGrid.Columns;

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

        public static void DrawCell(Graphics g, CellContext ctx)
        {
            if (ctx.IsSelected)
            {
                g.FillRectangle(Brushes.LightSkyBlue, ctx.Bounds);
            }
            else if (ctx.IsHovered)
            {
                g.FillRectangle(Brushes.LightCyan, ctx.Bounds);
            }
            else
            {
                g.FillRectangle(SystemBrushes.ControlLightLight, ctx.Bounds);
            }

            string text = ctx.Value?.ToString() ?? string.Empty;
            g.DrawString(text, ctx.Style.Font, Brushes.Black, ctx.Bounds);
            
            g.DrawRectangle(Pens.DarkGray, ctx.Bounds);
        }

        public static void DrawCheckBoxCell(Graphics g, CellContext ctx)
        {
            if (ctx.IsSelected)
            {
                g.FillRectangle(Brushes.LightSkyBlue, ctx.Bounds);
            }
            else if (ctx.IsHovered)
            {
                g.FillRectangle(Brushes.LightCyan, ctx.Bounds);
            }
            else
            {
                g.FillRectangle(SystemBrushes.ControlLightLight, ctx.Bounds);
            }

            if (Convert.ToBoolean(ctx.Value))
            { 
                CheckBoxRenderer.DrawCheckBox(g, ctx.Bounds.Location, CheckBoxState.CheckedNormal);
            }   
            else
            {
                CheckBoxRenderer.DrawCheckBox(g, ctx.Bounds.Location, CheckBoxState.UncheckedNormal);
            }

            g.DrawRectangle(Pens.DarkGray, ctx.Bounds);
        }
    }
}