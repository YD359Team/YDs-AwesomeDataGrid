using YDs_AwesomeDataGrid.InlineEditors;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace YDs_AwesomeDataGrid
{
    internal class InlineFloat32Editor : IInlineEditor
    {
        public event Action OnLostFocus;
        public event Action<IInlineEditor> OnEndEdit;

        public Control Grid { get; set; }
        public Type ColumnType => typeof(int);
        public Control Editor { get; private set; }
        public object Value { get; private set; }

        public InlineFloat32Editor()
        {
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
            this.Value = this.Editor.Text;
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
            ((NumericUpDown)this.Editor).Value = 0;
        }

#if NET10_0_OR_GREATER
        public void BeginEdit(Rectangle rect, object? cellValue)
#else
        public void BeginEdit(Rectangle rect, object cellValue)
#endif
        {
            Rectangle r = rect;
            r.Inflate(2, 2);
            this.Editor.Location = rect.Location;
            this.Editor.Size = rect.Size;
            this.Editor.Font = this.Grid.Font;
            if (cellValue != null)
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