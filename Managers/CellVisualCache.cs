using System;
using System.Collections.Generic;
using System.Linq;

namespace YDs_AwesomeDataGrid.Managers
{
    sealed class CellVisualCache
    {
        private readonly Dictionary<CellKey, CellVisual> _cache = new Dictionary<CellKey, CellVisual>();

        private int _firstRow = -1;
        private int _lastRow = -1;

        public CellVisual GetOrCreate(int row, int column, Func<CellVisual> factory)
        {
            var key = new CellKey(row, column);

            if (!_cache.TryGetValue(key, out var visual))
            {
                visual = factory();
                _cache[key] = visual;
            }

            return visual;
        }

        public CellVisual Get(int row, int column)
        {
            if (!_cache.TryGetValue(new CellKey(row, column), out var visual))
                throw new InvalidOperationException("CellVisual not prepared");

            return visual;
        }

        public void UpdateViewport(int firstRow, int lastRow)
        {
            if (firstRow == _firstRow && lastRow == _lastRow)
                return;

            _firstRow = firstRow;
            _lastRow = lastRow;

            Trim();
        }

        private void Trim()
        {
            var toRemove = _cache.Keys
                .Where(k => k.Row < _firstRow || k.Row >= _lastRow)
                .ToList();

            foreach (var key in toRemove)
                _cache.Remove(key);
        }

        public void Invalidate(int row, int column)
        {
            _cache.Remove(new CellKey(row, column));
        }

        public void InvalidateRow(int row)
        {
            var keys = _cache.Keys
                .Where(k => k.Row == row)
                .ToList();

            foreach (var key in keys)
                _cache.Remove(key);
        }

        public void InvalidateAll()
        {
            _cache.Clear();
            _firstRow = _lastRow = -1;
        }
    }
}