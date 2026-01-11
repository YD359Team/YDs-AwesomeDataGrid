using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using YDs_AwesomeDataGrid.Enums;
using YDs_AwesomeDataGrid.Helpers;
using YDs_AwesomeDataGrid.InlineEditors;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using YDs_AwesomeDataGrid.Managers;
using YDs_AwesomeDataGrid.Columns;

namespace YDs_AwesomeDataGrid
{
    public class AwesomeDataGrid : ExtendedControl
    {
        #region PrivateEvent
        private event Action ViewportChanged;
        #endregion

        #region PublicProperties

        #region StylesOverrides
        private Font _font = FontManager.ModernCommon;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new Font Font
        {
            get => _font;
            set
            {
                _font = value;
                base.Font = _font;
                Invalidate();
            }
        }
        #endregion

        #region GridDimensions
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object this[int row, int column]
        {
            get => this.DataProvider.GetData(row, column);
            set => this.DataProvider.SetData(row, column, value);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ColumnCount { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int RowCount { get; private set; }

        private ADGSelectionTypes _selectionType = ADGSelectionTypes.Cell;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ADGSelectionTypes SelectionType
        {
            get => _selectionType;
            set
            {
                _selectionType = value;
                Invalidate();
            }
        }

        private bool _isRowHeaderVisible = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsRowHeaderVisible
        {
            get => _isRowHeaderVisible;
            set
            {
                _isRowHeaderVisible = value;
                Invalidate();
            }
        }
        #endregion

        #region Data
        private static readonly EmptyDataProvider EmptyProvider = new EmptyDataProvider();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public IDataProvider DataProvider
        {
            get => _dataProvider ?? EmptyProvider;
            set
            {
                if (value.GetType() != typeof(EmptyDataProvider))
                {
                    if (_dataProvider != null)
                    {
                        _dataProvider.OnDataChanged -= AwesomeDataGrid_OnDataChanged;
                    }
                    _dataProvider = value ?? EmptyProvider;
                    LoadData();
                    InitEditorsForColumns();
                    RecalcRects();
                    UpdateVisibleCells();
                    Invalidate();
                    _dataProvider.OnDataChanged += AwesomeDataGrid_OnDataChanged;
                }
            }
        }
        private IDataProvider _dataProvider;
        #endregion
        #endregion

        #region PrivateProperties
        private int LastVisibleColumn =>
            _layout.GetLastVisibleColumn(_viewPort.FirstVisibleColumn);

        private CellKey EditingCellAddress { get; set; }
        #endregion

        #region PrivateFields
        private readonly GridLayoutEngine _layout = new GridLayoutEngine();
        private readonly ScrollManager _scrollManager = new ScrollManager();

        private readonly ViewPort _viewPort = new ViewPort();
        private readonly ScrollBarData _scrollBarData = new ScrollBarData();
        private readonly Selector _selector = new Selector();
        private readonly HoverSelector _hoverSelector = new HoverSelector();
        private GridInnerState _gridInnerState;
        private bool _needVertScroll;
        private bool _needHorScroll;
        private bool _isScrollVertHovered;
        private bool _isScrollHorHovered;
        private HoverStates _hoverState;

        // состояние
        private int _hotRow = -1;
        private int _selectedRow = -1;

        private IGridColumn[] _columns = Array.Empty<IGridColumn>();
        private const int HeaderResizeGripWidth = 4;

        #region InlineEditors
        private readonly Dictionary<string, IInlineEditor> _editors = new Dictionary<string, IInlineEditor>();
        private IInlineEditor _currentEditor;
        #endregion

        #region VisibleCellCache
        private readonly CellVisualCache _cellCache = new CellVisualCache();
        private int _cachedFirstRow = -1;
        private int _cachedLastRow = -1;
        #endregion

        #region ResizeColumns
        private bool _isResizingColumn;
        private int _resizingColumnIndex;
        private int _resizeStartX;
        private int _resizeStartWidth;
        #endregion

        private int _hoveredHeaderCol = -1;
        private int _pressedHeaderCol = -1;
        #region SortColumns
        private string _sortedColumnName;
        private ADGSortingDirection _sortingDirection = ADGSortingDirection.None;
        #endregion     

        #region DragThumb
        private bool _isDraggingVertThumb;
        private bool _isDraggingHorThumb;
        private Point _dragStartMousePos;
        private int _dragStartFirstVisibleRow;
        private int _dragStartFirstVisibleCol;
        #endregion

        #region Graphics
        private readonly Brush _maskBrush = new SolidBrush(Color.FromArgb(100, Color.DarkGray));
        private readonly SolidBrush _thumbBrush = new SolidBrush(SystemColors.ControlDarkDark);
        private readonly Pen _selectedBorderPen = new Pen(SystemColors.HighlightText, 1f);
        private readonly Pen _hoveredBorderPen = new Pen(Color.DeepSkyBlue, 1f);
        #endregion

        #region Styles
        private readonly CellStyle _defaultCellStyle = new CellStyle()
        {
             Font = FontManager.ModernCommon
        };
        private readonly CellStyle _defaultColHeaderStyle = new CellStyle()
        {
            Font = FontManager.ModernTitle
        };
        #endregion
        #endregion

        #region Constructor
        public AwesomeDataGrid()
        {
            // Enable double buffering and custom painting
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.Selectable, true);
        }
        #endregion

        #region PublicAPI
        public void SetData(int row, int column, object value)
        {
            this.DataProvider.SetData(row, column, value);
            InvalidateCellCache(row, column);
        }

        public object GetData(int row, int column)
        {
            return this.DataProvider.GetData(row, column);
        }
        #endregion

        #region ControlOverrides
        protected override void InitLayout()
        {
            base.InitLayout();
            //
            RecalcRects();
            ViewportChanged += OnViewportChanged; 
        }

        private void InitEditorsForColumns()
        {
            foreach (IGridColumn column in _columns)
            {
                // checkbox is not inline editor
                if (column is CheckBoxColumn || column is ImageColumn) continue;

                var editor = column.CreateEditor();
                editor.Grid = this;
                _editors[column.Name] = editor;
                editor.OnLostFocus += InlineEditor_OnLostFocus;
                editor.OnEndEdit += InlineEditor_OnEndEdit;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillRectangle(SystemBrushes.ControlDark, this.ClientRectangle);

            if (DataProvider == EmptyProvider)
                return;

            DrawRowHeaders(e.Graphics, _viewPort.FirstVisibleRow);
            DrawColumnHeaders(e.Graphics);
            DrawCells(e.Graphics);
            DrawVerticalGridLineAfterLastColumn(e.Graphics);
            DrawScrollBars(e.Graphics);
            if (_gridInnerState == GridInnerState.Editing)
            {
                DrawMask(e.Graphics);
            }
            DrawBorder(e.Graphics);
        }

        public Rectangle GetRowHeaderCellRect(int row, int firstVisibleRow)
        {
            if (row < firstVisibleRow || row >= firstVisibleRow + _layout.VisibleRowCount)
                return Rectangle.Empty;

            int y = _layout.GridRect.Y + (row - firstVisibleRow) * _layout.RowHeight;

            return new Rectangle(
                _layout.RowHeaderRect.X,
                y,
                _layout.RowHeaderRect.Width,
                _layout.RowHeight
            );
        }

        override protected void OnResize(EventArgs e)
        {
            base.OnResize(e);

            _viewPort.Width = this.Width;
            _viewPort.Height = this.Height;
            RecalcRects();
            UpdateVisibleCells();
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // ESC
            if (e.KeyCode == Keys.Escape)
            {
                if (_gridInnerState == GridInnerState.Editing)
                {
                    ChangeGridState(GridInnerState.Default);
                    return;
                }

                // Toggle selection visibility
                _selector.IsVisible = !_selector.IsVisible;
                SmartInvalidate(GetCellRect(_selector.Row, _selector.Column));
            }
            // Space or Enter
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                if (!_selector.IsVisible) return;

                int col = _selector.Column;
                if (col < 0 || col >= ColumnCount) return;
                if (!_columns[col].CanEdit) return;

                int row = _selector.Row;
                RequestCellEditing(row, col);
            }
            // Del
            else if (e.KeyCode == Keys.Delete)
            {
                if (!_selector.IsVisible) return;

                int col = _selector.Column;
                if (col < 0 || col >= ColumnCount) return;
                if (!_columns[col].CanEdit) return;

                int row = _selector.Row;
                this.DataProvider.SetData(row, col, GetDefaultValueForColumn(_columns[col]));
                InvalidateCellCache(row, col);
                SmartInvalidate(GetCellRect(row, col));
            }
            // Ctrl+C
            else if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
            {
                if (!_selector.IsVisible) return;
                int row = _selector.Row;
                int col = _selector.Column;
                Clipboard.SetText(_columns[col].Format(this.DataProvider.GetData(row, col)));

            }
            // Ctrl+V
            else if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
            {
                if (!_selector.IsVisible) return;
                int row = _selector.Row;
                int col = _selector.Column;
                if (!_columns[col].CanEdit) return;
                string clipboardText = Clipboard.GetText();
#if NET10_0_OR_GREATER
                object? value = clipboardText;
#else
                object value = clipboardText;
#endif
                try
                {
                    if (_columns[col].DataType == typeof(int))
                        value = int.Parse(clipboardText);
                    else if (_columns[col].DataType == typeof(float))
                        value = float.Parse(clipboardText);
                    else if (_columns[col].DataType == typeof(double))
                        value = double.Parse(clipboardText);
                    else if (_columns[col].DataType == typeof(decimal))
                        value = decimal.Parse(clipboardText);
                    else if (_columns[col].DataType == typeof(bool))
                        value = bool.Parse(clipboardText);
                    else if (_columns[col].DataType == typeof(DateTime))
                        value = DateTime.Parse(clipboardText);
                }
                catch
                {
                    // ignore parse errors
                    return;
                }
                this.DataProvider.SetData(row, col, value);
                InvalidateCellCache(row, col);
                SmartInvalidate(GetCellRect(row, col));
            }
            // arrows
            else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up || e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
            {
                int newRow = _selector.Row;
                int newCol = _selector.Column;

                switch (e.KeyCode)
                {
                    case Keys.Down: newRow++; break;
                    case Keys.Up: newRow--; break;
                    case Keys.Right: newCol++; break;
                    case Keys.Left: newCol--; break;
                }

                MoveTo(newRow, newCol);
            }
            // pgup pgdn
            else if (e.KeyCode == Keys.PageDown || e.KeyCode == Keys.PageUp)
            {
                int newRow = _selector.Row;
                int newCol = _selector.Column;

                switch (e.KeyCode)
                {
                    case Keys.PageDown: newRow += 10; break;
                    case Keys.PageUp: newRow -= 10; break;
                }

                MoveTo(newRow, newCol);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _hoverSelector.IsVisible = false;
            if (_hoverState == HoverStates.Cell)
            {
                _hoverSelector.IsVisible = false;
                SmartInvalidate(GetCellRect(_hoverSelector.Row, _hoverSelector.Column));
            }
            _isScrollVertHovered = false;
            _isScrollHorHovered = false;
            _hoverState = HoverStates.None;
            SmartInvalidate(GetCellRect(_selector.Row, _selector.Column));
            SmartInvalidate(_layout.VertScrollRect);
            SmartInvalidate(_layout.HorScrollRect);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_gridInnerState == GridInnerState.Editing) return;

            Rectangle mouseRect = new Rectangle(e.Location, new Size(1, 1));

            if (_gridInnerState != GridInnerState.Editing &&
                TryGetColumnHeaderByPoint(e.Location, out int col) &&
                IsOnHeaderResizeGrip(e.Location, col))
            {
                Cursor = Cursors.VSplit;
            }
            else if (!_isResizingColumn)
            {
                Cursor = Cursors.Default;
            }

            if (_isResizingColumn)
            {
                int dx = e.X - _resizeStartX;
                _layout.SetColumnWidth(
                    _resizingColumnIndex,
                    _resizeStartWidth + dx
                );

                RecalcRects();
                UpdateVisibleCells();
                Invalidate();
                return;
            }

            if (_isDraggingVertThumb)
            {
                int dy = e.Y - _dragStartMousePos.Y;

                int scrollRange = _layout.VertScrollRect.Height - _scrollBarData.VertThumb.Height;
                if (scrollRange <= 0) return;

                float ratio = dy / (float)scrollRange;
                int maxFirstRow = Math.Max(0, RowCount - _layout.VisibleRowCount);

                _viewPort.FirstVisibleRow = _dragStartFirstVisibleRow + (int)(ratio * maxFirstRow);
                _viewPort.FirstVisibleRow = MathHelper.Clamp(_viewPort.FirstVisibleRow, 0, maxFirstRow);

                UpdateScrollThumbs();
                UpdateVisibleCells();
                Invalidate();
                return;
            }

            if (_isDraggingHorThumb)
            {
                int dx = e.X - _dragStartMousePos.X;

                int scrollRange = _layout.HorScrollRect.Width - _scrollBarData.HorThumb.Width;
                if (scrollRange <= 0) return;

                float ratio = dx / (float)scrollRange;
                int maxFirstCol = Math.Max(0, ColumnCount - _layout.VisibleColumnCount(_viewPort.FirstVisibleColumn));

                _viewPort.FirstVisibleColumn = _dragStartFirstVisibleCol + (int)(ratio * maxFirstCol);
                _viewPort.FirstVisibleColumn = MathHelper.Clamp(_viewPort.FirstVisibleColumn, 0, maxFirstCol);

                UpdateScrollThumbs();
                UpdateVisibleCells();
                Invalidate();
                return;
            }

            if (_layout.VertScrollRect.IntersectsWith(mouseRect))
            {
                if (_hoverState == HoverStates.VerticalScroll) return;

                _isScrollVertHovered = true;
                _isScrollHorHovered = false;
                SmartInvalidate(_layout.VertScrollRect);
                _hoverState = HoverStates.VerticalScroll;
                return;
            }
            else if (_layout.HorScrollRect.IntersectsWith(mouseRect))
            {
                if (_hoverState == HoverStates.HorizontalScroll) return;

                _isScrollVertHovered = false;
                _isScrollHorHovered = true;
                SmartInvalidate(_layout.HorScrollRect);
                _hoverState = HoverStates.HorizontalScroll;
                return;
            }

            _isScrollVertHovered = false;
            _isScrollHorHovered = false;

            int lastHoveredRow = _hoverSelector.Row;
            int lastHoveredCol = _hoverSelector.Column;

            if (TryGetColumnHeaderByPoint(e.Location, out col))
            {
                if (_hoveredHeaderCol != col)
                {
                    int old = _hoveredHeaderCol;
                    _hoveredHeaderCol = col;

                    if (old >= 0)
                        SmartInvalidate(GetHeaderRect(old));

                    SmartInvalidate(GetHeaderRect(col));
                }
            }
            else if (_hoveredHeaderCol != -1)
            {
                SmartInvalidate(GetHeaderRect(_hoveredHeaderCol));
                _hoveredHeaderCol = -1;
            }

            if (_layout.GridRect.IntersectsWith(mouseRect) && TryGetCellByPoint(e.Location, out int row, out col))
            {
                if (_hoverState == HoverStates.Cell && lastHoveredCol == col && lastHoveredRow == row) return;

                _hoverSelector.Row = row;
                _hoverSelector.Column = col;
                _hoverSelector.IsVisible = true;
                _hoverState = HoverStates.Cell;
                SmartInvalidate(GetCellRect(row, col));
            }
            else
            {
                _hoverSelector.IsVisible = false;
            }
            SmartInvalidate(GetCellRect(lastHoveredRow, lastHoveredCol));
            SmartInvalidate(_layout.VertScrollRect);
            SmartInvalidate(_layout.HorScrollRect);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left) return;

            Rectangle mouseRect = new Rectangle(e.Location, new Size(1, 1));
            bool isInGrid = _layout.GridRect.IntersectsWith(mouseRect);

            if (TryGetColumnHeaderByPoint(e.Location, out int col) &&
                IsOnHeaderResizeGrip(e.Location, col))
            {
                _isResizingColumn = true;
                _resizingColumnIndex = col;
                _resizeStartX = e.X;
                _resizeStartWidth = _layout.GetColumnWidth(col);
                Capture = true;
                return;
            }

            Rectangle vertThumb = _scrollBarData.VertThumb;
            Rectangle horThumb = _scrollBarData.HorThumb;

            if (vertThumb.Contains(e.Location))
            {
                _isDraggingVertThumb = true;
                _dragStartMousePos = e.Location;
                _dragStartFirstVisibleRow = _viewPort.FirstVisibleRow;
                Capture = true; 
                return;
            }

            if (horThumb.Contains(e.Location))
            {
                _isDraggingHorThumb = true;
                _dragStartMousePos = e.Location;
                _dragStartFirstVisibleCol = _viewPort.FirstVisibleColumn;
                Capture = true;
                return;
            }

            // find column header by point
            if (TryGetColumnHeaderByPoint(e.Location, out int headerCol))
            {
                IGridColumn column = _columns[headerCol];

                if (!column.CanSort) return;

                _hoveredHeaderCol = headerCol;
                _pressedHeaderCol = headerCol;

                ADGSortingDirection currentSortingDir = _sortingDirection;
                if (currentSortingDir == ADGSortingDirection.None)
                    currentSortingDir = ADGSortingDirection.Ascending;
                else if (currentSortingDir == ADGSortingDirection.Ascending)
                    currentSortingDir = ADGSortingDirection.Descending;
                else if (currentSortingDir == ADGSortingDirection.Descending)
                    currentSortingDir = ADGSortingDirection.Ascending;
                else
                    currentSortingDir = ADGSortingDirection.None;

                _sortedColumnName = column.Name;
                _sortingDirection = currentSortingDir;

                this.DataProvider.SortColumn(_sortedColumnName, _sortingDirection);

                ViewportChanged?.Invoke();
                _cellCache.InvalidateAll();
                UpdateVisibleCells();
                Invalidate();
            }
            // find cell by point
            else if (isInGrid && TryGetCellByPoint(e.Location, out int row, out col))
            {
                int oldRow = _selector.Row;
                int oldCol = _selector.Column;

                _selector.Row = row;
                _selector.Column = col;
                _selector.IsVisible = true;

                EnsureSelectionVisible();

                SmartInvalidate(GetCellRect(oldRow, oldCol));
                SmartInvalidate(GetCellRect(row, col));
            }

            if (!this.Focused) Focus();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _isResizingColumn = false;
            Capture = false;
            Cursor = Cursors.Default;

            if (_pressedHeaderCol != -1)
            {
                SmartInvalidate(GetHeaderRect(_pressedHeaderCol));
                _pressedHeaderCol = -1;
            }

            _isDraggingVertThumb = false;
            _isDraggingHorThumb = false;
            Capture = false;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (_layout.GridRect.Contains(e.Location) && TryGetCellByPoint(e.Location, out int row, out int col))
            {
                if (col >= _columns.Length) return;
                if (!_columns[col].CanEdit) return;

                RequestCellEditing(row, col);
            }
        }

