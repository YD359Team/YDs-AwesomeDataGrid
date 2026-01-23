using System.Drawing;

namespace YDs_AwesomeDataGrid.Styles
{
    internal static class PredefinedGridStyles
    {
        public static readonly GridStyle Light = new GridStyle(
            backgroundBrush: new SolidBrush(SystemColors.ControlDark),
            gridBorderPen: new Pen(Color.Black),
            editMaskBrush: new SolidBrush(Color.FromArgb(100, Color.DarkGray)), 
            cellBorderPen: new Pen(Color.DarkGray),
            cellBorderHoverPen: new Pen(Color.DeepSkyBlue, 1f),
            cellBorderSelectedPen: new Pen(SystemColors.HighlightText, 1f),
            cellBackgroundBrush: new SolidBrush(SystemColors.ControlLightLight),
            cellBackgroundHoverBrush: new SolidBrush(Color.LightCyan),
            cellBackgroundSelectedBrush: new SolidBrush(Color.LightSkyBlue),
            headerBackgroundBrush: new SolidBrush(Color.Gainsboro),
            headerBackgroundHoverBrush: new SolidBrush(Color.LightGray),
            headerBackgroundPressedBrush: new SolidBrush(Color.DarkGray),
            textBrush: new SolidBrush(SystemColors.ControlText),
            highlightedTextBrush: new SolidBrush(SystemColors.HighlightText),
            scrollBarBackground: new SolidBrush(Color.Gainsboro),
            scrollBarThumb: new SolidBrush(SystemColors.ControlDarkDark));

        public static readonly GridStyle Dark = new GridStyle(
            backgroundBrush: new SolidBrush(Color.FromArgb(30, 30, 30)),
            gridBorderPen: new Pen(Color.FromArgb(45, 45, 45)),
            editMaskBrush: new SolidBrush(Color.FromArgb(120, 0, 0, 0)),
            cellBorderPen: new Pen(Color.FromArgb(60, 60, 60)),
            cellBorderHoverPen: new Pen(Color.FromArgb(0, 122, 204), 1f),
            cellBorderSelectedPen: new Pen(Color.FromArgb(0, 122, 204), 1f),
            cellBackgroundBrush: new SolidBrush(Color.FromArgb(37, 37, 38)),
            cellBackgroundHoverBrush: new SolidBrush(Color.FromArgb(45, 45, 48)),
            cellBackgroundSelectedBrush: new SolidBrush(Color.FromArgb(51, 153, 255)),
            headerBackgroundBrush: new SolidBrush(Color.FromArgb(45, 45, 48)),
            headerBackgroundHoverBrush: new SolidBrush(Color.FromArgb(62, 62, 64)),
            headerBackgroundPressedBrush: new SolidBrush(Color.FromArgb(28, 28, 28)),
            textBrush: new SolidBrush(Color.FromArgb(220, 220, 220)),
            highlightedTextBrush: new SolidBrush(Color.White),
            scrollBarBackground: new SolidBrush(Color.FromArgb(45, 45, 48)),
            scrollBarThumb: new SolidBrush(Color.FromArgb(90, 90, 90))
        );
    }
}