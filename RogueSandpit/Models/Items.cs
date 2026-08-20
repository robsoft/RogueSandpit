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

    public Item(string name, ItemType type, int power = 0)
    {
        Name = name;
        Type = type;
        Power = power;
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

    public Inventory(int capacity = 8)
    {
        Capacity = capacity;
    }

    public bool TryAdd(Item item)
    {
        if (item == null || IsFull) return false;
        _items.Add(item);
        return true;
    }

    public Item FindFirst(ItemType type)
    {
        return _items.FirstOrDefault(item => item.Type == type);
    }

    public bool Remove(Item item)
    {
        return item != null && _items.Remove(item);
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
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static Item CreateRandom()
    {
        return Create((ItemType)RandGen.RandInt(0, Enum.GetValues<ItemType>().Length));
    }
}