        private void RequestCellEditing(int row, int col)
        {
            if (_columns[col] is CheckBoxColumn)
            {
                SetData(row, col, !(bool)GetData(row, col));
                UpdateVisibleCells();
                SmartInvalidate(GetCellRect(_selector.Row, _selector.Column));
                return;
            }

            ChangeGridState(GridInnerState.Editing);

            if (_editors.TryGetValue(_columns[col].Name, out var editor))
            {
                _currentEditor = editor;
                this.EditingCellAddress = new CellKey(row, col);
                editor.BeginEdit(GetCellRect(row, col), GetData(this.EditingCellAddress.Row, this.EditingCellAddress.Column));
            }

            return;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (_gridInnerState == GridInnerState.Editing) return;

            if (e.Delta > 0)
                _viewPort.FirstVisibleRow = Math.Max(0, _viewPort.FirstVisibleRow - 1);
            else
                _viewPort.FirstVisibleRow = Math.Min(
                    _viewPort.FirstVisibleRow + 1,
                    Math.Max(0, RowCount - _layout.VisibleRowCount)
                );

            UpdateScrollThumbs();

            UpdateVisibleCells();
            Invalidate();
        }

#endregion

        #region PrivateMethods
        private void LoadData()
        {
            // reset viewport
            _viewPort.FirstVisibleRow = 0;
            _viewPort.FirstVisibleColumn = 0;
            // load columns
            _columns = this.DataProvider.GetColumnsDescription().ToArray();

            this.ColumnCount = _columns.Length;
            this.RowCount = this.DataProvider.RowCount;
        }

