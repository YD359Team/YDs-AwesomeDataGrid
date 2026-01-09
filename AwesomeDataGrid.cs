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

namespace YDs_AwesomeDataGrid
{
    public class AwesomeDataGrid : ExtendedControl
    {
        #region Event
        private event Action ViewportChanged;
        #endregion

        #region PublicProperties

        #region StylesOverrides
        private Font _font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
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
                // clear enum cache
                _enumValues.Clear();
                if (value.GetType() != typeof(EmptyDataProvider))
                {
                    if (_dataProvider != null)
                    {
                        _dataProvider.OnDataChanged -= AwesomeDataGrid_OnDataChanged;
                    }
                    _dataProvider = value ?? EmptyProvider;
                    LoadData();
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
        private int VisibleRowCount => _visibleRowCount;
        private int LastVisibleColumn
        {
            get
            {
                int viewportWidth = _layout.GridRect.Width;
                int firstCol = _viewPort.FirstVisibleColumn;
                int visibleCols = (viewportWidth + _layout.ColumnWidth - 1) / _layout.ColumnWidth;

                return Math.Min(ColumnCount - 1, firstCol + visibleCols - 1);
            }
        }

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
        private int _visibleRowCount;
        private bool _needVertScroll;
        private bool _needHorScroll;
        private bool _isScrollVertHovered;
        private bool _isScrollHorHovered;
        private HoverStates _hoverState;

        // состояние
        private int _hotRow = -1;
        private int _selectedRow = -1;

        private AwesomeDataGridColumn[] _columns = Array.Empty<AwesomeDataGridColumn>();

        #region InlineEditors
        private readonly Dictionary<Type, IInlineEditor> _editors = new Dictionary<Type, IInlineEditor>();
        private IInlineEditor _currentEditor;
        private readonly Dictionary<Type, object[]> _enumValues = new Dictionary<Type, object[]>();
        #endregion

        #region VisibleCellCache
        private readonly CellVisualCache _cellCache = new CellVisualCache();
        private int _cachedFirstRow = -1;
        private int _cachedLastRow = -1;
        #endregion

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
        private readonly Brush _defaultTextBrush = Brushes.Black;
        private readonly Brush _highlightTextBrush = SystemBrushes.HighlightText;
        private readonly Brush _highlightBackgroundBrush = new SolidBrush(Color.FromArgb(128, SystemColors.Highlight));
        private readonly Brush _maskBrush = new SolidBrush(Color.FromArgb(100, Color.DarkGray));
        private readonly SolidBrush _thumbBrush = new SolidBrush(SystemColors.ControlDarkDark);
        private readonly Pen _selectedBorderPen = new Pen(SystemColors.HighlightText, 1f);
        private readonly Pen _hoveredBorderPen = new Pen(Color.DeepSkyBlue, 1f);
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
            InitEditors();
            //
            RecalcRects();
            ViewportChanged += OnViewportChanged; 
        }

        private void InitEditors()
        {
            var textEditor = new InlineTextEditor(this);
            var dateTimeEditor = new InlineDateTimeEditor(this);
            var enumEditor = new InlineEnumEditor(this);
            var int32Editor = new InlineInt32Editor(this);
            var float32Editor = new InlineFloat32Editor(this);
            _editors[typeof(string)] = textEditor;
            _editors[typeof(DateTime)] = dateTimeEditor;
            _editors[typeof(Enum)] = enumEditor;
            _editors[typeof(int)] = int32Editor;
            _editors[typeof(float)] = float32Editor;
            textEditor.OnLostFocus += InlineEditor_OnLostFocus;
            textEditor.OnEndEdit += InlineEditor_OnEndEdit;
            dateTimeEditor.OnLostFocus += InlineEditor_OnLostFocus;
            dateTimeEditor.OnEndEdit += InlineEditor_OnEndEdit;
            enumEditor.OnLostFocus += InlineEditor_OnLostFocus;
            enumEditor.OnEndEdit += InlineEditor_OnEndEdit;
            int32Editor.OnLostFocus += InlineEditor_OnLostFocus;
            int32Editor.OnEndEdit += InlineEditor_OnEndEdit;
            float32Editor.OnLostFocus += InlineEditor_OnLostFocus;
            float32Editor.OnEndEdit += InlineEditor_OnEndEdit;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillRectangle(Brushes.White, this.ClientRectangle);

            if (DataProvider == EmptyProvider)
                return;

            DrawRowHeaders(e.Graphics, _viewPort.FirstVisibleRow);
            DrawColumnHeaders(e.Graphics);
            DrawCells(e.Graphics);
            DrawScrollBars(e.Graphics);
            if (_gridInnerState == GridInnerState.Editing)
            {
                DrawMask(e.Graphics);
            }
            DrawBorder(e.Graphics);
        }

