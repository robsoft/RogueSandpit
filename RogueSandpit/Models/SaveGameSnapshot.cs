using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueSandpit.Models;

public sealed class SaveGameSnapshot
{
    public const int CurrentVersion = 1;
    public int Version { get; init; } = CurrentVersion;
    public int Seed { get; init; }
    public ulong RandomState { get; init; }
    public long TurnCount { get; init; }
    public GameOutcome Outcome { get; init; }
    public bool RealtimeMode { get; init; }
    public int InitiativeOffset { get; init; }
    public long NextTrailSequence { get; init; }
    public PlayerSnapshot Player { get; init; }
    public List<ItemSnapshot> Items { get; init; } = [];
    public List<NpcSaveSnapshot> Npcs { get; init; } = [];
    public List<CellSnapshot> Cells { get; init; } = [];
    public List<DoorSnapshot> Doors { get; init; } = [];
    public List<GroundItemSnapshot> GroundItems { get; init; } = [];
    public List<PlacedTrapSnapshot> Traps { get; init; } = [];
    public List<EnvironmentalEffectSnapshot> Effects { get; init; } = [];
    public List<TrailSnapshot> Trails { get; init; } = [];
    public List<string> EventLog { get; init; } = [];
    public RunStatisticsSnapshot Statistics { get; init; }

    public static SaveGameSnapshot Capture(GameState game, bool realtimeMode = false)
    {
        Map map = game.Map;
        IEnumerable<Item> allItems = game.Player.Inventory.Items
            .Concat(map.GroundItems.Select(item => item.Item))
            .Concat(map.NPCs.Where(npc => npc.HeldItem != null).Select(npc => npc.HeldItem));
        var cells = new List<CellSnapshot>(map.Width * map.Height);
        for (int y = 0; y < map.Height; y++)
        for (int x = 0; x < map.Width; x++)
        {
            MapCell cell = map.MapCells[x, y];
            cells.Add(new CellSnapshot(x, y, cell.CellType, cell.IsVisible,
                cell.IsDiscovered, cell.ParentElement?.HasVisited == true));
        }

        return new SaveGameSnapshot
        {
            Seed = map.Seed,
            RandomState = RandGen.CaptureState(),
            TurnCount = game.TurnCount,
            Outcome = game.Outcome,
            RealtimeMode = realtimeMode,
            InitiativeOffset = game.NextNpcInitiativeOffset,
            NextTrailSequence = map.NextTrailSequence,
            Player = PlayerSnapshot.From(game.Player),
            Items = allItems.DistinctBy(item => item.Id).Select(ItemSnapshot.From).ToList(),
            Npcs = map.NPCs.Select(npc => npc.CapturePersistence()).ToList(),
            Cells = cells,
            Doors = map.Doors.Select(door => new DoorSnapshot(door.X1, door.Y1, door.State)).ToList(),
            GroundItems = map.GroundItems.Select(item =>
                new GroundItemSnapshot(item.Item.Id, item.X, item.Y)).ToList(),
            Traps = map.PlacedTraps.Select(trap =>
                new PlacedTrapSnapshot(trap.X, trap.Y, trap.Damage, trap.Kind)).ToList(),
            Effects = map.EnvironmentalEffects.Select(effect => new EnvironmentalEffectSnapshot(
                effect.Type, effect.X, effect.Y, effect.RemainingTurns, effect.Power)).ToList(),
            Trails = map.PlayerTrail.Select(trail => new TrailSnapshot(trail.Sequence, trail.X,
                trail.Y, trail.NextX, trail.NextY, trail.RemainingTurns, trail.Strength,
                trail.IsAuthentic)).ToList(),
            EventLog = game.EventLog.Entries.ToList(),
            Statistics = RunStatisticsSnapshot.From(game.Statistics)
        };
    }

    public GameState Restore()
    {
        Validate();
        var map = new Map(Seed);
        var items = Items.ToDictionary(item => item.Id, item => item.ToModel());
        map.RestorePersistence(this, items);
        var player = new Player();
        player.Restore(Player, map, items);
        var game = new GameState(map, player);
        game.RestorePersistence(this);
        RandGen.RestoreState(Seed, RandomState);
        return game;
    }