        // recalc layout
        private void RecalcRects()
        {
            _layout.Recalc(
                this.Size,
                RowCount,
                ColumnCount,
                IsRowHeaderVisible
            );

            _needVertScroll = _layout.NeedVertScroll;
            _needHorScroll = _layout.NeedHorScroll;

            UpdateScrollThumbs();
            ViewportChanged?.Invoke();
        }

        private Rectangle GetCellRect(int row, int col)
        {
            return _layout.GetCellRect(
                row,
                col,
                _viewPort.FirstVisibleRow,
                _viewPort.FirstVisibleColumn
            );
        }

        private bool TryGetCellByPoint(Point p, out int row, out int col)
        {
            return _layout.TryGetCellByPoint(
                p,
                _viewPort.FirstVisibleRow,
                _viewPort.FirstVisibleColumn,
                out row,
                out col
            );
        }

        private bool TryGetColumnHeaderByPoint(Point p, out int col)
        {
            return _layout.TryGetColumnHeaderByPoint(
                p,
                _viewPort.FirstVisibleColumn,
                out col
            );
        }

        private Rectangle GetHeaderRect(int col)
        {
            return _layout.GetHeaderRect(col, _viewPort.FirstVisibleColumn);
        }

        private bool IsOnHeaderResizeGrip(Point p, int col)
        {
            Rectangle r = GetHeaderRect(col);
            if (r.IsEmpty) return false;

            Rectangle grip = new Rectangle(
                r.Right - HeaderResizeGripWidth,
                r.Top,
                HeaderResizeGripWidth,
                r.Height
            );

            return grip.Contains(p);
        }

