namespace RogueSandpit.Models;

public sealed class PlayerTrailClue
{
    public long Sequence { get; }
    public int X { get; }
    public int Y { get; }
    public int NextX { get; }
    public int NextY { get; }
    public int RemainingTurns { get; set; }
    public int Strength { get; }
    public bool IsAuthentic { get; }

    public PlayerTrailClue(long sequence, int x, int y, int nextX, int nextY,
        int remainingTurns, int strength = 1, bool isAuthentic = true)
    {
        Sequence = sequence;
        X = x;
        Y = y;
        NextX = nextX;
        NextY = nextY;
        RemainingTurns = remainingTurns;
        Strength = strength;
        IsAuthentic = isAuthentic;
    }
}
