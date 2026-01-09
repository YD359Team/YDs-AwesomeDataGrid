namespace YDs_AwesomeDataGrid
{
#if NET10_0_OR_GREATER
    readonly record struct CellKey(int Row, int Column);
#else
    readonly struct CellKey 
    {
        public readonly int Row;
        public readonly int Column;

        public CellKey(int row, int column)
	    {
            this.Row = row;
            this.Column = column;
	    }
    }
#endif
}