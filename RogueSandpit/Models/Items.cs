using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueSandpit.Models;

public class Item
{
    public Guid Id { get; }
    public string Name { get; }
    public ItemType Type { get; }
    public int Power { get; }
    public int Tier { get; }
    public TrapKind? TrapKind { get; }

    public Item(string name, ItemType type, int power = 0, TrapKind? trapKind = null, int tier = 0,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        Name = name;
        Type = type;
        Power = power;
        TrapKind = trapKind;
        Tier = tier;
    }
}

public class GroundItem
{
    public Item Item { get; }
    public int X { get; }
    public int Y { get; }

    public GroundItem(Item item, int x, int y)
    {
        Item = item;
        X = x;
        Y = y;
    }
}

public class Inventory
{
    private readonly List<Item> _items = [];

    public int Capacity { get; }
    public IReadOnlyList<Item> Items => _items;
    public bool IsFull => _items.Count >= Capacity;
    public int SelectedIndex { get; private set; } = -1;
    public Item SelectedItem => SelectedIndex >= 0 && SelectedIndex < _items.Count
        ? _items[SelectedIndex]
        : null;

    public Inventory(int capacity = 8)
    {
        Capacity = capacity;
    }

    public bool TryAdd(Item item)
    {
        if (item == null || IsFull) return false;
        _items.Add(item);
        if (SelectedIndex < 0) SelectedIndex = 0;
        return true;
    }

    public Item FindFirst(ItemType type)
    {
        return _items.FirstOrDefault(item => item.Type == type);
    }

    public bool Remove(Item item)
    {
        if (item == null) return false;
        int removedIndex = _items.IndexOf(item);
        if (removedIndex < 0) return false;

        _items.RemoveAt(removedIndex);
        if (_items.Count == 0) SelectedIndex = -1;
        else if (removedIndex < SelectedIndex || SelectedIndex >= _items.Count) SelectedIndex--;
        return true;
    }

    public bool SelectNext()
    {
        if (_items.Count < 2) return false;
        SelectedIndex = (SelectedIndex + 1) % _items.Count;
        return true;
    }

    public bool SelectPrevious()
    {
        if (_items.Count < 2) return false;
        SelectedIndex = (SelectedIndex - 1 + _items.Count) % _items.Count;
        return true;
    }

    public bool SelectIndex(int index)
    {
        if (index < 0 || index >= _items.Count || index == SelectedIndex) return false;
        SelectedIndex = index;
        return true;
    }

    internal void RestoreSelection(int index)
    {
        SelectedIndex = _items.Count == 0 ? -1 : Math.Clamp(index, 0, _items.Count - 1);
    }
}

public static class ItemFactory
{
    public static Item Create(ItemType type)
    {
        return type switch
        {
            ItemType.HealingPotion => new Item("HEALING POTION", type, 35),
            ItemType.Weapon => new Item("IRON SWORD", type, 8, tier: 1),
            ItemType.Key => new Item("BRASS KEY", type),
            ItemType.Armor => new Item("LEATHER ARMOR", type, 5, tier: 1),
            ItemType.Trap => new Item("HUNTING TRAP", type, 18),
            ItemType.RangedWeapon => new Item("SHORT BOW", type, 7, tier: 1),
            ItemType.Bandage => new Item("BANDAGE", type, 12),
            ItemType.SmokeBomb => new Item("SMOKE BOMB", type),
            ItemType.FireBomb => new Item("FIRE BOMB", type, 6),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static Item CreateTrap(TrapKind kind) => kind switch
    {
        TrapKind.Hunting => new Item("HUNTING TRAP", ItemType.Trap, 18, kind),
        TrapKind.Snare => new Item("SNARE", ItemType.Trap, 0, kind),
        TrapKind.Alarm => new Item("ALARM TRAP", ItemType.Trap, 0, kind),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static Item CreateEquipment(ItemType type, int tier)
    {
        if (tier is < 1 or > 3) throw new ArgumentOutOfRangeException(nameof(tier));
        return (type, tier) switch
        {
            (ItemType.Weapon, 1) => new Item("IRON SWORD", type, 8, tier: tier),
            (ItemType.Weapon, 2) => new Item("STEEL AXE", type, 12, tier: tier),
            (ItemType.Weapon, 3) => new Item("WAR HAMMER", type, 16, tier: tier),
            (ItemType.Armor, 1) => new Item("LEATHER ARMOR", type, 5, tier: tier),
            (ItemType.Armor, 2) => new Item("CHAIN MAIL", type, 8, tier: tier),
            (ItemType.Armor, 3) => new Item("PLATE ARMOR", type, 12, tier: tier),
            (ItemType.RangedWeapon, 1) => new Item("SHORT BOW", type, 7, tier: tier),
            (ItemType.RangedWeapon, 2) => new Item("HUNTER BOW", type, 10, tier: tier),
            (ItemType.RangedWeapon, 3) => new Item("WAR BOW", type, 13, tier: tier),
            _ => throw new ArgumentException("Equipment tiers require a weapon, armor, or ranged weapon.")
        };
    }

    public static Item CreateRandom()
    {
        ItemType type = (ItemType)RandGen.RandInt(0, Enum.GetValues<ItemType>().Length);
        if (type == ItemType.Trap)
            return CreateTrap((TrapKind)RandGen.RandInt(0, Enum.GetValues<TrapKind>().Length));
        if (type is ItemType.Weapon or ItemType.Armor or ItemType.RangedWeapon)
            return CreateEquipment(type, RandGen.RandInt(1, 4));
        return Create(type);
    }

    public static Item CreateForDepth(GenerationDepthBand depthBand)
    {
        int band = Math.Clamp((int)depthBand, 0, 2);
        ItemType type = (ItemType)RandGen.RandInt(0, Enum.GetValues<ItemType>().Length);
        if (type == ItemType.Trap)
            return CreateTrap((TrapKind)RandGen.RandInt(0, Enum.GetValues<TrapKind>().Length));
        if (type is ItemType.Weapon or ItemType.Armor or ItemType.RangedWeapon)
            return CreateEquipment(type, band + 1);
        return Create(type);
    }
}
