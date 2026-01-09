
using System.Drawing;

internal class ScrollBarData
{
    public int OffsetX;
    public int OffsetY;

    public Rectangle VertThumb { get; internal set; }
    public Rectangle HorThumb { get; internal set; }

    public bool IsDraggingVert;
    public bool IsDraggingHor;

    public int DragOffset;
}
