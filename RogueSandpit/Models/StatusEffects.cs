using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueSandpit.Models;

public sealed class TimedStatusEffect
{
    public StatusEffectType Type { get; }
    public int RemainingTurns { get; set; }
    public int Power { get; set; }
    public string Source { get; set; }

    public TimedStatusEffect(StatusEffectType type, int remainingTurns, int power, string source)
    {
        Type = type;
        RemainingTurns = remainingTurns;
        Power = power;
        Source = source;
    }
}

public readonly record struct StatusTurnResult(bool SkipAction, int BleedingDamage);

public sealed class StatusEffectCollection
{
    private readonly List<TimedStatusEffect> _effects = [];
    public IReadOnlyList<TimedStatusEffect> Effects => _effects;
    public bool Has(StatusEffectType type) => _effects.Any(effect => effect.Type == type);

    public void Clear() => _effects.Clear();
    internal void Restore(IEnumerable<TimedStatusEffect> effects)
    {
        _effects.Clear();
        _effects.AddRange(effects);
    }
    public bool Remove(StatusEffectType type) => _effects.RemoveAll(effect => effect.Type == type) > 0;

    public void Apply(StatusEffectType type, int duration, int power = 0, string source = "UNKNOWN")
    {
        if (duration <= 0) return;
        TimedStatusEffect existing = _effects.FirstOrDefault(effect => effect.Type == type);
        if (existing == null)
        {
            _effects.Add(new TimedStatusEffect(type, duration, power, source));
            return;
        }

        existing.RemainingTurns = Math.Max(existing.RemainingTurns, duration);
        existing.Power = Math.Max(existing.Power, power);
        existing.Source = source;
    }

    public StatusTurnResult AdvanceTurn()
    {
        bool skipAction = _effects.Any(effect => effect.Type == StatusEffectType.Stunned);
        int bleedingDamage = _effects
            .Where(effect => effect.Type == StatusEffectType.Bleeding)
            .Sum(effect => effect.Power);

        foreach (TimedStatusEffect effect in _effects) effect.RemainingTurns--;
        _effects.RemoveAll(effect => effect.RemainingTurns <= 0);
        return new StatusTurnResult(skipAction, bleedingDamage);
    }
}
