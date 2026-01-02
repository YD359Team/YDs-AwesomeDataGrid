public interface IDataProvider
{
    event Action OnDataChanged;

    object GetData(int row, int column);

    void SetData(int row, int column, object value);

    IEnumerable<ColumnDescription> GetColumnsDescription();

    int RowCount { get; }

    void SortColumn(string dataPropertyName, AwesomeDataGridSortingDirection sortingDirection);
}
