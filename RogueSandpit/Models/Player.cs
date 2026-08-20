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
    public int MaxHealth { get; private set; } = 0;
    public int Health { get; private set; } = 0;
    public int BaseDamage { get; private set; } = 0;
    public int Damage => BaseDamage + (EquippedWeapon?.Power ?? 0);
    public bool Dead { get; private set; } = false;
    public bool HasSpecial { get; private set; } = false;
    public Inventory Inventory { get; private set; } = new();
    public Item EquippedWeapon { get; private set; }
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
        MaxHealth = 100 + RandGen.RandInt(0, 50);
        Health = MaxHealth;
        BaseDamage = 10 + RandGen.RandInt(0, 20);
        Dead = false;
        HasSpecial = false;
        Inventory = new Inventory();
        EquippedWeapon = null;
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

    public int Heal(int amount)
    {
        if (amount <= 0 || Dead) return 0;
        int previousHealth = Health;
        Health = Math.Min(MaxHealth, Health + amount);
        return Health - previousHealth;
    }

    public bool Equip(Item weapon)
    {
        if (weapon?.Type != ItemType.Weapon || !Inventory.Items.Contains(weapon)) return false;
        EquippedWeapon = weapon;
        return true;
    }

    public bool RemoveFromInventory(Item item)
    {
        if (!Inventory.Remove(item)) return false;
        if (EquippedWeapon == item) EquippedWeapon = null;
        return true;
    }



}
