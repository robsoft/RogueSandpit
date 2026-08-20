using System;
using System.Collections.Generic;

namespace RogueSandpit.Models;

public static class NPCFactory
{
    public static BaseNPC CreateNPC(Map map, CharacterTypes type, int x, int y, BaseContainingElement currentRoom)
    {
        return type switch
        {
            CharacterTypes.Orc => new Orc(map, x, y, currentRoom),
            CharacterTypes.Goblin => new Goblin(map, x, y, currentRoom),
            CharacterTypes.Skeleton => new Skeleton(map, x, y, currentRoom),
            CharacterTypes.Troll => new Troll(map, x, y, currentRoom),
            CharacterTypes.Wretch => new Wretch(map, x, y, currentRoom),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}

public static class NPCNameGenerator
{
    private static readonly Dictionary<CharacterTypes, (string[] Given, string[] Byname)> Parts = new()
    {
        [CharacterTypes.Orc] =
            (["Grak", "Morga", "Thruk", "Varka", "Drog"], ["Bonegnaw", "Ironjaw", "Redhand", "Stonefang", "Ashscar"]),
        [CharacterTypes.Goblin] =
            (["Nib", "Skrik", "Wizzle", "Grit", "Pock"], ["Quickfinger", "Muckfoot", "Ratcatcher", "Nailbite", "Coppernose"]),
        [CharacterTypes.Skeleton] =
            (["Aldric", "Morrow", "Severin", "Vesper", "Ossian"], ["the Hollow", "Dustwalker", "Gravebound", "the Rattling", "Pale-Eye"]),
        [CharacterTypes.Troll] =
            (["Brug", "Hrolda", "Mugrum", "Torga", "Uld"], ["Bogback", "Rockhide", "Mossbeard", "Bridge-Breaker", "Mudblood"]),
        [CharacterTypes.Wretch] =
            (["Grel", "Pella", "Siv", "Tarn", "Wim"], ["the Bent", "Ragcloak", "Soreskin", "the Hungry", "Cinder-Eye"])
    };

    public static string Generate(CharacterTypes type)
    {
        (string[] given, string[] byname) = Parts[type];
        return $"{given[RandGen.RandInt(0, given.Length)]} {byname[RandGen.RandInt(0, byname.Length)]}";
    }
}

public class Orc : BaseNPC
{
    public Orc(Map map, int x, int y, BaseContainingElement currentRoom) : base(map, x, y, currentRoom)
    {
        CharacterType = CharacterTypes.Orc;
        Description = "Orc";
        Name = NPCNameGenerator.Generate(CharacterType);
        Damage = RandGen.RandInt(10, 16);
        HP = RandGen.RandInt(35, 46);
    }
}

public class Goblin : BaseNPC
{
    public Goblin(Map map, int x, int y, BaseContainingElement currentRoom) : base(map, x, y, currentRoom)
    {
        CharacterType = CharacterTypes.Goblin;
        Description = "Goblin";
        Name = NPCNameGenerator.Generate(CharacterType);
        Damage = RandGen.RandInt(7, 13);
        HP = RandGen.RandInt(20, 29);
    }
}

public class Skeleton : BaseNPC
{
    public Skeleton(Map map, int x, int y, BaseContainingElement currentRoom) : base(map, x, y, currentRoom)
    {
        CharacterType = CharacterTypes.Skeleton;
        Description = "Skeleton";
        Name = NPCNameGenerator.Generate(CharacterType);
        Damage = RandGen.RandInt(8, 14);
        HP = RandGen.RandInt(28, 37);
    }
}

public class Troll : BaseNPC
{
    public Troll(Map map, int x, int y, BaseContainingElement currentRoom) : base(map, x, y, currentRoom)
    {
        CharacterType = CharacterTypes.Troll;
        Description = "Troll";
        Name = NPCNameGenerator.Generate(CharacterType);
        Damage = RandGen.RandInt(12, 19);
        HP = RandGen.RandInt(55, 71);
    }
}

public class Wretch : BaseNPC
{
    public Wretch(Map map, int x, int y, BaseContainingElement currentRoom) : base(map, x, y, currentRoom)
    {
        CharacterType = CharacterTypes.Wretch;
        Description = "Wretch";
        Name = NPCNameGenerator.Generate(CharacterType);
        Damage = RandGen.RandInt(4, 9);
        HP = RandGen.RandInt(15, 23);
    }
}
