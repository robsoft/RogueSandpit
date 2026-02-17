using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public static class RandGen
{
    private static Random _rand = new Random();

    public static int RandInt(int min, int max)
    {
        if (min <= max)
            return _rand.Next(min, max);
        else
            return _rand.Next(max, min);
    }

    public static float RandFloat(float min, float max)
    {
        return (float)(_rand.NextDouble() * (max - min) + min);
    }

}
