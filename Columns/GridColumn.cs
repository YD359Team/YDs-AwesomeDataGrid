using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using YDs_AwesomeDataGrid;
using YDs_AwesomeDataGrid.Enums;
using YDs_AwesomeDataGrid.InlineEditors;

namespace YDs_AwesomeDataGrid.Columns
{
    public abstract class GridColumn<T> : IGridColumn
    {
        public Type DataType => typeof(T);
        public virtual int Width { get; set; } = 120;
        public virtual string Name { get; }
        public virtual string HeaderText { get; }
        public virtual bool CanEdit { get; }
        public virtual bool CanSort { get; }
        public virtual bool CanUseInlineEditor { get; }

        protected GridColumn(string name, string headerText, bool canEdit, bool canSort)
        {
            this.Name = name;
            this.HeaderText = headerText;
            this.CanEdit = canEdit;
            this.CanSort = canSort;
        }

#if NET10_0_OR_GREATER
        public abstract IInlineEditor? CreateEditor();
#else
        public abstract IInlineEditor CreateEditor();
#endif
        public abstract void DrawCell(Graphics g, CellContext ctx);

        public abstract void DrawHeader(Graphics g, HeaderContext ctx);

        public abstract string Format(T value);

        string IGridColumn.Format(object value)
            => value is T v ? Format(v) : string.Empty;

        public abstract object GetDefaultValue();
    }
}