    public void Validate()
    {
        if (Version != CurrentVersion) throw new InvalidOperationException($"Unsupported save version {Version}.");
        if (Player == null || Statistics == null || Items == null || Npcs == null || Cells == null
            || Doors == null || GroundItems == null || Traps == null || Effects == null
            || Trails == null || EventLog == null)
            throw new InvalidOperationException("Save is incomplete.");
        if (Cells.Count != 80 * 58) throw new InvalidOperationException("Save map dimensions are invalid.");
        if (Cells.Any(cell => cell == null || !InBounds(cell.X, cell.Y))
            || Cells.Select(cell => (cell.X, cell.Y)).Distinct().Count() != Cells.Count)
            throw new InvalidOperationException("Save map cells are invalid.");
        if (!InBounds(Player.X, Player.Y) || Player.InventoryItemIds == null || Player.StatusEffects == null)
            throw new InvalidOperationException("Saved player position is invalid.");
        if (Npcs.Any(npc => npc == null || !InBounds(npc.X, npc.Y)
                || npc.SearchTargets == null || npc.ObservedCasualties == null
                || npc.KnownHazards == null || npc.StatusEffects == null)
            || Npcs.Select(npc => npc.Id).Distinct().Count() != Npcs.Count)
            throw new InvalidOperationException("Saved NPC state is invalid.");
        if (Doors.Any(door => door == null || !InBounds(door.X, door.Y))
            || GroundItems.Any(item => item == null || !InBounds(item.X, item.Y))
            || Traps.Any(trap => trap == null || !InBounds(trap.X, trap.Y))
            || Effects.Any(effect => effect == null || !InBounds(effect.X, effect.Y))
            || Trails.Any(trail => trail == null || !InBounds(trail.X, trail.Y)
                || !InBounds(trail.NextX, trail.NextY)))
            throw new InvalidOperationException("Saved map contents are invalid.");
        if (Items.Select(item => item.Id).Distinct().Count() != Items.Count)
            throw new InvalidOperationException("Save contains duplicate item identifiers.");
        HashSet<Guid> itemIds = Items.Select(item => item.Id).ToHashSet();
        IEnumerable<Guid> referencedItems = Player.InventoryItemIds
            .Concat(GroundItems.Select(item => item.ItemId))
            .Concat(Npcs.Where(npc => npc.HeldItemId.HasValue).Select(npc => npc.HeldItemId!.Value));
        if (referencedItems.Any(id => !itemIds.Contains(id)))
            throw new InvalidOperationException("Save references a missing item.");
    }

    private static bool InBounds(int x, int y) => x is >= 0 and < 80 && y is >= 0 and < 58;
}

public sealed record PointSnapshot(int X, int Y)
{
    public (int X, int Y) ToTuple() => (X, Y);
    public static PointSnapshot From((int X, int Y)? point) =>
        point is { } value ? new PointSnapshot(value.X, value.Y) : null;
}

public sealed record ItemSnapshot(Guid Id, string Name, ItemType Type, int Power, int Tier, TrapKind? TrapKind)
{
    public static ItemSnapshot From(Item item) => new(item.Id, item.Name, item.Type, item.Power, item.Tier, item.TrapKind);
    public Item ToModel() => new(Name, Type, Power, TrapKind, Tier, Id);
}

public sealed record StatusEffectSnapshot(StatusEffectType Type, int RemainingTurns, int Power, string Source)
{
    public static StatusEffectSnapshot From(TimedStatusEffect effect) =>
        new(effect.Type, effect.RemainingTurns, effect.Power, effect.Source);
    public TimedStatusEffect ToModel() => new(Type, RemainingTurns, Power, Source);
}

