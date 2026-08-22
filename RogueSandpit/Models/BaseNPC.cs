using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueSandpit.Models;


public abstract class BaseNPC
{
    private readonly Queue<(int X, int Y)> _searchTargets = new();
    private bool _isLocalSearching;
    private long _lastObservedTrailSequence;
    private readonly HashSet<Guid> _observedCasualties = new();
    private readonly HashSet<(int X, int Y)> _knownHazards = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public BaseContainingElement CurrentRoom { get; private set; }
    public string Description { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public CharacterTypes CharacterType { get; protected set; }
    public Map Map { get; private set; }

    public Direction Direction { get; set; } = Direction.Up;
    public int X { get; set; }
    public int Y { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; protected set; }
    public int Damage { get; set; }
    public Item HeldItem { get; set; }
    public NPCAwarenessProfile AwarenessProfile { get; protected set; }
    public NPCMoraleProfile MoraleProfile { get; protected set; }
    public NPCRangedProfile RangedProfile { get; protected set; }
    public NPCMoraleState MoraleState { get; protected set; } = NPCMoraleState.Steady;
    public (int X, int Y)? RetreatTarget { get; private set; }
    public bool HasCalledForHelp { get; private set; }
    public StatusEffectCollection StatusEffects { get; } = new();
    public int EffectiveDamage => Damage + (MoraleState == NPCMoraleState.Enraged
        ? MoraleProfile.EnrageDamageBonus : 0);

    public int AssetID { get; set; }
    public int AnimFrame { get; set; }

    public NPCState State { get; set; } = NPCState.InActive;
    public CharacterMood Mood { get; set; } = CharacterMood.Neutral;
    public int MoodValue { get; set; } = 0;

    public Visibility Visibility { get; set; } = Visibility.Hidden;
    public bool HasSeenPlayer { get; private set; } = false;
    public NPCAwareness Awareness { get; private set; } = NPCAwareness.Unaware;
    public (int X, int Y)? LastKnownPlayerPosition { get; private set; }
    public (int X, int Y)? InvestigationOrigin { get; private set; }
    public NPCInvestigationSource InvestigationSource { get; private set; } = NPCInvestigationSource.None;
    public int InvestigationConfidence { get; private set; }
    public (int X, int Y)? LastObservedPlayerMovement { get; private set; }
    public (int X, int Y)? PredictedInvestigationTarget { get; private set; }
    public (int X, int Y)? InvestigationTarget =>
        _searchTargets.Count > 0 ? _searchTargets.Peek() : InvestigationOrigin;
    public bool IsPursuingPlayer => Awareness == NPCAwareness.Pursuing;
    public IReadOnlyCollection<(int X, int Y)> KnownHazards => _knownHazards;
    public int ObservedCasualtyCount => _observedCasualties.Count;

    internal NpcSaveSnapshot CapturePersistence() => new()
    {
        Id = Id, CharacterType = CharacterType, Name = Name, Description = Description,
        X = X, Y = Y, HomeX = HomeX, HomeY = HomeY, HP = HP, MaxHP = MaxHP,
        Damage = Damage, Direction = Direction, State = State, Mood = Mood, MoodValue = MoodValue,
        Visibility = Visibility, MoraleState = MoraleState, RetreatTarget = PointSnapshot.From(RetreatTarget),
        HasCalledForHelp = HasCalledForHelp, HasSeenPlayer = HasSeenPlayer, Awareness = Awareness,
        LastKnownPlayerPosition = PointSnapshot.From(LastKnownPlayerPosition),
        InvestigationOrigin = PointSnapshot.From(InvestigationOrigin), InvestigationSource = InvestigationSource,
        InvestigationConfidence = InvestigationConfidence,
        LastObservedPlayerMovement = PointSnapshot.From(LastObservedPlayerMovement),
        PredictedInvestigationTarget = PointSnapshot.From(PredictedInvestigationTarget),
        IsLocalSearching = _isLocalSearching, LastObservedTrailSequence = _lastObservedTrailSequence,
        SearchTargets = _searchTargets.Select(point => new PointSnapshot(point.X, point.Y)).ToList(),
        ObservedCasualties = _observedCasualties.ToList(),
        KnownHazards = _knownHazards.Select(point => new PointSnapshot(point.X, point.Y)).ToList(),
        HeldItemId = HeldItem?.Id,
        StatusEffects = StatusEffects.Effects.Select(StatusEffectSnapshot.From).ToList()
    };

    internal void RestorePersistence(NpcSaveSnapshot snapshot, IReadOnlyDictionary<Guid, Item> items)
    {
        Id = snapshot.Id;
        Name = snapshot.Name;
        Description = snapshot.Description;
        X = snapshot.X; Y = snapshot.Y; HomeX = snapshot.HomeX; HomeY = snapshot.HomeY;
        HP = snapshot.HP; MaxHP = snapshot.MaxHP; Damage = snapshot.Damage;
        Direction = snapshot.Direction; State = snapshot.State; Mood = snapshot.Mood;
        MoodValue = snapshot.MoodValue; Visibility = snapshot.Visibility;
        MoraleState = snapshot.MoraleState; RetreatTarget = snapshot.RetreatTarget?.ToTuple();
        HasCalledForHelp = snapshot.HasCalledForHelp; HasSeenPlayer = snapshot.HasSeenPlayer;
        Awareness = snapshot.Awareness; LastKnownPlayerPosition = snapshot.LastKnownPlayerPosition?.ToTuple();
        InvestigationOrigin = snapshot.InvestigationOrigin?.ToTuple();
        InvestigationSource = snapshot.InvestigationSource;
        InvestigationConfidence = snapshot.InvestigationConfidence;
        LastObservedPlayerMovement = snapshot.LastObservedPlayerMovement?.ToTuple();
        PredictedInvestigationTarget = snapshot.PredictedInvestigationTarget?.ToTuple();
        _isLocalSearching = snapshot.IsLocalSearching;
        _lastObservedTrailSequence = snapshot.LastObservedTrailSequence;
        _searchTargets.Clear();
        foreach (PointSnapshot point in snapshot.SearchTargets) _searchTargets.Enqueue(point.ToTuple());
        _observedCasualties.Clear();
        foreach (Guid id in snapshot.ObservedCasualties) _observedCasualties.Add(id);
        _knownHazards.Clear();
        foreach (PointSnapshot point in snapshot.KnownHazards) _knownHazards.Add(point.ToTuple());
        HeldItem = snapshot.HeldItemId is Guid itemId ? items[itemId] : null;
        StatusEffects.Restore(snapshot.StatusEffects.Select(effect => effect.ToModel()));
        CurrentRoom = Map.MapCells[X, Y].ParentElement;
    }


    // this is where the NPC starts out from, and is where it will 'home' back to when it can't find a target   
    // (the target is dead or disappeared etc)
    public int HomeX { get; set; }
    public int HomeY { get; set; }

    public BaseNPC(Map map, int x, int y, BaseContainingElement currentRoom)
    {
        this.Map = map;
        this.X = x;
        this.Y = y;
        this.HomeX = x;
        this.HomeY = y;
        this.CurrentRoom = currentRoom;

        Direction = (Direction)RandGen.RandInt(0, 4);
    }


    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            State = NPCState.Dead;
            Awareness = NPCAwareness.Unaware;
            LastKnownPlayerPosition = null;
            InvestigationOrigin = null;
            InvestigationSource = NPCInvestigationSource.None;
            InvestigationConfidence = 0;
            LastObservedPlayerMovement = null;
            PredictedInvestigationTarget = null;
            _searchTargets.Clear();
            _isLocalSearching = false;
            Console.WriteLine($"{Name} has been killed!");
            return;
        }

