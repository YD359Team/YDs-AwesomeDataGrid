using System;
using System.Drawing;
using System.Windows.Forms;
using YDs_AwesomeDataGrid.Helpers;
using YDs_AwesomeDataGrid.InlineEditors;

namespace YDs_AwesomeDataGrid.Columns
{
    public class FloatColumn : GridColumn<float>
    {
        public override bool CanUseInlineEditor => false;

        public FloatColumn(string name, string headerText, bool canEdit, bool canSort) : base(name, headerText, canEdit, canSort)
        {
            // do nothing
        }

#if NET10_0_OR_GREATER
        public override IInlineEditor? CreateEditor()
#else
        public override IInlineEditor CreateEditor()
#endif
        {
            return new InlineFloat32Editor();
        }

        public override void DrawCell(Graphics g, CellContext ctx)
        {
            GraphicsHelper.DrawCell(g, ctx);
        }

        public override void DrawHeader(Graphics g, HeaderContext ctx)
        {
            GraphicsHelper.DrawHeader(g, ctx);
        }

        public override string Format(float value)
            => value.ToString();

        public override object GetDefaultValue() => 0f;
    }
}
