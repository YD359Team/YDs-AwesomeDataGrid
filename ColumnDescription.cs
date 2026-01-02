public readonly struct ColumnDescription
{
    public string HeaderText { get; }
    public string DataPropertyName { get; }
    public Type DataType { get; }
    public bool IsReadOnly { get; }
    public bool AllowSort { get; }

    public ColumnDescription(string headerText, string dataPropertyName, Type dateType, bool isReadOnly, bool allowSort = true)
    {
        this.HeaderText = headerText;
        this.DataPropertyName = dataPropertyName;
        this.DataType = dateType;
        this.IsReadOnly = isReadOnly;
        this.AllowSort = allowSort;
    }
}