        public bool TryHitRowHeader(Point p, int firstVisibleRow, out int row)
        {
            row = -1;

            if (!IsRowHeaderVisible)
                return false;

            if (!_layout.RowHeaderRect.Contains(p))
                return false;

            int index = (p.Y - _layout.RowHeaderRect.Y) / _layout.RowHeight;
            row = firstVisibleRow + index;

            return row >= 0 && row < RowCount;
        }

        public Rectangle GetRowHeaderCellRect(int row, int firstVisibleRow)
        {
            if (row < firstVisibleRow || row >= firstVisibleRow + VisibleRowCount)
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

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    return true;
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
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
                if (_columns[col].IsReadOnly) return;

                int row = _selector.Row;
                RequestCellEditing(row, col);
            }
            // Del
            else if (e.KeyCode == Keys.Delete)
            {
                if (!_selector.IsVisible) return;

                int col = _selector.Column;
                if (col < 0 || col >= ColumnCount) return;
                if (_columns[col].IsReadOnly) return;

                int row = _selector.Row;
                this.DataProvider.SetData(row, col, GetDefaultValueForType(_columns[col].DataType));
                InvalidateCellCache(row, col);
                SmartInvalidate(GetCellRect(row, col));
            }
            // Ctrl+C
            else if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
            {
                if (!_selector.IsVisible) return;
                int row = _selector.Row;
                int col = _selector.Column;
                Clipboard.SetText(GetCellPres(this.DataProvider.GetData(row, col), _columns[col].DataType));

            }
            // Ctrl+V
            else if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
            {
                if (!_selector.IsVisible) return;
                int row = _selector.Row;
                int col = _selector.Column;
                if (_columns[col].IsReadOnly) return;
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

            if (_isDraggingVertThumb)
            {
                int dy = e.Y - _dragStartMousePos.Y;

                int scrollRange = _layout.VertScrollRect.Height - _scrollBarData.VertThumb.Height;
                if (scrollRange <= 0) return;

                float ratio = dy / (float)scrollRange;
                int maxFirstRow = Math.Max(0, RowCount - VisibleRowCount);

                _viewPort.FirstVisibleRow = _dragStartFirstVisibleRow + (int)(ratio * maxFirstRow);
#if NET10_0_OR_GREATER
                _viewPort.FirstVisibleRow = Math.Clamp(_viewPort.FirstVisibleRow, 0, maxFirstRow);
#else
                _viewPort.FirstVisibleRow = MathHelper.Clamp(_viewPort.FirstVisibleRow, 0, maxFirstRow);
#endif

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
#if NET10_0_OR_GREATER
                _viewPort.FirstVisibleColumn = Math.Clamp(_viewPort.FirstVisibleColumn, 0, maxFirstCol);
#else
                _viewPort.FirstVisibleColumn = MathHelper.Clamp(_viewPort.FirstVisibleColumn, 0, maxFirstCol);
#endif

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

            if (_layout.GridRect.IntersectsWith(mouseRect) && TryGetCellByPoint(e.Location, out int row, out int col))
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

            Rectangle mouseRect = new Rectangle(e.Location, new Size(1, 1));
            bool isInGrid = _layout.GridRect.IntersectsWith(mouseRect);

            Rectangle vertThumb = _scrollBarData.VertThumb;
            Rectangle horThumb = _scrollBarData.HorThumb;

            if (vertThumb.Contains(e.Location))
            {
                _isDraggingVertThumb = true;
                _dragStartMousePos = e.Location;
                _dragStartFirstVisibleRow = _viewPort.FirstVisibleRow;
                Capture = true; // чтобы получать события мыши вне контрола
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
                AwesomeDataGridColumn column = _columns[headerCol];

                if (!column.AllowSort) return;

                if (_columns.Any(c => c != column && c.SortingDirection != ADGSortingDirection.None))
                {
                    foreach (var c in _columns)
                        if (c != column)
                            c.SortingDirection = ADGSortingDirection.None;
                }

                ADGSortingDirection currentSortingDir = _sortingDirection;
                if (currentSortingDir == ADGSortingDirection.None)
                    currentSortingDir = ADGSortingDirection.Ascending;
                else if (currentSortingDir == ADGSortingDirection.Ascending)
                    currentSortingDir = ADGSortingDirection.Descending;
                else if (currentSortingDir == ADGSortingDirection.Descending)
                    currentSortingDir = ADGSortingDirection.Ascending;
                else
                    currentSortingDir = ADGSortingDirection.None;

                _sortedColumnName = column.DataPropertyName;
                _sortingDirection = currentSortingDir;

                this.DataProvider.SortColumn(_sortedColumnName, _sortingDirection);

                ViewportChanged?.Invoke();
                _cellCache.InvalidateAll();
                Invalidate();
            }
            // find cell by point
            else if (isInGrid && TryGetCellByPoint(e.Location, out int row, out int col))
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
            _isDraggingVertThumb = false;
            _isDraggingHorThumb = false;
            Capture = false;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (_layout.GridRect.Contains(e.Location) && TryGetCellByPoint(e.Location, out int row, out int col))
            {
                if (col >= _columns.Length) return;
                if (_columns[col].IsReadOnly) return;

                RequestCellEditing(row, col);
            }
        }

        private void RequestCellEditing(int row, int col)
        {
            if (_columns[col].DataType == typeof(bool))
            {
                SetData(row, col, !(bool)GetData(row, col));
                _cellCache.Invalidate(row, col);
                Invalidate(GetCellRect(_selector.Row, _selector.Column));
                return;
            }

            ChangeGridState(GridInnerState.Editing);

            // show editor with stored values for enum
            if (_columns[col].DataType.IsEnum)
            {
                this.EditingCellAddress = new CellKey(row, col);
                var enumVals = _enumValues[_columns[col].DataType];
                _currentEditor = _editors[typeof(Enum)];
                _currentEditor.BeginEdit(GetCellRect(row, col),
                    GetData(this.EditingCellAddress.Row, this.EditingCellAddress.Column),
                    enumVals);
            }
            // show typed editor for other types
            else if (_editors.TryGetValue(_columns[col].DataType, out var editor))
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
                    Math.Max(0, RowCount - VisibleRowCount)
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
            var columnsDescription = this.DataProvider.GetColumnsDescription().ToArray();
            _columns = columnsDescription
                .Select(cd => new AwesomeDataGridColumn(cd))
                .ToArray();

            // get enum values
            foreach (var colDesc in columnsDescription)
            {
                if (colDesc.DataType.IsEnum && !_enumValues.ContainsKey(colDesc.DataType))
                {
                    var values = Enum.GetValues(colDesc.DataType).Cast<object>().ToArray();
                    _enumValues[colDesc.DataType] = values;
                }
            }

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

            _visibleRowCount = _layout.VisibleRowCount;
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
            int endRow = Math.Min(RowCount, startRow + VisibleRowCount);

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
                    _cellCache.Get(row, col, () =>
                    {
                        var value = DataProvider.GetData(row, col);
                        return new CellVisual
                        {
                            Text = GetCellPres(value, _columns[col].DataType),
                            Style = ResolveCellStyle(row, col, value)
                        };
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
            // простая логика, можно расширить
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
            else if (_selector.Row >= _viewPort.FirstVisibleRow + VisibleRowCount)
            {
                _viewPort.FirstVisibleRow = _selector.Row - VisibleRowCount + 1;
                updated = true;
            }

            if (_selector.Column < _viewPort.FirstVisibleColumn)
            {
                _viewPort.FirstVisibleColumn = _selector.Column;
                updated = true;
            }
            else if (_selector.Column > LastVisibleColumn)
            {
                _viewPort.FirstVisibleColumn = _selector.Column - (_layout.GridRect.Width / _layout.ColumnWidth) + 1;
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
            int lastRow = firstRow + VisibleRowCount - 1;

            if (row < firstRow)
                _viewPort.FirstVisibleRow = row;
            else if (row > lastRow)
                _viewPort.FirstVisibleRow = row - VisibleRowCount + 1;

            int firstCol = _viewPort.FirstVisibleColumn;
            int visibleCols = (_layout.GridRect.Width + _layout.ColumnWidth - 1) / _layout.ColumnWidth;
            int lastCol = firstCol + visibleCols - 1;

            if (col < firstCol)
                _viewPort.FirstVisibleColumn = col;
            else if (col > lastCol)
                _viewPort.FirstVisibleColumn = col - visibleCols + 1;

            UpdateVisibleCells();
            UpdateScrollThumbs();
            Invalidate();
        }

        private string GetCellPres(object value, Type dataType)
        {
            if (value == null) return string.Empty;
            if (dataType == typeof(float)) return ((float)value).ToString("F2");
            if (dataType == typeof(double)) return ((double)value).ToString("F2");
            if (dataType == typeof(decimal)) return ((decimal)value).ToString("F2");
            if (dataType == typeof(DateTime)) return ((DateTime)value).ToShortDateString();
            return value?.ToString() ?? string.Empty;
        }

#if NET10_0_OR_GREATER
        private object? GetDefaultValueForType(Type type)
#else
        private object GetDefaultValueForType(Type type)
#endif
        {
            if (type == typeof(string)) return string.Empty;
            if (type == typeof(int)) return 0;
            if (type == typeof(float)) return 0f;
            if (type == typeof(double)) return 0.0;
            if (type == typeof(decimal)) return 0.0m;
            if (type == typeof(bool)) return false;
            if (type == typeof(DateTime)) return default(DateTime);
            if (type.IsEnum) return _enumValues[type].First();
            return null;
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
            int endRow = Math.Min(RowCount, startRow + this.VisibleRowCount);

            for (int row = startRow; row < endRow; row++)
            {
                int y = _layout.HeaderHeight + (row - startRow) * _layout.RowHeight;

                for (int col = _viewPort.FirstVisibleColumn; col <= LastVisibleColumn; col++)
                {
                    Rectangle cellRect = GetCellRect(row, col);

                    g.SetClip(cellRect);

                    var visual = _cellCache.Get(row, col, () =>
                    {
                        var value = DataProvider.GetData(row, col);
                        return new CellVisual
                        {
                            Text = GetCellPres(value, _columns[col].DataType),
                            Style = ResolveCellStyle(row, col, value)
                        };
                    });

                    string pres = visual.Text;

                    bool isSelected = _selector.IsVisible &&
                                      (_selectionType == ADGSelectionTypes.Cell
                                        ? (_selector.Row == row && _selector.Column == col)
                                        : (_selector.Row == row));

                    bool isHovered =
                        !isSelected &&
                        _hoverSelector.IsVisible &&
                        _hoverSelector.Row == row &&
                        _hoverSelector.Column == col;

                    Rectangle r = Rectangle.Inflate(cellRect, -1, -1);

                    if (isSelected)
                    {
                        Rectangle selectionRect = _selectionType == ADGSelectionTypes.FullRow && _isRowHeaderVisible
                            ? new Rectangle(0, cellRect.Y, _layout.GridRect.Width, cellRect.Height)
                            : cellRect;

                        g.FillRectangle(_columns[col].DataType == typeof(bool) ? _highlightBackgroundBrush : SystemBrushes.Highlight, selectionRect);
                        g.DrawRectangle(_selectedBorderPen, Rectangle.Inflate(cellRect, -1, -1));
                    }
                    else if (isHovered)
                    {
                        g.DrawRectangle(_hoveredBorderPen, r);
                    }
                    else
                    {
                        g.DrawRectangle(Pens.Black, r);
                    }

                    if (_columns[col].DataType == typeof(bool))
                    {
                        _ = bool.TryParse(pres, out bool b);
                        var state = b
                           ? (isSelected ? CheckBoxState.CheckedHot : CheckBoxState.CheckedNormal)
                           : (isSelected ? CheckBoxState.UncheckedHot : CheckBoxState.UncheckedNormal);

                        CheckBoxRenderer.DrawCheckBox(g,
                            new Point(cellRect.X + (cellRect.Width - 14) / 2, cellRect.Y + (cellRect.Height - 14) / 2),
                            state);
                    }
                    else
                    {
                        GraphicsHelper.DrawString(g, pres, this.Font, isSelected ? _highlightTextBrush : _defaultTextBrush, cellRect);
                    }

                    g.ResetClip();
                }
            }
        }

        private void DrawColumnHeaders(Graphics g)
        {
            for (int col = _viewPort.FirstVisibleColumn; col <= LastVisibleColumn; col++)
            {
                int x = _layout.GridRect.X + (col - _viewPort.FirstVisibleColumn) * _layout.ColumnWidth;
                Rectangle rect = new Rectangle(x, 0, _layout.ColumnWidth, _layout.RowHeight);

                g.FillRectangle(Brushes.LightGray, rect);
                g.DrawRectangle(Pens.Black, rect);

                var column = _columns[col];

                bool isSorted = column.DataPropertyName == _sortedColumnName;
                var dir = isSorted ? _sortingDirection : ADGSortingDirection.None;

                string headerText = column.HeaderText;

                if (dir == ADGSortingDirection.Ascending)
                    headerText += " ▲";
                else if (dir == ADGSortingDirection.Descending)
                    headerText += " ▼";

                GraphicsHelper.DrawString(g, headerText, this.Font, Brushes.Black, rect);
            }
        }

        private void DrawRowHeaders(Graphics g, int firstVisibleRow)
        {
            if (!IsRowHeaderVisible)
                return;

            for (int row = firstVisibleRow;
                 row < firstVisibleRow + VisibleRowCount && row < RowCount;
                 row++)
            {
                Rectangle r = GetRowHeaderCellRect(row, firstVisibleRow);

                bool selected = row == _selectedRow;
                bool hot = row == _hotRow;

                Color back =
                    selected ? SystemColors.Highlight :
                    hot ? Color.LightGray :
                    SystemColors.Control;

                using (var b = new SolidBrush(back))
                {
                    g.FillRectangle(b, r);
                }

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

        private void MoveVerticalThumb(int mouseY)
        {
            Rectangle track = _layout.VertScrollRect;
            Rectangle thumb = _scrollBarData.VertThumb;

            int y = mouseY - _scrollBarData.DragOffset;
#if NET10_0_OR_GREATER
            y = Math.Clamp(y, track.Top, track.Bottom - thumb.Height);
#else
            y = MathHelper.Clamp(y, track.Top, track.Bottom - thumb.Height);
#endif

            int scrollRange = track.Height - thumb.Height;
            if (scrollRange <= 0)
                return;

            float t = (float)(y - track.Top) / scrollRange;

            int maxFirstRow = Math.Max(0, RowCount - _layout.VisibleRowCount);
            _viewPort.FirstVisibleRow = (int)(t * maxFirstRow);
        }

        private void MoveHorizontalThumb(int mouseX)
        {
            Rectangle track = _layout.HorScrollRect;
            Rectangle thumb = _scrollBarData.HorThumb;

            int x = mouseX - _scrollBarData.DragOffset;
#if NET10_0_OR_GREATER
            x = Math.Clamp(x, track.Left, track.Right - thumb.Width);
#else
            x = MathHelper.Clamp(x, track.Left, track.Right - thumb.Width);
#endif
            int scrollRange = track.Width - thumb.Width;
            if (scrollRange <= 0)
                return;

            float t = (float)(x - track.Left) / scrollRange;

            int maxFirstCol = Math.Max(0, ColumnCount - _layout.VisibleColumnCount(_viewPort.FirstVisibleColumn));
            _viewPort.FirstVisibleColumn = (int)(t * maxFirstCol);
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
            int lastRow = Math.Min(RowCount, firstRow + VisibleRowCount);

            _cellCache.UpdateViewport(firstRow, lastRow);
        }

        private void InlineEditor_OnEndEdit(IInlineEditor editor)
        {
            SetData(this.EditingCellAddress.Row, this.EditingCellAddress.Column, editor.Value);
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
                //_highlightBackgroundBrush?.Dispose();
                //_highlightTextBrush?.Dispose();
                //_defaultTextBrush?.Dispose();
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