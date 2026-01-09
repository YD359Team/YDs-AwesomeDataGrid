using System;
using YDs_AwesomeDataGrid.Enums;

namespace YDs_AwesomeDataGrid
{
#if NET10_0_OR_GREATER
    public sealed record AwesomeDataGridColumn
#else
    public sealed class AwesomeDataGridColumn
#endif
    {
        public string HeaderText { get; set; }
        public string DataPropertyName { get; set; }
        public Type DataType { get; set; }
        public bool IsReadOnly { get; set; }
        public bool AllowSort { get; set; }
        public ADGSortingDirection SortingDirection { get; set; }

        public AwesomeDataGridColumn(ColumnDescription columnDescription)
        {
            this.HeaderText = columnDescription.HeaderText;
            this.DataPropertyName = columnDescription.DataPropertyName;
            this.DataType = columnDescription.DataType;
            this.IsReadOnly = columnDescription.IsReadOnly;
            this.AllowSort = columnDescription.AllowSort;
        }
    }
}