using System;
using System.Drawing;
using YDs_AwesomeDataGrid.Helpers;
using YDs_AwesomeDataGrid.InlineEditors;

namespace YDs_AwesomeDataGrid.Columns
{
    public class TextColumn : GridColumn<string>
    {
        public override bool CanUseInlineEditor => true;

        public TextColumn(string name, string headerText, bool canEdit, bool canSort) : base(name, headerText, canEdit, canSort)
        {

        }

#if NET10_0_OR_GREATER
        public override IInlineEditor? CreateEditor()
#else
        public override IInlineEditor CreateEditor()
#endif
        {
            return new InlineTextEditor();
        }

        public override void DrawCell(Graphics g, CellContext ctx)
        {
            GraphicsHelper.DrawCell(g, ctx);
        }

        public override void DrawHeader(Graphics g, HeaderContext ctx)
        {
            GraphicsHelper.DrawHeader(g, ctx);
        }

        public override string Format(string value)
            => value ?? string.Empty;

        public override object GetDefaultValue() => string.Empty;
    }
}
