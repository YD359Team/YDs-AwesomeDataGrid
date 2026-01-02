public class AwesomeDataGridColumn
{
    public string HeaderText { get; set; }
    public string DataPropertyName { get; set; }
    public Type DataType { get; set; }
    public bool IsReadOnly { get; set; }
    public bool AllowSort { get; set; }
    public AwesomeDataGridSortingDirection SortingDirection { get; set; }

    public AwesomeDataGridColumn(ColumnDescription columnDescription)
    {
        this.HeaderText = columnDescription.HeaderText;
        this.DataPropertyName = columnDescription.DataPropertyName;
        this.DataType = columnDescription.DataType;
        this.IsReadOnly = columnDescription.IsReadOnly;
        this.AllowSort = columnDescription.AllowSort;
    }
}