public sealed class PlayerSnapshot
{
    public Guid Id { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int MaxHealth { get; init; }
    public int Health { get; init; }
    public int BaseDamage { get; init; }
    public bool Dead { get; init; }
    public bool HasSpecial { get; init; }
    public int InventoryCapacity { get; init; }
    public int SelectedInventoryIndex { get; init; }
    public List<Guid> InventoryItemIds { get; init; } = [];
    public Guid? EquippedWeaponId { get; init; }
    public Guid? EquippedArmorId { get; init; }
    public Guid? EquippedRangedWeaponId { get; init; }
    public List<StatusEffectSnapshot> StatusEffects { get; init; } = [];

    public static PlayerSnapshot From(Player player) => new()
    {
        Id = player.Id, X = player.X, Y = player.Y, MaxHealth = player.MaxHealth,
        Health = player.Health, BaseDamage = player.BaseDamage, Dead = player.Dead,
        HasSpecial = player.HasSpecial, InventoryCapacity = player.Inventory.Capacity,
        SelectedInventoryIndex = player.Inventory.SelectedIndex,
        InventoryItemIds = player.Inventory.Items.Select(item => item.Id).ToList(),
        EquippedWeaponId = player.EquippedWeapon?.Id, EquippedArmorId = player.EquippedArmor?.Id,
        EquippedRangedWeaponId = player.EquippedRangedWeapon?.Id,
        StatusEffects = player.StatusEffects.Effects.Select(StatusEffectSnapshot.From).ToList()
    };
}

public sealed class NpcSaveSnapshot
{
    public Guid Id { get; init; }
    public CharacterTypes CharacterType { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int HomeX { get; init; }
    public int HomeY { get; init; }
    public int HP { get; init; }
    public int MaxHP { get; init; }
    public int Damage { get; init; }
    public Direction Direction { get; init; }
    public NPCState State { get; init; }
    public CharacterMood Mood { get; init; }
    public int MoodValue { get; init; }
    public Visibility Visibility { get; init; }
    public NPCMoraleState MoraleState { get; init; }
    public PointSnapshot RetreatTarget { get; init; }
    public bool HasCalledForHelp { get; init; }
    public bool HasSeenPlayer { get; init; }
    public NPCAwareness Awareness { get; init; }
    public PointSnapshot LastKnownPlayerPosition { get; init; }
    public PointSnapshot InvestigationOrigin { get; init; }
    public NPCInvestigationSource InvestigationSource { get; init; }
    public int InvestigationConfidence { get; init; }
    public PointSnapshot LastObservedPlayerMovement { get; init; }
    public PointSnapshot PredictedInvestigationTarget { get; init; }
    public bool IsLocalSearching { get; init; }
    public long LastObservedTrailSequence { get; init; }
    public List<PointSnapshot> SearchTargets { get; init; } = [];
    public List<Guid> ObservedCasualties { get; init; } = [];
    public List<PointSnapshot> KnownHazards { get; init; } = [];
    public Guid? HeldItemId { get; init; }
    public List<StatusEffectSnapshot> StatusEffects { get; init; } = [];
}

public sealed record CellSnapshot(int X, int Y, MapCellType CellType, bool IsVisible,
    bool IsDiscovered, bool ParentVisited);
public sealed record DoorSnapshot(int X, int Y, DoorState State);
public sealed record GroundItemSnapshot(Guid ItemId, int X, int Y);
public sealed record PlacedTrapSnapshot(int X, int Y, int Damage, TrapKind Kind);
public sealed record EnvironmentalEffectSnapshot(EnvironmentalEffectType Type, int X, int Y,
    int RemainingTurns, int Power);
public sealed record TrailSnapshot(long Sequence, int X, int Y, int NextX, int NextY,
    int RemainingTurns, int Strength, bool IsAuthentic);

public sealed class RunStatisticsSnapshot
{
    public long Turns { get; init; }
    public long DeliberateTurns { get; init; }
    public long RealtimeTurns { get; init; }
    public long? ObjectiveCollectedTurn { get; init; }
    public long? EscapeTurn { get; init; }
    public Dictionary<CharacterTypes, int> DefeatsByArchetype { get; init; } = [];
    public int DamageDealt { get; init; }
    public int DamageReceived { get; init; }
    public int HealingReceived { get; init; }
    public int MeleeAttacks { get; init; }
    public int RangedShots { get; init; }
    public int RangedHits { get; init; }
    public int DetectionEpisodes { get; init; }
    public int NpcsAlerted { get; init; }
    public int MaximumPursuers { get; init; }
    public int ItemsCollected { get; init; }
    public int ItemsConsumed { get; init; }
    public int ItemsDropped { get; init; }
    public int ItemsThrown { get; init; }
    public int DoorsOpened { get; init; }
    public int DoorsClosed { get; init; }
    public int DoorsUnlocked { get; init; }
    public int TrapsPlaced { get; init; }
    public int TrapsTriggered { get; init; }
    public string DefeatCause { get; init; }

    public static RunStatisticsSnapshot From(RunStatistics statistics) => new()
    {
        Turns = statistics.Turns, DeliberateTurns = statistics.DeliberateTurns,
        RealtimeTurns = statistics.RealtimeTurns, ObjectiveCollectedTurn = statistics.ObjectiveCollectedTurn,
        EscapeTurn = statistics.EscapeTurn, DefeatsByArchetype = statistics.DefeatsByArchetype.ToDictionary(),
        DamageDealt = statistics.DamageDealt, DamageReceived = statistics.DamageReceived,
        HealingReceived = statistics.HealingReceived, MeleeAttacks = statistics.MeleeAttacks,
        RangedShots = statistics.RangedShots, RangedHits = statistics.RangedHits,
        DetectionEpisodes = statistics.DetectionEpisodes, NpcsAlerted = statistics.NpcsAlerted,
        MaximumPursuers = statistics.MaximumPursuers, ItemsCollected = statistics.ItemsCollected,
        ItemsConsumed = statistics.ItemsConsumed, ItemsDropped = statistics.ItemsDropped,
        ItemsThrown = statistics.ItemsThrown, DoorsOpened = statistics.DoorsOpened,
        DoorsClosed = statistics.DoorsClosed, DoorsUnlocked = statistics.DoorsUnlocked,
        TrapsPlaced = statistics.TrapsPlaced, TrapsTriggered = statistics.TrapsTriggered,
        DefeatCause = statistics.DefeatCause
    };
}
