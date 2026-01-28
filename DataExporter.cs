using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        internal static async Task ExportToCsvAsync(IDataProvider dataProvider, IGridColumn[] columns, string fullPathToCsv, CancellationToken token)
        {
            using (StreamWriter writer = new StreamWriter(fullPathToCsv, false))
            {
                int columnCount = columns.Length;
                int rowCount = dataProvider.RowCount;

                await writer.WriteLineAsync(string.Join(";", columns.Select(x => x.HeaderText).ToArray()));

                for (int row = 0; row < rowCount; row++)
                {
                    if (token.IsCancellationRequested) break;

                    StringBuilder buffer = new StringBuilder();
                    for (int col = 0; col < columnCount; col++)
                    {
                        var data = dataProvider.GetData(row, col);
                        var formattedData = columns[col].Format(data) ?? "NULL";
                        buffer.Append(col + 1 == columnCount ? formattedData : formattedData + ";");
                    }
                    await writer.WriteLineAsync(buffer.ToString());
                }
                writer.Close();
            }
        }
    }
}