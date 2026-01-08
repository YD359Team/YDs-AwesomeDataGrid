using YDs_AwesomeDataGrid.InlineEditors;

namespace YDs_AwesomeDataGrid
{
    internal class InlineTextEditor : IInlineEditor
    {
        public event Action OnLostFocus;
        public event Action<IInlineEditor> OnEndEdit;

        public Control Grid { get; }
        public Type ColumnType => typeof(string);
        public Control Editor { get; private set; }
        public object Value { get; private set; }

        public InlineTextEditor(Control grid)
        {
            this.Grid = grid;
            this.Editor = new TextBox()
            {
                BorderStyle = BorderStyle.FixedSingle,
            };
            this.Editor.Leave += Editor_LostFocus;
            this.Editor.KeyDown += Editor_KeyDown;
        }

        private void Editor_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
                return;
            }
            else if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            this.Value = this.Editor.Text;
            HideEditor();
            OnEndEdit?.Invoke(this);
        }

        private void Editor_LostFocus(object? sender, EventArgs e)
        {
            HideEditor();
            OnLostFocus?.Invoke();
        }

        private void HideEditor()
        {
            this.Editor.Visible = false;
            this.Grid.Controls.Remove(this.Editor);
            ((TextBox)this.Editor).Clear();
        }

        public void BeginEdit(Rectangle rect, object? cellValue, object[]? enumValues = null)
        {
            Rectangle r = rect;
            r.Inflate(2, 2);
            this.Editor.Location = rect.Location;
            this.Editor.Size = rect.Size;
            this.Editor.Font = this.Grid.Font;
            if (cellValue is not null)
            {
                ((TextBox)this.Editor).Text = cellValue.ToString();
            }
            this.Grid.Controls.Add(this.Editor);
            this.Editor.Visible = true;
            this.Editor.Focus();
        }

        public void Close()
        {
            HideEditor();
        }
    }
}