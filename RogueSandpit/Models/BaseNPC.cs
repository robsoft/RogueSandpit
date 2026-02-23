using System;
using System.Runtime.Intrinsics.X86;
using Microsoft.Xna.Framework;


namespace RogueSandpit.Models;


public abstract class BaseNPC
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public BaseContainingElement CurrentRoom { get; private set; }
    public string Description { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public Map Map { get; private set; }

    public Direction Direction { get; set; } = Direction.Up;
    public int X { get; set; }
    public int Y { get; set; }
    public float Speed { get; set; } = 1.0F;
    public int HP { get; set; }
    public int Damage { get; set; }

    public int AssetID { get; set; }
    public int AnimFrame { get; set; }

    public NPCState State { get; set; } = NPCState.InActive;
    public CharacterMood Mood { get; set; } = CharacterMood.Neutral;
    public int MoodValue { get; set; } = 0;

    public Visibility Visibility { get; set; } = Visibility.Hidden;
    public bool HasSeenPlayer { get; private set; } = false;


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
            Console.WriteLine($"{Name} has been killed!");
        }
    }

    public void Move(GameTime gameTime, Player player)
    {
        // Check if adjacent for attack
        int dx = Math.Abs(X - player.X);
        int dy = Math.Abs(Y - player.Y);
        if (dx <= 1 && dy <= 1 && (dx + dy) > 0)
        {
            // Attack
            Console.WriteLine($"{Name} attacked player with {Damage} damage!");
            player.TakeDamage(Damage);
            return;
        }

        /*

                if (Room.Contains(player.X, player.Y) || HasSeenPlayer)
                {
                    HasSeenPlayer = true;
                    // Chase if in line of sight
                    if (dungeon.HasLineOfSight(X, Y, player.X, player.Y))
                    {
                        var path = Pathfinding.AStar(dungeon, X, Y, player.X, player.Y);
                        if (path.Count > 0)
                        {
                            var next = path[0];
                            X = next.Item1;
                            Y = next.Item2;
                        }
                    }
                {*/

        // Player not seen, random move within room
        var newX = X;
        var newY = Y;
        var lastDirection = (int)Direction;
        // chance of just changing direction mid-flight
        if (RandGen.RandInt(0, 100) < 15)
        {
            Direction = (Direction)(RandGen.RandInt(0, 4));
        }

        if (!CanMove(out newX, out newY))
        {
            //Console.WriteLine($"{Name} can't move {Direction} from ({X}, {Y})");
            var attempts = 0;
            while (attempts < 3 && !CanMove(out newX, out newY))
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

        X = newX;
        Y = newY;

        if (Map.MapCells[X, Y].ParentElement != null && Map.MapCells[X, Y].ParentElement != CurrentRoom)
        {
            Console.WriteLine($"{Name} changed room ({X}, {Y})");
            CurrentRoom = Map.MapCells[X, Y].ParentElement;
        }
    }

    private bool CanMove(out int newX, out int newY)
    {
        newX = X;
        newY = Y;
        switch (Direction)
        {
            case Direction.Up:
                newY = (int)Math.Round(newY - Speed);
                break;
            case Direction.Down:
                newY = (int)Math.Round(newY + Speed);
                break;
            case Direction.Left:
                newX = (int)Math.Round(newX - Speed);
                break;
            case Direction.Right:
                newX = (int)Math.Round(newX + Speed);
                break;
            default:
                break;
        }
        return Map.IsWalkable(newX, newY);
    }

}
