using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using YDs_AwesomeDataGrid.InlineEditors;

namespace YDs_AwesomeDataGrid.Columns
{
    public interface IGridColumn
    {
        Type DataType { get; }
        string Name { get; }
        string HeaderText { get; }
        int Width { get; set; }

        void DrawCell(Graphics g, CellContext ctx);
        void DrawHeader(Graphics g, HeaderContext ctx);

        bool CanEdit { get; }
        bool CanSort { get; }

#if NET10_0_OR_GREATER
        IInlineEditor? CreateEditor();
#else
        IInlineEditor CreateEditor();
#endif

        string Format(object value);
        object GetDefaultValue();
    }
}
