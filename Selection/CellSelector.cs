namespace YDs_AwesomeDataGrid.Selection
{
    internal class CellSelector : ISelector
    {
        public bool IsVisible { get; set; }

        public int Row;
        public int Column;

        public bool IsCellSelected(int row, int column)
            => IsVisible && row == Row && column == Column;

        public bool IsRowSelected(int row)
            => false;
    }
}