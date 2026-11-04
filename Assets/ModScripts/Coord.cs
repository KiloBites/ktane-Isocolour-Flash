using System;
using static CFMove;

public struct Coord : IEquatable<Coord>
{
    public int Row { get; private set; }
    public int Column { get; private set; }
    
    public Coord(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public override bool Equals(object obj) => obj is Coord && Equals((Coord)obj);

    public override int GetHashCode() => Row * 4 + Column;

    public override string ToString() => $"{"ABCD"[Column]}{Row + 1}";

    public bool Equals(Coord other) => Row == other.Row && Column == other.Column;
    
    public static bool operator ==(Coord a, Coord b) => a.Equals(b);
    public static bool operator !=(Coord a, Coord b) => !a.Equals(b);

    public void MakeMove(CFMove move)
    {
        switch (move)
        {
            case Left:
                Column = (Column - 1 + 4) % 4;
                break;
            case Right:
                Column = (Column + 1) % 4;
                break;
            case Up:
                Row = (Row - 1 + 4) % 4;
                break;
            case Down:
                Row = (Row + 1) % 4;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(move), move, null);
        }
    }
}
