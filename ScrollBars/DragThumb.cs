using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace YDs_AwesomeDataGrid.ScrollBars
{
    internal class DragThumb
    {
        public bool IsDraggingVertThumb;
        public bool IsDraggingHorThumb;
        public Point DragStartMousePos;
        public int DragStartFirstVisibleRow;
        public int DragStartFirstVisibleCol;
    }
}
