using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public class Player
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int Health { get; private set; } = 0;
    public int Damage { get; private set; } = 0;
    public bool Dead { get; private set; } = false;
    public bool HasSpecial { get; private set; } = false;
    public BaseContainingElement CurrentRoom { get; set; } = null;

    public Player()
    {
        Reset();
    }

    public void Update()
    {
        Console.WriteLine($"Player is at ({X}, {Y}) with {Health} HP and {Damage} damage.");
        // any per-turn updates to the player would go here
        /*
        // Check if adjacent for attack
        int dx = Math.Abs(X - player.X);
        int dy = Math.Abs(Y - player.Y);
        if (dx <= 1 && dy <= 1 && (dx + dy) > 0)
        {
            // Attack
            Console.WriteLine($"NPC {Name} attacked player at ({player.X}, {player.Y}) with {Damage} damage!");
            player.TakeDamage(Damage);
            return;
        }
        */
    }

    public void Reset()
    {
        Health = 100 + (RandGen.RandInt(0, 50));
        Damage = 10 + (RandGen.RandInt(0, 20));
        Dead = false;
        HasSpecial = false;
    }

    public void CollectSpecial()
    {
        HasSpecial = true;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health < 0) Health = 0;
        if (Health == 0)
        {
            Dead = true;
        }
    }



}
