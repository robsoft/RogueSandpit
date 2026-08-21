namespace RogueSandpit.Models;

public sealed class PlacedTrap
{
    public int X { get; }
    public int Y { get; }
    public int Damage { get; }

    public PlacedTrap(int x, int y, int damage)
    {
        X = x;
        Y = y;
        Damage = damage;
    }
}
