using System;
using System.Drawing;
using YDs_AwesomeDataGrid.Helpers;
using YDs_AwesomeDataGrid.InlineEditors;

namespace YDs_AwesomeDataGrid.Columns
{
    public class ImageColumn : GridColumn<Bitmap>
    {
        public ImageColumn(string name, string headerText) : base(name, headerText, false, false)
        {

        }

#if NET10_0_OR_GREATER
        public override IInlineEditor? CreateEditor()
#else
        public override IInlineEditor CreateEditor()
#endif
        {
            throw new NotImplementedException(nameof(ImageColumn) + " don't supports inline editor");
        }

        public override void DrawCell(Graphics g, CellContext ctx)
        {
            GraphicsHelper.DrawImage(g, ctx);
        }

        public override void DrawHeader(Graphics g, HeaderContext ctx)
        {
            GraphicsHelper.DrawHeader(g, ctx);
        }

        public override string Format(Bitmap value)
            => default;

        public override object GetDefaultValue() => false;
    }
}