        private void UpdateScrollThumbs()
        {
            _scrollManager.Update(
                _viewPort,
                _layout,
                this.RowCount,
                this.ColumnCount,
                _scrollBarData
            );
        }

        #region CellCache
        // visible cell cache
        private void UpdateVisibleCells()
        {
            int startRow = _viewPort.FirstVisibleRow;
            int endRow = Math.Min(RowCount, startRow + _layout.VisibleRowCount);

            if (startRow != _cachedFirstRow || endRow != _cachedLastRow)
            {
                TrimCache(startRow, endRow);
                _cachedFirstRow = startRow;
                _cachedLastRow = endRow;
            }

            for (int row = startRow; row < endRow; row++)
            {
                for (int col = _viewPort.FirstVisibleColumn; col <= LastVisibleColumn; col++)
                {
                    _cellCache.GetOrCreate(row, col, () =>
                    {
                        var value = DataProvider.GetData(row, col);
                        return new CellVisual(
                            value,
                            _columns[col].Format(value),
                            ResolveCellStyle(row, col, value)
                        );
                    });
                }
            }
        }

        private void TrimCache(int firstRow, int lastRow)
        {
            for (int row = _cachedFirstRow; row < firstRow; row++)
                _cellCache.InvalidateRow(row);

            for (int row = lastRow; row < _cachedLastRow; row++)
                _cellCache.InvalidateRow(row);
        }

