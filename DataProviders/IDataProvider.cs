using YDs_AwesomeDataGrid.Enums;

public interface IDataProvider
{
    event Action OnDataChanged;

    object GetData(int row, int column);

    void SetData(int row, int column, object value);

    IEnumerable<ColumnDescription> GetColumnsDescription();

    int RowCount { get; }

    void SortColumn(string dataPropertyName, ADGSortingDirection sortingDirection);
}
