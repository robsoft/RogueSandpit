using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class EquipmentBandageTests
{
    [Theory]
    [InlineData(ItemType.Weapon)]
    [InlineData(ItemType.Armor)]
    [InlineData(ItemType.RangedWeapon)]
    public void EquipmentPowerIncreasesAcrossNamedTiers(ItemType type)
    {
        Item first = ItemFactory.CreateEquipment(type, 1);
        Item second = ItemFactory.CreateEquipment(type, 2);
        Item third = ItemFactory.CreateEquipment(type, 3);

        Assert.True(first.Power < second.Power);
        Assert.True(second.Power < third.Power);
        Assert.NotEqual(first.Name, second.Name);
        Assert.NotEqual(second.Name, third.Name);
    }

    [Fact]
    public void BandageStopsBleedingHealsAndIsConsumed()
    {
        Map map = CreateMap();
        var player = new Player { X = map.StartPosX, Y = map.StartPosY };
        player.TakeDamage(20);
        player.ApplyStatus(StatusEffectType.Bleeding, 3, 2, "TEST");
        Item bandage = ItemFactory.Create(ItemType.Bandage);
        player.Inventory.TryAdd(bandage);
        var game = new GameState(map, player);
        int health = player.Health;

        game.Update(PlayerCommand.UseBandage);

        Assert.False(player.StatusEffects.Has(StatusEffectType.Bleeding));
        Assert.Equal(health + bandage.Power, player.Health);
        Assert.DoesNotContain(bandage, player.Inventory.Items);
        Assert.Contains("BLEEDING STOPPED", game.EventLog.Entries);
        Assert.Contains($"BANDAGED {bandage.Power}", game.EventLog.Entries);
    }

    [Fact]
    public void UnneededBandageIsNotConsumedAndCostsNoTurn()
    {
        Map map = CreateMap();
        var player = new Player { X = map.StartPosX, Y = map.StartPosY };
        Item bandage = ItemFactory.Create(ItemType.Bandage);
        player.Inventory.TryAdd(bandage);
        var game = new GameState(map, player);

        game.Update(PlayerCommand.UseBandage);

        Assert.Contains(bandage, player.Inventory.Items);
        Assert.Equal(0, game.TurnCount);
        Assert.Contains("BANDAGE NOT NEEDED", game.EventLog.Entries);
    }

    [Fact]
    public void BandageDoesNotRemoveStunned()
    {
        var player = new Player();
        player.ApplyStatus(StatusEffectType.Stunned, 2, source: "TEST");
        player.ApplyStatus(StatusEffectType.Bleeding, 2, 1, "TEST");
        Item bandage = ItemFactory.Create(ItemType.Bandage);
        player.Inventory.TryAdd(bandage);

        Assert.Equal(PlayerItemActionResult.Success,
            player.UseSelectedBandage(out _, out _));

        Assert.True(player.StatusEffects.Has(StatusEffectType.Stunned));
        Assert.False(player.StatusEffects.Has(StatusEffectType.Bleeding));
    }

    private static Map CreateMap()
    {
        var map = new Map(123);
        map.NPCs.Clear();
        return map;
    }
}
