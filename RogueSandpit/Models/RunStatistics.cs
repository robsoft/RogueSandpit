using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueSandpit.Models;

public sealed class RunStatistics
{
    private readonly Dictionary<CharacterTypes, int> _defeatsByArchetype =
        Enum.GetValues<CharacterTypes>().ToDictionary(type => type, _ => 0);

    public long Turns { get; private set; }
    public long DeliberateTurns { get; private set; }
    public long RealtimeTurns { get; private set; }
    public long? ObjectiveCollectedTurn { get; internal set; }
    public long? EscapeTurn { get; internal set; }
    public int NpcsDefeated => _defeatsByArchetype.Values.Sum();
    public IReadOnlyDictionary<CharacterTypes, int> DefeatsByArchetype => _defeatsByArchetype;
    public int DamageDealt { get; internal set; }
    public int DamageReceived { get; internal set; }
    public int HealingReceived { get; internal set; }
    public int MeleeAttacks { get; internal set; }
    public int RangedShots { get; internal set; }
    public int RangedHits { get; internal set; }
    public int DetectionEpisodes { get; internal set; }
    public int NpcsAlerted { get; internal set; }
    public int MaximumPursuers { get; internal set; }
    public int ItemsCollected { get; internal set; }
    public int ItemsConsumed { get; internal set; }
    public int ItemsDropped { get; internal set; }
    public int ItemsThrown { get; internal set; }
    public int DoorsOpened { get; internal set; }
    public int DoorsClosed { get; internal set; }
    public int DoorsUnlocked { get; internal set; }
    public int TrapsPlaced { get; internal set; }
    public int TrapsTriggered { get; internal set; }
    public string DefeatCause { get; internal set; } = "";

    internal void RecordTurn(bool automaticRealtime)
    {
        Turns++;
        if (automaticRealtime) RealtimeTurns++;
        else DeliberateTurns++;
    }

    internal void RecordDefeat(CharacterTypes type) => _defeatsByArchetype[type]++;

    internal void Restore(RunStatisticsSnapshot snapshot)
    {
        Turns = snapshot.Turns;
        DeliberateTurns = snapshot.DeliberateTurns;
        RealtimeTurns = snapshot.RealtimeTurns;
        ObjectiveCollectedTurn = snapshot.ObjectiveCollectedTurn;
        EscapeTurn = snapshot.EscapeTurn;
        foreach (CharacterTypes type in Enum.GetValues<CharacterTypes>())
            _defeatsByArchetype[type] = snapshot.DefeatsByArchetype.GetValueOrDefault(type);
        DamageDealt = snapshot.DamageDealt;
        DamageReceived = snapshot.DamageReceived;
        HealingReceived = snapshot.HealingReceived;
        MeleeAttacks = snapshot.MeleeAttacks;
        RangedShots = snapshot.RangedShots;
        RangedHits = snapshot.RangedHits;
        DetectionEpisodes = snapshot.DetectionEpisodes;
        NpcsAlerted = snapshot.NpcsAlerted;
        MaximumPursuers = snapshot.MaximumPursuers;
        ItemsCollected = snapshot.ItemsCollected;
        ItemsConsumed = snapshot.ItemsConsumed;
        ItemsDropped = snapshot.ItemsDropped;
        ItemsThrown = snapshot.ItemsThrown;
        DoorsOpened = snapshot.DoorsOpened;
        DoorsClosed = snapshot.DoorsClosed;
        DoorsUnlocked = snapshot.DoorsUnlocked;
        TrapsPlaced = snapshot.TrapsPlaced;
        TrapsTriggered = snapshot.TrapsTriggered;
        DefeatCause = snapshot.DefeatCause ?? "";
    }
}
