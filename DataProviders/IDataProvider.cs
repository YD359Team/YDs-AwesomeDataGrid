using YDs_AwesomeDataGrid.Enums;

public interface IDataProvider
{
    event Action OnDataChanged;

    int RowCount { get; }

    object GetData(int row, int column);

    void SetData(int row, int column, object value);

    IEnumerable<ColumnDescription> GetColumnsDescription();

    void SortColumn(string dataPropertyName, ADGSortingDirection sortingDirection);
}
