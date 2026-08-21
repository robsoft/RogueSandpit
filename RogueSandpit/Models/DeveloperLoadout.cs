using System.Linq;

namespace RogueSandpit.Models;

public static class DeveloperLoadout
{
    public static void Apply(Player player)
    {
        player.Heal(player.MaxHealth);

        foreach (Item item in player.Inventory.Items.ToList())
        {
            player.RemoveFromInventory(item);
        }

        Item weapon = ItemFactory.CreateEquipment(ItemType.Weapon, 2);
        Item armor = ItemFactory.CreateEquipment(ItemType.Armor, 2);
        Item ranged = ItemFactory.CreateEquipment(ItemType.RangedWeapon, 2);
        Item[] items =
        [
            weapon,
            armor,
            ranged,
            ItemFactory.Create(ItemType.HealingPotion),
            ItemFactory.Create(ItemType.Key),
            ItemFactory.CreateTrap(TrapKind.Hunting),
            ItemFactory.Create(ItemType.SmokeBomb),
            ItemFactory.Create(ItemType.FireBomb)
        ];

        foreach (Item item in items) player.TryCollectItem(item, out _);

        player.Equip(weapon);
        player.EquipArmor(armor);
        player.EquipRanged(ranged);
    }
}
