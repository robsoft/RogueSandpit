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
    public int Defence => EquippedArmor?.Power ?? 0;
    public bool Dead { get; private set; } = false;
    public bool HasSpecial { get; private set; } = false;
    public Inventory Inventory { get; private set; } = new();
    public Item EquippedWeapon { get; private set; }
    public Item EquippedArmor { get; private set; }
    public Item EquippedRangedWeapon { get; private set; }
    public BaseContainingElement CurrentRoom { get; set; } = null;
    public StatusEffectCollection StatusEffects { get; } = new();

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
        EquippedArmor = null;
        EquippedRangedWeapon = null;
        StatusEffects.Clear();
    }

    public void CollectSpecial()
    {
        HasSpecial = true;
    }

    public void Place(Map map, int x, int y)
    {
        X = x;
        Y = y;
        CurrentRoom = map.MapCells[x, y].ParentElement;
        if (CurrentRoom != null) CurrentRoom.HasVisited = true;
        map.CurrentPlayerX = x;
        map.CurrentPlayerY = y;
        map.UpdateVisibility(x, y);
    }

    public int TakeDamage(int damage)
    {
        int actualDamage = Math.Max(1, damage - Defence);
        Health -= actualDamage;
        if (Health < 0) Health = 0;
        if (Health == 0)
        {
            Dead = true;
        }
        return actualDamage;
    }

    public void ApplyStatus(StatusEffectType type, int duration, int power = 0, string source = "UNKNOWN")
    {
        StatusEffects.Apply(type, duration, power, source);
    }

    public StatusTurnResult AdvanceStatusTurn()
    {
        StatusTurnResult result = StatusEffects.AdvanceTurn();
        if (result.BleedingDamage > 0) TakeDamage(result.BleedingDamage);
        return result;
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

    public bool EquipRanged(Item weapon)
    {
        if (weapon?.Type != ItemType.RangedWeapon || !Inventory.Items.Contains(weapon)) return false;
        EquippedRangedWeapon = weapon;
        return true;
    }

    public bool EquipArmor(Item armor)
    {
        if (armor?.Type != ItemType.Armor || !Inventory.Items.Contains(armor)) return false;
        EquippedArmor = armor;
        return true;
    }

    public bool RemoveFromInventory(Item item)
    {
        if (!Inventory.Remove(item)) return false;
        if (EquippedWeapon == item) EquippedWeapon = null;
        if (EquippedArmor == item) EquippedArmor = null;
        if (EquippedRangedWeapon == item) EquippedRangedWeapon = null;
        return true;
    }

    public bool SelectInventoryItem(bool next)
    {
        return next ? Inventory.SelectNext() : Inventory.SelectPrevious();
    }

    public bool TryCollectItem(Item item, out bool autoEquipped)
    {
        autoEquipped = false;
        if (!Inventory.TryAdd(item)) return false;

        if (item.Type == ItemType.Weapon && EquippedWeapon == null)
        {
            autoEquipped = Equip(item);
        }
        else if (item.Type == ItemType.RangedWeapon && EquippedRangedWeapon == null)
        {
            autoEquipped = EquipRanged(item);
        }

        return true;
    }

    public PlayerItemActionResult UseSelectedPotion(out int healed)
    {
        healed = 0;
        Item potion = Inventory.SelectedItem;
        if (potion == null) return PlayerItemActionResult.NoSelection;
        if (potion.Type != ItemType.HealingPotion) return PlayerItemActionResult.WrongItemType;

        healed = Heal(potion.Power);
        if (healed == 0) return PlayerItemActionResult.NoEffect;
        RemoveFromInventory(potion);
        return PlayerItemActionResult.Success;
    }

    public PlayerItemActionResult EquipSelectedItem(out Item equippedItem)
    {
        equippedItem = Inventory.SelectedItem;
        if (equippedItem == null) return PlayerItemActionResult.NoSelection;
        if (equippedItem.Type == ItemType.Weapon && Equip(equippedItem)) return PlayerItemActionResult.Success;
        if (equippedItem.Type == ItemType.Armor && EquipArmor(equippedItem)) return PlayerItemActionResult.Success;
        if (equippedItem.Type == ItemType.RangedWeapon && EquipRanged(equippedItem)) return PlayerItemActionResult.Success;
        return PlayerItemActionResult.WrongItemType;
    }

    public PlayerItemActionResult DropSelectedItem(Map map, out Item droppedItem)
    {
        droppedItem = Inventory.SelectedItem;
        if (droppedItem == null) return PlayerItemActionResult.NoSelection;
        if (!map.DropItem(droppedItem, X, Y)) return PlayerItemActionResult.Blocked;

        RemoveFromInventory(droppedItem);
        return PlayerItemActionResult.Success;
    }



}
