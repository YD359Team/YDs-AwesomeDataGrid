using System;
using System.Collections.Generic;
using YDs_AwesomeDataGrid.Enums;

public class EmptyDataProvider : IDataProvider
{
    public event Action OnDataChanged;

    public object GetData(int row, int column)
    {
        return null;
    }

    public void SetData(int row, int column, object value)
    {
        // Do nothing
    }

    public IEnumerable<ColumnDescription> GetColumnsDescription()
    {
        return Array.Empty<ColumnDescription>();
    }

    public int RowCount => 0;

    public void SortColumn(string dataPropertyName, ADGSortingDirection sortingDirection)
    {
        // Do nothing
    }
}