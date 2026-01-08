namespace YDs_AwesomeDataGrid.InlineEditors
{
    public interface IInlineEditor
    {
        event Action OnLostFocus;
        event Action<IInlineEditor> OnEndEdit;

        Control Grid { get; }
        Type ColumnType { get; }
        Control Editor { get; }
        object Value { get; }

        void BeginEdit(Rectangle rect, object? cellValue, object[]? enumValues = null);
        void Close();
    }
}