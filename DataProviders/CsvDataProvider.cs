using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YDs_AwesomeDataGrid.Columns;
using YDs_AwesomeDataGrid.Enums;

public class CsvDataProvider : IDataProvider
{
    public event Action OnDataChanged;

    public int RowCount => _data.Count;

    private TextColumn[] _columns;
    private int _lineItemsCount;
    private List<string[]> _data;

    public CsvDataProvider(string pathToCsv)
    {
        using (var sr = new StreamReader(pathToCsv))
        {
            string firstLine = sr.ReadLine();

            _columns = firstLine.Split(',').Select(x => new TextColumn(x, x, false, false)).ToArray();
            _lineItemsCount = _columns.Length;
            _data = new List<string[]>();

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                _data.Add(line.Split(','));
            }
        }
    }

    public object GetData(int row, int column)
    {
        return _data[row][column];
    }

    public void SetData(int row, int column, object value)
    {
        _data[row][column] = value.ToString();
    }

    public IEnumerable<IGridColumn> GetColumns()
    {
        return _columns;
    }

    public void SortColumn(string dataPropertyName, ADGSortingDirection sortingDirection)
    {
        // Do nothing
    }
}