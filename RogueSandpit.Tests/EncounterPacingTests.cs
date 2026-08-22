using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class EncounterPacingTests
{
    [Fact]
    public void ObjectiveRoomHasGuardsAndNoNpcStartsInProtectedApproachAcrossManySeeds()
    {
        for (int seed = 0; seed < 100; seed++)
        {
            var map = new Map(seed);
            Room objectiveRoom = Assert.Single(map.RoomList, room => room.Specials.Count > 0);
            int guardCount = map.NPCs.Count(npc =>
                map.MapCells[npc.X, npc.Y].ParentElement == objectiveRoom);

            Assert.True(guardCount >= 2,
                $"Seed {seed} generated only {guardCount} objective-room guards.");
            Assert.All(map.NPCs, npc => Assert.True(
                map.GetEntranceDistance(npc.X, npc.Y) > Map.EntranceSafetyDistance,
                $"Seed {seed} placed {npc.Name} at entrance distance {map.GetEntranceDistance(npc.X, npc.Y)}."));
        }
    }

    [Fact]
    public void DeepPopulationIsStrongerThanShallowPopulationAcrossRepresentativeSeeds()
    {
        int shallowTotal = 0;
        int shallowStrong = 0;
        int deepTotal = 0;
        int deepStrong = 0;

        for (int seed = 0; seed < 100; seed++)
        {
            var map = new Map(seed);
            foreach (BaseNPC npc in map.NPCs)
            {
                GenerationDepthBand band = map.GetDepthBand(npc.X, npc.Y);
                bool strong = npc.CharacterType is CharacterTypes.Orc or CharacterTypes.Troll;
                if (band == GenerationDepthBand.Shallow)
                {
                    shallowTotal++;
                    if (strong) shallowStrong++;
                    Assert.NotEqual(CharacterTypes.Troll, npc.CharacterType);
                }
                else if (band == GenerationDepthBand.Deep)
                {
                    deepTotal++;
                    if (strong) deepStrong++;
                }
            }
        }

        Assert.True(shallowTotal > 0 && deepTotal > 0);
        Assert.True((double)deepStrong / deepTotal > (double)shallowStrong / shallowTotal,
            $"Deep strong ratio {deepStrong}/{deepTotal} did not exceed shallow {shallowStrong}/{shallowTotal}.");
    }

    [Fact]
    public void GeneratedRewardsFollowDepthBandsAndEquipmentTiersAcrossManySeeds()
    {
        for (int seed = 0; seed < 100; seed++)
        {
            var map = new Map(seed);
            GroundItem healing = Assert.Single(map.GroundItems, item =>
                item.Item.Type == ItemType.HealingPotion);
            GroundItem weapon = Assert.Single(map.GroundItems, item =>
                item.Item.Type == ItemType.Weapon);
            GroundItem armor = Assert.Single(map.GroundItems, item =>
                item.Item.Type == ItemType.Armor);
            GroundItem trap = Assert.Single(map.GroundItems, item =>
                item.Item.Type == ItemType.Trap);
            GroundItem ranged = Assert.Single(map.GroundItems, item =>
                item.Item.Type == ItemType.RangedWeapon);
            GroundItem deepConsumable = Assert.Single(map.GroundItems, item =>
                item.Item.Type is ItemType.Bandage or ItemType.SmokeBomb or ItemType.FireBomb);

            AssertPlacement(map, healing, GenerationDepthBand.Shallow, 0, seed);
            AssertPlacement(map, weapon, GenerationDepthBand.Shallow, 1, seed);
            AssertPlacement(map, armor, GenerationDepthBand.Middle, 2, seed);
            AssertPlacement(map, trap, GenerationDepthBand.Middle, 0, seed);
            AssertPlacement(map, ranged, GenerationDepthBand.Deep, 3, seed);
            AssertPlacement(map, deepConsumable, GenerationDepthBand.Deep, 0, seed);

            foreach (BaseNPC npc in map.NPCs.Where(npc => npc.HeldItem?.Tier > 0))
            {
                Assert.Equal((int)map.GetDepthBand(npc.X, npc.Y) + 1, npc.HeldItem.Tier);
            }
        }
    }

    private static void AssertPlacement(Map map, GroundItem item, GenerationDepthBand expectedBand,
        int expectedTier, int seed)
    {
        Assert.True(map.GetDepthBand(item.X, item.Y) == expectedBand,
            $"Seed {seed} placed {item.Item.Name} in {map.GetDepthBand(item.X, item.Y)} rather than {expectedBand}.");
        Assert.Equal(expectedTier, item.Item.Tier);
    }
}
