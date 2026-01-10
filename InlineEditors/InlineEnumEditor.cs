using YDs_AwesomeDataGrid.InlineEditors;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace YDs_AwesomeDataGrid
{
    internal class InlineEnumEditor : IInlineEditor
    {
        public event Action OnLostFocus;
        public event Action<IInlineEditor> OnEndEdit;

        public Control Grid { get; set; }
        public Type ColumnType => typeof(string);
        public Control Editor { get; private set; }
        public object Value { get; private set; }

        public InlineEnumEditor()
        {
            this.Editor = new ComboBox()
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
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
            this.Value = ((ComboBox)this.Editor).SelectedItem;
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
            ((ComboBox)this.Editor).Items.Clear();
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
            ((ComboBox)this.Editor).Items.AddRange(enumValues);
            if (cellValue != null)
            {
                ((ComboBox)this.Editor).SelectedItem = cellValue;
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