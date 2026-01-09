using System;
using System.Collections.Generic;
using System.Linq;

namespace YDs_AwesomeDataGrid
{
    internal sealed class ColumnWidthManager
    {
        private readonly List<int> _widths;

        public ColumnWidthManager(int columnCount, int defaultWidth)
        {
            _widths = Enumerable.Repeat(defaultWidth, columnCount).ToList();
        }

        public int ColumnCount => _widths.Count;

        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= _widths.Count)
                    return 130; // fallback ширина по умолчанию
                return _widths[index];
            }
            set
            {
                if (index < 0 || index >= _widths.Count)
                    return;
                _widths[index] = Math.Max(10, value);
            }
        }

        public int TotalWidth => _widths.Sum();

        public int GetX(int columnIndex)
        {
            if (columnIndex >= _widths.Count) return 0;
            int x = 0;
            for (int i = 0; i < columnIndex; i++)
                x += _widths[i];
            return x;
        }
    }
}