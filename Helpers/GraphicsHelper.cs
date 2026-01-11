using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using YDs_AwesomeDataGrid.Columns;
using YDs_AwesomeDataGrid.Enums;

namespace YDs_AwesomeDataGrid.Helpers
{
    internal static class GraphicsHelper
    {
        private static readonly StringFormat DefaultStringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };

        public static void DrawString(Graphics g, string text, Font font, Brush brush, RectangleF layoutRectangle)
        {
            g.DrawString(text, font, brush, layoutRectangle, DefaultStringFormat);
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

            string text = ctx.Pres ?? string.Empty;
            g.DrawString(text, ctx.Style.Font, Brushes.Black, ctx.Bounds, DefaultStringFormat);
            
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

            Rectangle bounds = ctx.Bounds;
            bounds.Inflate(-6, -3);
            bool value = ctx.Value is bool b && b;
            if (value)
            { 
                CheckBoxRenderer.DrawCheckBox(g, bounds.Location, ctx.IsHovered ? CheckBoxState.CheckedHot : CheckBoxState.CheckedNormal);
            }   
            else
            {
                CheckBoxRenderer.DrawCheckBox(g, bounds.Location, ctx.IsHovered ? CheckBoxState.UncheckedHot : CheckBoxState.UncheckedNormal);
            }

            g.DrawRectangle(Pens.DarkGray, ctx.Bounds);
        }

        public static void DrawHeader(Graphics g, HeaderContext ctx)
        {
            // background
            if (ctx.IsPressed)
                g.FillRectangle(Brushes.DarkGray, ctx.Bounds);
            else if (ctx.IsHovered)
                g.FillRectangle(Brushes.LightGray, ctx.Bounds);
            else
                g.FillRectangle(Brushes.Gainsboro, ctx.Bounds);

            // text + sort icon
            string text = ctx.Text;
            if (ctx.IsSorted)
                text += ctx.SortDirection == ADGSortingDirection.Ascending ? " ▲" : " ▼";

            GraphicsHelper.DrawString(
                g,
                text,
                ctx.Style.Font,
                Brushes.Black,
                ctx.Bounds
            );

            g.DrawRectangle(Pens.DarkGray, ctx.Bounds);
        }
    }
}