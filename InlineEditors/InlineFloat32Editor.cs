using YDs_AwesomeDataGrid.InlineEditors;

namespace YDs_AwesomeDataGrid
{
    internal class InlineFloat32Editor : IInlineEditor
    {
        public event Action OnLostFocus;
        public event Action<IInlineEditor> OnEndEdit;

        public Control Grid { get; }
        public Type ColumnType => typeof(int);
        public Control Editor { get; private set; }
        public object Value { get; private set; }

        public InlineFloat32Editor(Control grid)
        {
            this.Grid = grid;
            this.Editor = new NumericUpDown()
            {
                BorderStyle = BorderStyle.FixedSingle,
                DecimalPlaces = 2,
                Minimum = Convert.ToDecimal(decimal.MinValue),
                Maximum = Convert.ToDecimal(decimal.MaxValue),
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
            ((NumericUpDown)this.Editor).Value = 0;
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
                ((NumericUpDown)this.Editor).Value = Convert.ToDecimal((float)cellValue);
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