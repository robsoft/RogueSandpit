using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class DeveloperLoadoutTests
{
    [Fact]
    public void ApplyRestoresHealthAndReplacesInventoryWithEquippedRepresentativeSet()
    {
        var player = new Player();
        player.TakeDamage(25);
        player.TryCollectItem(ItemFactory.Create(ItemType.Bandage), out _);

        DeveloperLoadout.Apply(player);

        Assert.Equal(player.MaxHealth, player.Health);
        Assert.Equal(player.Inventory.Capacity, player.Inventory.Items.Count);
        Assert.DoesNotContain(player.Inventory.Items, item => item.Type == ItemType.Bandage);
        Assert.Contains(player.Inventory.Items, item => item.Type == ItemType.HealingPotion);
        Assert.Contains(player.Inventory.Items, item => item.Type == ItemType.Key);
        Assert.Contains(player.Inventory.Items, item => item.Type == ItemType.Trap);
        Assert.Contains(player.Inventory.Items, item => item.Type == ItemType.SmokeBomb);
        Assert.Contains(player.Inventory.Items, item => item.Type == ItemType.FireBomb);
        Assert.Equal(ItemType.Weapon, player.EquippedWeapon.Type);
        Assert.Equal(ItemType.Armor, player.EquippedArmor.Type);
        Assert.Equal(ItemType.RangedWeapon, player.EquippedRangedWeapon.Type);
        Assert.All(new[] { player.EquippedWeapon, player.EquippedArmor, player.EquippedRangedWeapon },
            item => Assert.Contains(item, player.Inventory.Items));
    }
}
