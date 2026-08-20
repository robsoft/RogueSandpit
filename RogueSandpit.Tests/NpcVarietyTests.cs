using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class NpcVarietyTests
{
    [Fact]
    public void FactoryCreatesAllFiveArchetypesWithExpectedProfiles()
    {
        var map = new Map(123);

        BaseNPC orc = NPCFactory.CreateNPC(map, CharacterTypes.Orc, 1, 1, null);
        BaseNPC goblin = NPCFactory.CreateNPC(map, CharacterTypes.Goblin, 1, 1, null);
        BaseNPC skeleton = NPCFactory.CreateNPC(map, CharacterTypes.Skeleton, 1, 1, null);
        BaseNPC troll = NPCFactory.CreateNPC(map, CharacterTypes.Troll, 1, 1, null);
        BaseNPC wretch = NPCFactory.CreateNPC(map, CharacterTypes.Wretch, 1, 1, null);

        Assert.IsType<Orc>(orc);
        Assert.IsType<Goblin>(goblin);
        Assert.IsType<Skeleton>(skeleton);
        Assert.IsType<Troll>(troll);
        Assert.IsType<Wretch>(wretch);
        Assert.InRange(orc.HP, 35, 45);
        Assert.InRange(orc.Damage, 10, 15);
        Assert.InRange(goblin.HP, 20, 28);
        Assert.InRange(goblin.Damage, 7, 12);
        Assert.InRange(skeleton.HP, 28, 36);
        Assert.InRange(skeleton.Damage, 8, 13);
        Assert.InRange(troll.HP, 55, 70);
        Assert.InRange(troll.Damage, 12, 18);
        Assert.InRange(wretch.HP, 15, 22);
        Assert.InRange(wretch.Damage, 4, 8);
    }

    [Fact]
    public void GeneratedNpcIdentityIsDeterministicForMapSeed()
    {
        var firstMap = new Map(456);
        var first = firstMap.NPCs
            .Select(npc => (npc.CharacterType, npc.Name, npc.HP, npc.Damage, npc.X, npc.Y))
            .ToList();

        var secondMap = new Map(456);
        var second = secondMap.NPCs
            .Select(npc => (npc.CharacterType, npc.Name, npc.HP, npc.Damage, npc.X, npc.Y))
            .ToList();

        Assert.Equal(first, second);
        Assert.All(first, identity => Assert.Contains(' ', identity.Name));
    }

    [Fact]
    public void GeneratedMapsUseEveryArchetypeAcrossRepresentativeSeeds()
    {
        var generatedTypes = new HashSet<CharacterTypes>();

        for (int seed = 0; seed < 20; seed++)
        {
            generatedTypes.UnionWith(new Map(seed).NPCs.Select(npc => npc.CharacterType));
        }

        Assert.Equal(Enum.GetValues<CharacterTypes>().Order(), generatedTypes.Order());
    }
}
