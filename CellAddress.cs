namespace YDs_AwesomeDataGrid
{
    internal readonly struct CellAddress : IEquatable<CellAddress>
    {
        public readonly int Row;
        public readonly int Col;

        public CellAddress(int row, int col)
        {
            this.Row = row;
            this.Col = col;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Col);
        }

        public bool Equals(CellAddress other)
        {
            return (this.Row == other.Row && this.Col == other.Col);
        }
    }
}