        ReactToWounds();
    }

    private void ReactToWounds()
    {
        if (MoraleProfile.IsFearless)
        {
            MoraleState = NPCMoraleState.Fearless;
            return;
        }

        int healthPercent = HP * 100 / MaxHP;
        if (healthPercent > MoraleProfile.FleeHealthPercent) return;
        if (MoraleProfile.EnragesWhenWounded)
        {
            MoraleState = NPCMoraleState.Enraged;
            return;
        }

        if (MoraleState != NPCMoraleState.Fleeing)
        {
            MoraleState = NPCMoraleState.Fleeing;
            RetreatTarget = null;
            HasCalledForHelp = false;
        }
    }

    public void Move(Player player, Action<string> eventSink = null)
    {
        if (State != NPCState.Active) return;
        EnvironmentalEffect fire = Map.GetEnvironmentalEffectAt(X, Y, EnvironmentalEffectType.Fire);
        if (fire != null)
        {
            TakeDamage(fire.Power);
            eventSink?.Invoke($"{Name} BURNED {fire.Power}");
            if (State == NPCState.Dead)
            {
                ResolveDeathConsequences(eventSink);
                return;
            }
        }
        StatusTurnResult statusTurn = StatusEffects.AdvanceTurn();
        if (statusTurn.BleedingDamage > 0)
        {
            TakeDamage(statusTurn.BleedingDamage);
            eventSink?.Invoke($"{Name} BLED {statusTurn.BleedingDamage}");
            if (State == NPCState.Dead)
            {
                ResolveDeathConsequences(eventSink);
                return;
            }
        }
        if (statusTurn.SkipAction)
        {
            eventSink?.Invoke($"{Name} STUNNED");
            return;
        }

        ObserveCasualtiesAndHazards(eventSink);

        int dx = Math.Abs(X - player.X);
        int dy = Math.Abs(Y - player.Y);

        if (dx + dy <= AwarenessProfile.SightRange && Map.HasLineOfSight(X, Y, player.X, player.Y))
        {
            bool newlySpottedPlayer = Awareness != NPCAwareness.Pursuing;
            if (LastKnownPlayerPosition is { } previousPlayerPosition)
            {
                int movementX = player.X - previousPlayerPosition.X;
                int movementY = player.Y - previousPlayerPosition.Y;
                if (Math.Abs(movementX) + Math.Abs(movementY) == 1)
                {
                    LastObservedPlayerMovement = (movementX, movementY);
                    PredictedInvestigationTarget = Map.PredictContinuation(
                        player.X, player.Y, movementX, movementY);
                }
            }
            HasSeenPlayer = true;
            Awareness = NPCAwareness.Pursuing;
            LastKnownPlayerPosition = (player.X, player.Y);
            InvestigationOrigin = (player.X, player.Y);
            InvestigationSource = NPCInvestigationSource.LastSeen;
            InvestigationConfidence = InitialConfidence(NPCInvestigationSource.LastSeen);
            _searchTargets.Clear();
            _isLocalSearching = false;

            if (newlySpottedPlayer && MoraleState != NPCMoraleState.Fleeing)
            {
                int alertedAllies = Map.AlertNearbyAllies(this, player.X, player.Y);
                if (alertedAllies > 0) eventSink?.Invoke($"{Name} ALERTED {alertedAllies} ALLIES");
            }

            if (TryFlee(eventSink)) return;

            if (RangedProfile != null)
            {
                int distance = dx + dy;
                if (distance < RangedProfile.MinimumRange && TryTacticalRetreat(player, eventSink))
                    return;
                if (distance >= RangedProfile.MinimumRange && distance <= RangedProfile.MaximumRange)
                {
                    int actualDamage = player.TakeDamage(RangedProfile.Damage);
                    eventSink?.Invoke($"{Name} SHOT PLAYER {actualDamage}");
                    int listeners = Map.NotifyNoise(player.X, player.Y, 8, this);
                    if (listeners > 0) eventSink?.Invoke($"RANGED COMBAT DREW {listeners} NPCS");
                    return;
                }
            }

            if (dx + dy == 1)
            {
                Console.WriteLine($"{Name} attacked player with {EffectiveDamage} damage!");
                int actualDamage = player.TakeDamage(EffectiveDamage);
                eventSink?.Invoke($"{Name} HIT PLAYER {actualDamage}");
                int listeners = Map.NotifyNoise(X, Y, 10, this);
                if (listeners > 0) eventSink?.Invoke($"COMBAT DREW {listeners} NPCS");
                return;
            }

            MoveToward(player.X, player.Y, eventSink);
            return;
        }

        if (TryFlee(eventSink)) return;

        if (Awareness == NPCAwareness.Pursuing && PredictedInvestigationTarget is { } prediction)
        {
            InvestigationOrigin = prediction;
            _searchTargets.Clear();
            _isLocalSearching = false;
        }

        if (InvestigationOrigin is { } investigationOrigin)
        {
            Awareness = NPCAwareness.Investigating;
            FollowNearbyTrail(eventSink);
            investigationOrigin = InvestigationOrigin.Value;
            if (InvestigationConfidence <= 0)
            {
                AbandonInvestigation();
                Wander(player, eventSink);
                return;
            }

            if (!_isLocalSearching && (X != investigationOrigin.X || Y != investigationOrigin.Y))
            {
                MoveToward(investigationOrigin.X, investigationOrigin.Y, eventSink);
                SpendInvestigationConfidence();
                return;
            }

            if (!_isLocalSearching)
            {
                BeginLocalSearch(investigationOrigin);
            }

            if (_searchTargets.Count > 0 && X == _searchTargets.Peek().X && Y == _searchTargets.Peek().Y)
            {
                _searchTargets.Dequeue();
            }

            if (_searchTargets.Count == 0)
            {
                AbandonInvestigation();
                return;
            }

            (int targetX, int targetY) = _searchTargets.Peek();
            MoveToward(targetX, targetY, eventSink);
            SpendInvestigationConfidence();
            return;
        }

        Awareness = NPCAwareness.Unaware;
        Wander(player, eventSink);
    }

    public bool ReceiveInvestigation((int X, int Y) origin, NPCInvestigationSource source)
    {
        if (State != NPCState.Active || Awareness == NPCAwareness.Pursuing) return false;
        if (source < InvestigationSource) return false;

        Awareness = NPCAwareness.Investigating;
        InvestigationOrigin = origin;
        InvestigationSource = source;
        InvestigationConfidence = InitialConfidence(source);
        LastKnownPlayerPosition = source == NPCInvestigationSource.LastSeen ? origin : null;
        PredictedInvestigationTarget = null;
        LastObservedPlayerMovement = null;
        _searchTargets.Clear();
        _isLocalSearching = false;
        return true;
    }

    private int InitialConfidence(NPCInvestigationSource source)
    {
        int evidenceConfidence = source switch
        {
            NPCInvestigationSource.Noise => 8,
            NPCInvestigationSource.AllyAlert => 10,
            NPCInvestigationSource.Casualty => 10,
            NPCInvestigationSource.Trail => 11,
            NPCInvestigationSource.LastSeen => 12,
            _ => 0
        };
        return Math.Max(1, evidenceConfidence + AwarenessProfile.PersistenceAdjustment);
    }

    private void AbandonInvestigation()
    {
        LastKnownPlayerPosition = null;
        InvestigationOrigin = null;
        InvestigationSource = NPCInvestigationSource.None;
        InvestigationConfidence = 0;
        LastObservedPlayerMovement = null;
        PredictedInvestigationTarget = null;
        Awareness = NPCAwareness.Unaware;
        _searchTargets.Clear();
        _isLocalSearching = false;
    }

    private bool TryFlee(Action<string> eventSink)
    {
        if (MoraleState != NPCMoraleState.Fleeing) return false;
        if (LastKnownPlayerPosition is not { } threat) return false;

        if (!HasCalledForHelp)
        {
            int listeners = Map.NotifyNoise(X, Y, MoraleProfile.HelpCallRadius, this);
            int alerted = Map.AlertNearbyAllies(this, threat.X, threat.Y, MoraleProfile.HelpCallRadius);
            HasCalledForHelp = true;
            eventSink?.Invoke($"{Name} CALLED FOR HELP {alerted}");
            if (listeners > alerted) eventSink?.Invoke($"CALL HEARD BY {listeners} NPCS");
        }

        RetreatTarget ??= Map.FindRetreatTarget(this, threat.X, threat.Y);
        if (RetreatTarget is not { } retreat)
        {
            MoraleState = NPCMoraleState.Shaken;
            return true;
        }

        if (X == retreat.X && Y == retreat.Y)
        {
            MoraleState = NPCMoraleState.Shaken;
            RetreatTarget = null;
            eventSink?.Invoke($"{Name} REACHED SAFETY");
            return true;
        }

        Awareness = NPCAwareness.Investigating;
        MoveToward(retreat.X, retreat.Y, eventSink);
        return true;
    }

    public void ApplyStatus(StatusEffectType type, int duration, int power = 0, string source = "UNKNOWN")
    {
        StatusEffects.Apply(type, duration, power, source);
    }

    public void ResolveDeathConsequences(Action<string> eventSink = null)
    {
        if (State != NPCState.Dead) return;
        eventSink?.Invoke($"{Name} DIED");
        if (Map.DropItemNear(HeldItem, X, Y, out _))
        {
            eventSink?.Invoke($"{Name} DROPPED {HeldItem.Name}");
            HeldItem = null;
        }
    }

    private void FollowNearbyTrail(Action<string> eventSink)
    {
        PlayerTrailClue clue = Map.FindNewestTrailNear(
            X, Y, _lastObservedTrailSequence, AwarenessProfile.TrailDetectionRange);
        if (clue == null) return;

        _lastObservedTrailSequence = clue.Sequence;
        if (!clue.IsAuthentic && AwarenessProfile.TrailDetectionRange >= 2)
        {
            eventSink?.Invoke($"{Name} REJECTED FALSE TRAIL");
            return;
        }
        if (clue.NextX == X && clue.NextY == Y) return;

        InvestigationOrigin = (clue.NextX, clue.NextY);
        InvestigationSource = NPCInvestigationSource.Trail;
        InvestigationConfidence = InitialConfidence(NPCInvestigationSource.Trail);
        PredictedInvestigationTarget = null;
        _searchTargets.Clear();
        _isLocalSearching = false;
        eventSink?.Invoke($"{Name} FOUND TRAIL");
    }

    private void SpendInvestigationConfidence()
    {
        InvestigationConfidence--;
        if (InvestigationConfidence <= 0) AbandonInvestigation();
    }

    private void BeginLocalSearch((int X, int Y) origin)
    {
        _isLocalSearching = true;

        foreach ((int dx, int dy) in new[] { (0, -1), (1, 0), (0, 1), (-1, 0) })
        {
            int targetX = origin.X + dx;
            int targetY = origin.Y + dy;
            if (Map.IsWalkable(targetX, targetY))
            {
                _searchTargets.Enqueue((targetX, targetY));
            }
        }
    }

    private void MoveToward(int targetX, int targetY, Action<string> eventSink = null)
    {
        var path = Pathfinding.FindPath(Map, X, Y, targetX, targetY, this);
        if (path.Count > 0)
        {
            Doorway door = Map.GetDoorAt(path[0].X, path[0].Y);
            if (door?.State == DoorState.Closed)
            {
                door.State = DoorState.Open;
                eventSink?.Invoke($"{Name} OPENED DOOR");
                Map.UpdateVisibility(Map.CurrentPlayerX, Map.CurrentPlayerY);
                return;
            }

            MoveTo(path[0].X, path[0].Y);
            TriggerTrap(eventSink);
        }
    }

    private void Wander(Player player, Action<string> eventSink = null)
    {
        var newX = X;
        var newY = Y;
        // chance of just changing direction mid-flight
        if (RandGen.RandInt(0, 100) < 15)
        {
            Direction = (Direction)(RandGen.RandInt(0, 4));
        }

        if (!CanMove(player, out newX, out newY))
        {
            //Console.WriteLine($"{Name} can't move {Direction} from ({X}, {Y})");
            var attempts = 0;
            while (attempts < 3 && !CanMove(player, out newX, out newY))
            {
                Direction = (Direction)(RandGen.RandInt(0, 4));
                attempts++;
            }
            if (attempts == 3)
            {
                // reset to opposite last direction
                Direction = (Direction)(((int)Direction + 2) % 4);
                Console.WriteLine($"{Name} got stuck at ({X}, {Y}), flipped the direction to {Direction}");
                // but we don't move this time
                return;
            }
        }

        MoveTo(newX, newY);
        TriggerTrap(eventSink);
    }

    private void TriggerTrap(Action<string> eventSink)
    {
        PlacedTrap trap = Map.GetTrapAt(X, Y);
        if (trap == null) return;

        Map.RemoveTrap(trap);
        TakeDamage(trap.Damage);
        eventSink?.Invoke($"{Name} TRIGGERED TRAP {trap.Damage} ({trap.Kind})");
        if (State == NPCState.Active && trap.Kind != TrapKind.Alarm)
        {
            int duration = trap.Kind == TrapKind.Snare ? 2 : 1;
            ApplyStatus(StatusEffectType.Stunned, duration, source: $"{trap.Kind} TRAP");
            eventSink?.Invoke($"{Name} STUNNED BY {trap.Kind} TRAP");
        }
        if (State == NPCState.Dead) ResolveDeathConsequences(eventSink);
        int radius = trap.Kind == TrapKind.Alarm ? 16 : 9;
        int listeners = Map.NotifyNoise(X, Y, radius, this);
        if (listeners > 0) eventSink?.Invoke($"TRAP DREW {listeners} NPCS");
    }

    private void MoveTo(int newX, int newY)
    {
        X = newX;
        Y = newY;

        if (Map.MapCells[X, Y].ParentElement != null && Map.MapCells[X, Y].ParentElement != CurrentRoom)
        {
            Console.WriteLine($"{Name} changed room ({X}, {Y})");
            CurrentRoom = Map.MapCells[X, Y].ParentElement;
        }
    }

    private bool CanMove(Player player, out int newX, out int newY)
    {
        newX = X;
        newY = Y;
        switch (Direction)
        {
            case Direction.Up:
                newY--;
                break;
            case Direction.Down:
                newY++;
                break;
            case Direction.Left:
                newX--;
                break;
            case Direction.Right:
                newX++;
                break;
            default:
                break;
        }
        return !IsKnownHazard(newX, newY)
            && Map.CanNpcEnter(newX, newY, this, player);
    }

    private bool TryTacticalRetreat(Player player, Action<string> eventSink)
    {
        int currentDistance = Math.Abs(X - player.X) + Math.Abs(Y - player.Y);
        (int X, int Y)? best = null;
        int bestDistance = currentDistance;
        foreach ((int dx, int dy) in new[] { (0, -1), (1, 0), (0, 1), (-1, 0) })
        {
            int candidateX = X + dx;
            int candidateY = Y + dy;
            int distance = Math.Abs(candidateX - player.X) + Math.Abs(candidateY - player.Y);
            if (distance <= bestDistance || IsKnownHazard(candidateX, candidateY)
                || !Map.CanNpcEnter(candidateX, candidateY, this, player)) continue;
            best = (candidateX, candidateY);
            bestDistance = distance;
        }

        if (best == null) return false;
        MoveTo(best.Value.X, best.Value.Y);
        TriggerTrap(eventSink);
        eventSink?.Invoke($"{Name} KEPT DISTANCE");
        return true;
    }

    public bool IsKnownHazard(int x, int y) => _knownHazards.Contains((x, y));

    private void ObserveCasualtiesAndHazards(Action<string> eventSink)
    {
        foreach ((int hazardX, int hazardY) in _knownHazards.ToArray())
        {
            if (CanSee(hazardX, hazardY) && Map.GetTrapAt(hazardX, hazardY) == null)
                _knownHazards.Remove((hazardX, hazardY));
        }

        foreach (PlacedTrap trap in Map.PlacedTraps)
        {
            int distance = Math.Abs(X - trap.X) + Math.Abs(Y - trap.Y);
            if (distance <= AwarenessProfile.TrapDetectionRange
                && CanSee(trap.X, trap.Y)
                && _knownHazards.Add((trap.X, trap.Y)))
                eventSink?.Invoke($"{Name} SPOTTED TRAP");
        }

        foreach (BaseNPC casualty in Map.NPCs)
        {
            if (casualty == this || casualty.State != NPCState.Dead
                || _observedCasualties.Contains(casualty.Id)
                || !CanSee(casualty.X, casualty.Y)) continue;

            _observedCasualties.Add(casualty.Id);
            eventSink?.Invoke($"{Name} FOUND {casualty.Name} DEAD");
            ReceiveInvestigation((casualty.X, casualty.Y), NPCInvestigationSource.Casualty);
        }
    }

    private bool CanSee(int targetX, int targetY)
    {
        return Math.Abs(X - targetX) + Math.Abs(Y - targetY) <= AwarenessProfile.SightRange
            && Map.HasLineOfSight(X, Y, targetX, targetY);
    }

}
