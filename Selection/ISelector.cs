namespace YDs_AwesomeDataGrid.Selection
{
    internal interface ISelector
    {
        bool IsVisible { get; set; }
        bool IsCellSelected(int row, int column);
        bool IsRowSelected(int row);
    }
}