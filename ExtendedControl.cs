using System;
using System.Drawing;
using System.Windows.Forms;

namespace YDs_AwesomeDataGrid
{
    public class ExtendedControl : Control
    {
        private DateTime _lastClick;
        private bool _inDoubleClick;
        private Rectangle _doubleClickArea;
        private TimeSpan _doubleClickMaxTime;
        private System.Windows.Forms.Timer _clickTimer;

        public ExtendedControl()
        {
            _doubleClickMaxTime = TimeSpan.FromMilliseconds(SystemInformation.DoubleClickTime);

            _clickTimer = new Timer();
            if (!DesignMode)
            {
                _clickTimer.Interval = SystemInformation.DoubleClickTime;
                _clickTimer.Tick += ClickTimer_Tick;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    return true;
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        private void ExtendedControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (_inDoubleClick)
            {
                _inDoubleClick = false;

                TimeSpan length = DateTime.Now - _lastClick;

                // If double click is valid, respond
                if (_doubleClickArea.Contains(e.Location) && length < _doubleClickMaxTime)
                {
                    _clickTimer.Stop();
                    OnDoubleClick(this, e);
                }

                return;
            }

            // Double click was invalid, restart 
            _clickTimer.Stop();
            _clickTimer.Start();
            _lastClick = DateTime.Now;
            _inDoubleClick = true;
#if NET10_0_OR_GREATER
            _doubleClickArea = new Rectangle(e.Location - (SystemInformation.DoubleClickSize / 2),
#else
            Size doubleClickSize = SystemInformation.DoubleClickSize;
            _doubleClickArea = new Rectangle(e.Location - (new Size(doubleClickSize.Width / 2, doubleClickSize.Height / 2)),
#endif
                SystemInformation.DoubleClickSize);
        }

        private void ClickTimer_Tick(object sender, EventArgs e)
        {
            _inDoubleClick = false;
            _clickTimer.Stop();
        }

        protected virtual void OnDoubleClick(object sender, MouseEventArgs e)
        {

        }
    }
}