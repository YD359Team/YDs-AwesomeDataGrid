namespace YDs_AwesomeDataGrid.Selection
{
    internal sealed class EmptySelector : ISelector
    {
        public bool IsVisible { get; set; } = false;
        public bool IsCellSelected(int row, int column) => false;
        public bool IsRowSelected(int row) => false;
    }
}