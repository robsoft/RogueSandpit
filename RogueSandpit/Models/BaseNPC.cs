using System;
namespace RogueSandpit.Models;


public abstract class BaseNPC
{
    private const int SightRange = 12;

    public Guid Id { get; private set; } = Guid.NewGuid();
    public BaseContainingElement CurrentRoom { get; private set; }
    public string Description { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public Map Map { get; private set; }

    public Direction Direction { get; set; } = Direction.Up;
    public int X { get; set; }
    public int Y { get; set; }
    public int HP { get; set; }
    public int Damage { get; set; }

    public int AssetID { get; set; }
    public int AnimFrame { get; set; }

    public NPCState State { get; set; } = NPCState.InActive;
    public CharacterMood Mood { get; set; } = CharacterMood.Neutral;
    public int MoodValue { get; set; } = 0;

    public Visibility Visibility { get; set; } = Visibility.Hidden;
    public bool HasSeenPlayer { get; private set; } = false;
    public NPCAwareness Awareness { get; private set; } = NPCAwareness.Unaware;
    public (int X, int Y)? LastKnownPlayerPosition { get; private set; }
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
            Console.WriteLine($"{Name} has been killed!");
        }
    }

    public void Move(Player player, Action<string> eventSink = null)
    {
        int dx = Math.Abs(X - player.X);
        int dy = Math.Abs(Y - player.Y);

        if (dx + dy <= SightRange && Map.HasLineOfSight(X, Y, player.X, player.Y))
        {
            HasSeenPlayer = true;
            Awareness = NPCAwareness.Pursuing;
            LastKnownPlayerPosition = (player.X, player.Y);

            if (dx + dy == 1)
            {
                Console.WriteLine($"{Name} attacked player with {Damage} damage!");
                eventSink?.Invoke($"{Name} HIT PLAYER {Damage}");
                player.TakeDamage(Damage);
                return;
            }

            MoveToward(player.X, player.Y);
            return;
        }

        if (LastKnownPlayerPosition is { } lastKnownPosition)
        {
            Awareness = NPCAwareness.Investigating;
            if (X == lastKnownPosition.X && Y == lastKnownPosition.Y)
            {
                LastKnownPlayerPosition = null;
                Awareness = NPCAwareness.Unaware;
                Wander(player);
                return;
            }

            MoveToward(lastKnownPosition.X, lastKnownPosition.Y);
            return;
        }

        Awareness = NPCAwareness.Unaware;
        Wander(player);
    }

    private void MoveToward(int targetX, int targetY)
    {
        var path = Pathfinding.FindPath(Map, X, Y, targetX, targetY, this);
        if (path.Count > 0)
        {
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
        return Map.IsWalkable(newX, newY)
            && !Map.IsOccupiedByLivingNPC(newX, newY, this)
            && (newX != player.X || newY != player.Y);
    }

}
