using System;
using System.Collections.Generic;

namespace YDs_AwesomeDataGrid.Selection
{
    internal sealed class SelectionController
    {
        public ISelector Selector { get; private set; } = new EmptySelector();

        public CellKey? Anchor { get; private set; }

        public void SelectCell(int row, int col)
        {
            Anchor = new CellKey(row, col);
            Selector = new CellSelector
            {
                Row = row,
                Column = col,
                IsVisible = true
            };
        }

        public void SelectRow(int row)
        {
            Anchor = new CellKey(row, 0);
            Selector = new RowSelector
            {
                Row = row,
                IsVisible = true
            };
        }

        public void CtrlSelectCell(int row, int col)
        {
            var key = new CellKey(row, col);

            if (Selector is MultiCellSelector multi)
            {
                if (!multi.Cells.Add(key))
                    multi.Cells.Remove(key);
            }
            else
            {
                var multiSelector = new MultiCellSelector
                {
                    IsVisible = true
                };
                multiSelector.Cells.Add(key);
                Selector = multiSelector;
            }

            if (Anchor == null)
                Anchor = key;
        }

        public void ShiftSelectCell(int row, int col)
        {
            if (Anchor == null)
            {
                SelectCell(row, col);
                return;
            }

            var cells = BuildCellRange(Anchor.Value, new CellKey(row, col));

            var selector = new MultiCellSelector
            {
                IsVisible = true
            };

            foreach (var cell in cells)
                selector.Cells.Add(cell);

            Selector = selector;
        }

        public void Clear()
        {
            Anchor = null;
            Selector = new EmptySelector();
        }

        private static HashSet<CellKey> BuildCellRange(CellKey a, CellKey b)
        {
            int rowStart = Math.Min(a.Row, b.Row);
            int rowEnd = Math.Max(a.Row, b.Row);

            int colStart = Math.Min(a.Column, b.Column);
            int colEnd = Math.Max(a.Column, b.Column);

            var result = new HashSet<CellKey>();

            for (int row = rowStart; row <= rowEnd; row++)
            {
                for (int col = colStart; col <= colEnd; col++)
                {
                    result.Add(new CellKey(row, col));
                }
            }

            return result;
        }
    }
}