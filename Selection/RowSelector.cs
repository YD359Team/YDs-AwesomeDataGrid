namespace YDs_AwesomeDataGrid.Selection
{
    internal class RowSelector : ISelector
    {
        public bool IsVisible { get; set; }

        public int Row;

        public bool IsCellSelected(int row, int column)
            => false;

        public bool IsRowSelected(int row)
            => IsVisible && row == Row;
    }
}