using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using YDs_AwesomeDataGrid.Helpers;
using YDs_AwesomeDataGrid.InlineEditors;

namespace YDs_AwesomeDataGrid.Columns
{
    public class ComboBoxColumn<T> : GridColumn<T> where T : struct, Enum
    {
        private readonly T[] _enumValues;

        public ComboBoxColumn(string name, string headerText, bool canEdit, bool canSort) : base(name, headerText, canEdit, canSort)
        {
            _enumValues = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }

#if NET10_0_OR_GREATER
        public override IInlineEditor? CreateEditor()
#else
        public override IInlineEditor CreateEditor()
#endif
        {
            return new InlineEnumEditor();
        }

        public override void DrawCell(Graphics g, CellContext ctx)
        {
            GraphicsHelper.DrawCell(g, ctx);
        }

        public override void DrawHeader(Graphics g, HeaderContext ctx)
        {
            GraphicsHelper.DrawHeader(g, ctx);
        }

        public override string Format(T value)
            => value.ToString();

        public override object GetDefaultValue() => _enumValues.FirstOrDefault();
    }
}
