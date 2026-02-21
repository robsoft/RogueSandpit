using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

// need to write helpers for;
// 1) generate a name (markov chain video about character name generation)
// 2) generate attributes like agression, hp, damage etc - needs some thought & inspiration
// 3) helper methods for targetting things, returning 'home' etc


public class Orc : BaseNPC
{
    public Orc()
    {
        Description = "Orc";
        Name = "Orc";
    }
}

public class Goblin : BaseNPC
{
    public Goblin()
    {
        Description = "Goblin";
        Name = "Goblin";
    }
}




