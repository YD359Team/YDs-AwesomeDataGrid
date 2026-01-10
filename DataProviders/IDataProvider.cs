using System;
using System.Collections.Generic;
using YDs_AwesomeDataGrid.Columns;
using YDs_AwesomeDataGrid.Enums;

public interface IDataProvider
{
    event Action OnDataChanged;

    int RowCount { get; }

    object GetData(int row, int column);

    void SetData(int row, int column, object value);

    IEnumerable<IGridColumn> GetColumnsDescription();

    void SortColumn(string dataPropertyName, ADGSortingDirection sortingDirection);
}
