using YDs_AwesomeDataGrid.InlineEditors;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace YDs_AwesomeDataGrid
{
    internal class InlineDateTimeEditor : IInlineEditor
    {
        public event Action OnLostFocus;
        public event Action<IInlineEditor> OnEndEdit;

        private static readonly DateTime FallbackValue = new DateTime(2000, 1, 1);

        public Control Grid { get; set; }
        public Type ColumnType => typeof(string);
        public Control Editor { get; private set; }
        public object Value { get; private set; }

        public InlineDateTimeEditor()
        {
            this.Editor = new DateTimePicker()
            {
                Value = FallbackValue
            };
            this.Editor.Leave += Editor_LostFocus;
            this.Editor.KeyDown += Editor_KeyDown;
        }

#if NET10_0_OR_GREATER
        private void Editor_KeyDown(object? sender, KeyEventArgs e)
#else
        private void Editor_KeyDown(object sender, KeyEventArgs e)
#endif
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
            this.Value = ((DateTimePicker)this.Editor).Value;
            HideEditor();
            OnEndEdit?.Invoke(this);
        }

#if NET10_0_OR_GREATER
        private void Editor_LostFocus(object? sender, EventArgs e)
#else
        private void Editor_LostFocus(object sender, EventArgs e)
#endif
        {
            HideEditor();
            OnLostFocus?.Invoke();
        }

        private void HideEditor()
        {
            this.Editor.Visible = false; 
            this.Grid.Controls.Remove(this.Editor);
            ((DateTimePicker)this.Editor).Value = FallbackValue;
        }

#if NET10_0_OR_GREATER
        public void BeginEdit(Rectangle rect, object? cellValue, object[]? enumValues = null)
#else
        public void BeginEdit(Rectangle rect, object cellValue, object[] enumValues = null)
#endif
        {
            Rectangle r = rect;
            r.Inflate(2, 2);
            this.Editor.Location = rect.Location;
            this.Editor.Size = rect.Size;
            this.Editor.Font = this.Grid.Font;
            if (cellValue != null)
            {
                ((DateTimePicker)this.Editor).Value = (DateTime)cellValue;
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