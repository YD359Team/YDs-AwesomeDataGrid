using System;
using System.Collections.Generic;

namespace YDs_AwesomeDataGrid.Selection
{
    internal sealed class MultiRowSelector : ISelector
    {
        public bool IsVisible { get; set; }

        public HashSet<int> Rows { get; } = new HashSet<int>();

        public bool IsRowSelected(int row)
            => IsVisible && Rows.Contains(row);

        public bool IsCellSelected(int row, int column)
            => false;
    }
}