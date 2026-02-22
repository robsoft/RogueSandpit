using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace RogueSandpit.Models;

// need to write helpers for;
// 1) generate a name (markov chain video about character name generation)
// 2) generate attributes like aggression, hp, damage etc - needs some thought & inspiration
// 3) helper methods for targetting things, returning 'home' etc


public static class NPCFactory
{
    public static BaseNPC CreateNPC(Map map,CharacterTypes type, int x, int y, BaseMapElement currentRoom)
    {
        switch (type)
        {
            case CharacterTypes.Orc:
                return new Orc(map, x, y, currentRoom);
            case CharacterTypes.Goblin:
                return new Goblin(map, x, y, currentRoom);
            case CharacterTypes.Skeleton:
                return new Skeleton(map, x, y, currentRoom);
            default:
                throw new ArgumentException("Invalid character type");
        }
    }
}

public class Orc : BaseNPC
{
    public Orc(Map map, int x, int y, BaseMapElement currentRoom): base(map, x, y, currentRoom)
    {
        Description = "Orc";
        Name = "Orc";
        Damage = 5 + RandGen.RandInt(0, 10);
        HP = 30 + RandGen.RandInt(0, 10);
    }
}

public class Goblin : BaseNPC
{
    public Goblin(Map map, int x, int y, BaseMapElement currentRoom): base(map, x, y, currentRoom)
    {
        Description = "Goblin";
        Name = "Goblin";
        Damage = 10 + RandGen.RandInt(0, 10);
        HP = 30 + RandGen.RandInt(0, 10);
    }
}

public class Skeleton : BaseNPC
{
    public Skeleton(Map map, int x, int y, BaseMapElement currentRoom): base(map, x, y, currentRoom)
    {
        Description = "Skeleton";
        Name = "Skeleton";
        Damage = 5 + RandGen.RandInt(0, 10);
        HP = 40 + RandGen.RandInt(0, 10);
    }
}



