using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YDs_AwesomeDataGrid.InlineEditors
{
    public static class InlineEditors
    {
        public static IInlineEditor CreateByColumnType(Type type)
        {
            if (type.IsEnum) return new InlineEnumEditor(Enum.GetValues(type).Cast<object>().ToArray());
            if (type == typeof(int)) return new InlineInt32Editor();
            if (type == typeof(float)) return new InlineFloat32Editor();
            if (type == typeof(DateTime)) return new InlineDateTimeEditor();
            return new InlineTextEditor();
        }
    }
}
