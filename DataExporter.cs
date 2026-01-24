using System;
using System.IO;
using System.Linq;
using System.Text;
using YDs_AwesomeDataGrid.Columns;

namespace YDs_AwesomeDataGrid
{
    internal class DataExporter
    {
        internal static void ExportToCsv(IDataProvider dataProvider, IGridColumn[] columns, string fullPathToCsv)
        {
            using (StreamWriter writer = new StreamWriter(fullPathToCsv, false)) 
            { 
                int columnCount = columns.Length;
                int rowCount = dataProvider.RowCount;

                writer.WriteLine(string.Join(";", columns.Select(x => x.HeaderText).ToArray()));

                for (int row = 0; row < rowCount; row++)
                {
                    StringBuilder buffer = new StringBuilder();
                    for (int col = 0; col < columnCount; col++)
                    {
                        var data = dataProvider.GetData(row, col);
                        var formattedData = columns[col].Format(data) ?? "NULL";
                        buffer.Append(col + 1 == columnCount ? formattedData : formattedData + ";");
                    }
                    writer.WriteLine(buffer.ToString());
                }
                writer.Close();
            }
        }
    }
}