using System.Collections.Generic;

namespace YDs_AwesomeDataGrid.Selection
{
    internal sealed class MultiCellSelector : ISelector
    {
        public bool IsVisible { get; set; }

        public HashSet<CellKey> Cells { get; } = new HashSet<CellKey>();

        public bool IsCellSelected(int row, int column)
            => IsVisible && Cells.Contains(new CellKey(row, column));

        public bool IsRowSelected(int row)
            => false;
    }
}