using System;
using System.Drawing;
using System.Windows.Forms;

namespace YDs_AwesomeDataGrid.InlineEditors
{
    public interface IInlineEditor
    {
        event Action OnLostFocus;
        event Action<IInlineEditor> OnEndEdit;

        Control Grid { get; set; }
        Type ColumnType { get; }
        Control Editor { get; }
        object Value { get; }

#if NET10_0_OR_GREATER
        void BeginEdit(Rectangle rect, object? cellValue, object[]? enumValues = null);
#else
        void BeginEdit(Rectangle rect, object cellValue, object[] enumValues = null);
#endif
        void Close();
    }
}