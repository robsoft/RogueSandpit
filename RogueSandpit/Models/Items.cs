using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueSandpit.Models;

public class Item
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    public ItemType Type { get; }
    public int Power { get; }
    public TrapKind? TrapKind { get; }

    public Item(string name, ItemType type, int power = 0, TrapKind? trapKind = null)
    {
        Name = name;
        Type = type;
        Power = power;
        TrapKind = trapKind;
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
        if (_items.Count == 0) return false;
        SelectedIndex = (SelectedIndex + 1) % _items.Count;
        return true;
    }

    public bool SelectPrevious()
    {
        if (_items.Count == 0) return false;
        SelectedIndex = (SelectedIndex - 1 + _items.Count) % _items.Count;
        return true;
    }
}

public static class ItemFactory
{
    public static Item Create(ItemType type)
    {
        return type switch
        {
            ItemType.HealingPotion => new Item("HEALING POTION", type, 35),
            ItemType.Weapon => new Item("IRON SWORD", type, 8),
            ItemType.Key => new Item("BRASS KEY", type),
            ItemType.Armor => new Item("LEATHER ARMOR", type, 5),
            ItemType.Trap => new Item("HUNTING TRAP", type, 18),
            ItemType.RangedWeapon => new Item("SHORT BOW", type, 7),
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

    public static Item CreateRandom()
    {
        ItemType type = (ItemType)RandGen.RandInt(0, Enum.GetValues<ItemType>().Length);
        return type == ItemType.Trap
            ? CreateTrap((TrapKind)RandGen.RandInt(0, Enum.GetValues<TrapKind>().Length))
            : Create(type);
    }
}
