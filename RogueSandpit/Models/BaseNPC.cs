using System;
using System.Collections.Generic;

namespace RogueSandpit.Models;


public abstract class BaseNPC
{
    private readonly Queue<(int X, int Y)> _searchTargets = new();
    private bool _isLocalSearching;

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
    public int Damage { get; set; }
    public Item HeldItem { get; set; }
    public NPCAwarenessProfile AwarenessProfile { get; protected set; }

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
    public (int X, int Y)? InvestigationTarget =>
        _searchTargets.Count > 0 ? _searchTargets.Peek() : InvestigationOrigin;
    public bool IsPursuingPlayer => Awareness == NPCAwareness.Pursuing;


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
            _searchTargets.Clear();
            _isLocalSearching = false;
            Console.WriteLine($"{Name} has been killed!");
        }
    }

    public void Move(Player player, Action<string> eventSink = null)
    {
        int dx = Math.Abs(X - player.X);
        int dy = Math.Abs(Y - player.Y);

        if (dx + dy <= AwarenessProfile.SightRange && Map.HasLineOfSight(X, Y, player.X, player.Y))
        {
            bool newlySpottedPlayer = Awareness != NPCAwareness.Pursuing;
            HasSeenPlayer = true;
            Awareness = NPCAwareness.Pursuing;
            LastKnownPlayerPosition = (player.X, player.Y);
            InvestigationOrigin = (player.X, player.Y);
            InvestigationSource = NPCInvestigationSource.LastSeen;
            InvestigationConfidence = InitialConfidence(NPCInvestigationSource.LastSeen);
            _searchTargets.Clear();
            _isLocalSearching = false;

            if (newlySpottedPlayer)
            {
                int alertedAllies = Map.AlertNearbyAllies(this, player.X, player.Y);
                if (alertedAllies > 0) eventSink?.Invoke($"{Name} ALERTED {alertedAllies} ALLIES");
            }

            if (dx + dy == 1)
            {
                Console.WriteLine($"{Name} attacked player with {Damage} damage!");
                int actualDamage = player.TakeDamage(Damage);
                eventSink?.Invoke($"{Name} HIT PLAYER {actualDamage}");
                int listeners = Map.NotifyNoise(X, Y, 10, this);
                if (listeners > 0) eventSink?.Invoke($"COMBAT DREW {listeners} NPCS");
                return;
            }

            MoveToward(player.X, player.Y, eventSink);
            return;
        }

        if (InvestigationOrigin is { } investigationOrigin)
        {
            Awareness = NPCAwareness.Investigating;
            if (InvestigationConfidence <= 0)
            {
                AbandonInvestigation();
                Wander(player);
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
        Wander(player);
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
        Awareness = NPCAwareness.Unaware;
        _searchTargets.Clear();
        _isLocalSearching = false;
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
        }
    }

    private void Wander(Player player)
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
        return Map.CanNpcEnter(newX, newY, this, player);
    }

}
