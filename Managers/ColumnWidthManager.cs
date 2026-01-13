using System;
using System.Collections.Generic;
using System.Linq;
using YDs_AwesomeDataGrid.Helpers;

namespace YDs_AwesomeDataGrid.Managers
{
    internal sealed class ColumnWidthManager
    {
        public int TotalWidth => _totalWidth;
        private int _totalWidth;

        private readonly List<int> _widths;

        private const int MaxColumnWidth = 2000;

        public ColumnWidthManager(int columnCount, int defaultWidth)
        {
            _widths = Enumerable.Repeat(defaultWidth, columnCount).ToList();
            _totalWidth = columnCount * defaultWidth;
        }

        public int ColumnCount => _widths.Count;


        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= _widths.Count)
                    return 130;
                return _widths[index];
            }
            set
            {
                if (index < 0 || index >= _widths.Count)
                    return;
                int newWidth = Math.Max(10, value);
                _widths[index] = MathHelper.Clamp(newWidth, 10, MaxColumnWidth);
                int delta = newWidth - _widths[index];

                _widths[index] = newWidth;
                _totalWidth += delta;
            }
        }

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