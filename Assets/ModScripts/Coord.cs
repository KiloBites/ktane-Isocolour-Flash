using System;
using static CFMove;

public struct Coord
{
    public int X { get; private set; }
    public int Y { get; private set; }
    
    // In this instance, X is the row, and Y is the column. Keep in mind the grid is fixed.

    public Coord(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString() => $"{"ABCD"[Y]}{X + 1}";

    public void MakeMove(CFMove move)
    {
        switch (move)
        {
            case Left:
                Y = (Y - 1 + 4) % 4;
                break;
            case Right:
                Y = (Y + 1) % 4;
                break;
            case Up:
                X = (X - 1 + 4) % 4;
                break;
            case Down:
                X = (X + 1) % 4;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(move), move, $"{move} is not a valid move");
        }
    }
}
