namespace RogueSandpit.Models;

public enum EnvironmentalEffectType { Smoke, Fire }

public sealed class EnvironmentalEffect
{
    public EnvironmentalEffectType Type { get; }
    public int X { get; }
    public int Y { get; }
    public int RemainingTurns { get; set; }
    public int Power { get; }

    public EnvironmentalEffect(EnvironmentalEffectType type, int x, int y, int remainingTurns, int power = 0)
    {
        Type = type; X = x; Y = y; RemainingTurns = remainingTurns; Power = power;
    }
}