        private void InvalidateCellCache(int row, int col)
        {
            _cellCache.Invalidate(row, col);
        }

        private CellStyle ResolveCellStyle(int row, int col, object value)
        {
            // TODO: change to logic
            return CellStyle.Default;
        }
        #endregion

        private void EnsureSelectionVisible()
        {
            bool updated = false;

            if (_selector.Row < _viewPort.FirstVisibleRow)
            {
                _viewPort.FirstVisibleRow = _selector.Row;
                updated = true;
            }
            else if (_selector.Row >= _viewPort.FirstVisibleRow + _layout.VisibleRowCount)
            {
                _viewPort.FirstVisibleRow = _selector.Row - _layout.VisibleRowCount + 1;
                updated = true;
            }

            if (_selector.Column < _viewPort.FirstVisibleColumn)
            {
                _viewPort.FirstVisibleColumn = _selector.Column;
                updated = true;
            }
            else if (_selector.Column > LastVisibleColumn)
            {
                _viewPort.FirstVisibleColumn = _selector.Column;
                updated = true;
            }

            if (updated)
            {
                UpdateVisibleCells();
                UpdateScrollThumbs();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SmartInvalidate(Rectangle rect)
        {
            if (!rect.IsEmpty) 
                Invalidate(rect);
        }

        // move cursor to row,cel
        private void MoveTo(int row, int col)
        {
            row = Math.Max(0, Math.Min(RowCount - 1, row));
            col = Math.Max(0, Math.Min(ColumnCount - 1, col));

            _selector.Row = row;
            _selector.Column = col;
            _selector.IsVisible = true;

            int firstRow = _viewPort.FirstVisibleRow;
            int lastRow = firstRow + _layout.VisibleRowCount - 1;

            if (row < firstRow)
                _viewPort.FirstVisibleRow = row;
            else if (row > lastRow)
                _viewPort.FirstVisibleRow = row - _layout.VisibleRowCount + 1;

            int firstCol = _viewPort.FirstVisibleColumn;
            int lastVisibleCol = LastVisibleColumn;

            if (col < _viewPort.FirstVisibleColumn)
                _viewPort.FirstVisibleColumn = col;
            else if (col > lastVisibleCol)
                _viewPort.FirstVisibleColumn = col;

            UpdateVisibleCells();
            UpdateScrollThumbs();
            Invalidate();
        }

#if NET10_0_OR_GREATER
        private object? GetDefaultValueForColumn(IGridColumn column)
#else
        private object GetDefaultValueForColumn(IGridColumn column)
#endif
        {
            return column.GetDefaultValue();
        }

        private void ChangeGridState(GridInnerState newState)
        {
            if (_gridInnerState == GridInnerState.Editing)
            {
                CancelEditing();
            }
            _gridInnerState = newState;
            Invalidate();
        } 

        private void CancelEditing()
        {
            if (_currentEditor is null || !_currentEditor.Editor.Visible) return;

            _currentEditor.Close();
        }

#endregion

        #region Paint
        private void DrawBorder(Graphics g)
        {
            g.DrawRectangle(Pens.Black, 0, 0, this.Width - 1, this.Height - 1);
        }

        private void DrawMask(Graphics g)
        {
            g.FillRectangle(_maskBrush, this.ClientRectangle);
        }

        private void DrawScrollBars(Graphics g)
        {
            if (_needVertScroll)
            {
                g.SetClip(_layout.VertScrollRect);
                g.FillRectangle(Brushes.Gainsboro, _layout.VertScrollRect);

                // Используем ScrollBarData
                g.FillRectangle(_thumbBrush, _scrollBarData.VertThumb);

                g.ResetClip();
            }

            if (_needHorScroll)
            {
                g.SetClip(_layout.HorScrollRect);
                g.FillRectangle(Brushes.Gainsboro, _layout.HorScrollRect);

                // Используем ScrollBarData
                g.FillRectangle(_thumbBrush, _scrollBarData.HorThumb);

                g.ResetClip();
            }
        }

        private void DrawCells(Graphics g)
        {
            int startRow = _viewPort.FirstVisibleRow;
            int endRow = Math.Min(RowCount, startRow + _layout.VisibleRowCount);

            for (int row = startRow; row < endRow; row++)
            {
                int y = _layout.HeaderHeight + (row - startRow) * _layout.RowHeight;

                for (int col = _viewPort.FirstVisibleColumn; col <= LastVisibleColumn; col++)
                {
                    Rectangle cellRect = GetCellRect(row, col);

                    g.SetClip(cellRect);

                    var visual = _cellCache.Get(row, col);

                    bool isSelected = _selector.IsVisible &&
                                      (_selectionType == ADGSelectionTypes.Cell
                                        ? (_selector.Row == row && _selector.Column == col)
                                        : (_selector.Row == row));

                    bool isHovered =
                        !isSelected &&
                        _hoverSelector.IsVisible &&
                        _hoverSelector.Row == row &&
                        _hoverSelector.Column == col;

                    var ctx = new CellContext(
                        row,
                        col,
                        cellRect,
                        cellRect,
                        visual.Value,
                        visual.Text,
                        isSelected,
                        isHovered,
                        _defaultCellStyle
                    );

                    _columns[col].DrawCell(g, ctx);

                    g.ResetClip();
                }
            }
        }

        private void DrawVerticalGridLineAfterLastColumn(Graphics g)
        {
            int x = _layout.GridRect.X;

            for (int col = _viewPort.FirstVisibleColumn; col <= LastVisibleColumn; col++)
            {
                x += _layout.GetColumnWidth(col);
            }

            // ограничиваем линию GridRect'ом
            x = Math.Min(x, _layout.GridRect.Right);

            g.DrawLine(
                SystemPens.ControlDark,
                x - 1,
                _layout.GridRect.Y,
                x - 1,
                _layout.GridRect.Bottom
            );
        }

        private void DrawColumnHeaders(Graphics g)
        {
            for (int col = _viewPort.FirstVisibleColumn; col <= LastVisibleColumn; col++)
            {
                Rectangle rect = GetHeaderRect(col);

                IGridColumn column = _columns[col];

                if (rect.IsEmpty)
                    continue;
                HeaderContext ctx = new HeaderContext(
                    col,
                    rect,
                    column.HeaderText,
                    col == _hoveredHeaderCol,
                    col == _pressedHeaderCol,
                    _sortedColumnName == column.Name,
                    _sortedColumnName == column.Name ? _sortingDirection : ADGSortingDirection.None,
                    _defaultColHeaderStyle
                );

                GraphicsHelper.DrawHeader(g, ctx);
            }
        }

        private void DrawRowHeaders(Graphics g, int firstVisibleRow)
        {
            if (!IsRowHeaderVisible)
                return;

            for (int row = firstVisibleRow;
                 row < firstVisibleRow + _layout.VisibleRowCount && row < RowCount;
                 row++)
            {
                Rectangle r = GetRowHeaderCellRect(row, firstVisibleRow);

                bool selected = row == _selectedRow;
                bool hot = row == _hotRow;

                Brush back =
                    selected ? SystemBrushes.Highlight :
                    hot ? Brushes.LightGray :
                    SystemBrushes.Control;

                g.FillRectangle(back, r);

                ControlPaint.DrawBorder(g, r, SystemColors.ControlDark, ButtonBorderStyle.Solid);

                TextRenderer.DrawText(
                    g,
                    (row + 1).ToString(),
                    Font,
                    r,
                    selected ? SystemColors.HighlightText : SystemColors.ControlText,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }
        }

        #endregion

        #region EventHandlers
        private void AwesomeDataGrid_OnDataChanged()
        {
            this.RowCount = this.DataProvider.RowCount;
            RecalcRects();
            UpdateVisibleCells();
            Invalidate();
        }

        private void OnViewportChanged()
        {
            int firstRow = _viewPort.FirstVisibleRow;
            int lastRow = Math.Min(RowCount, firstRow + _layout.VisibleRowCount);

            _cellCache.UpdateViewport(firstRow, lastRow);
        }

        private void InlineEditor_OnEndEdit(IInlineEditor editor)
        {
            SetData(this.EditingCellAddress.Row, this.EditingCellAddress.Column, editor.Value);
            UpdateVisibleCells();
            SmartInvalidate(GetCellRect(this.EditingCellAddress.Row, this.EditingCellAddress.Column));
            this.EditingCellAddress = default;
        }

        private void InlineEditor_OnLostFocus()
        {
            ChangeGridState(GridInnerState.Default);
        }
        #endregion

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            ViewportChanged -= OnViewportChanged;
            try
            {
                // cant disposing brushes from SystemBrushes. etc
                _thumbBrush.Dispose();
                _maskBrush?.Dispose();
                _hoveredBorderPen?.Dispose();
                _selectedBorderPen?.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }

}