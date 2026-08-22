using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public static class RandGen
{
    public static int Seed { get; private set; } = 0;

    private const ulong DefaultState = 0x9E3779B97F4A7C15UL;
    private static ulong _state = DefaultState;

    public static void SetSeed(int seed)
    {
        Seed = seed;
        _state = DefaultState ^ unchecked((uint)seed);
        NextUInt64();
    }

    public static int RandInt(int min, int max)
    {
        if (min > max) (min, max) = (max, min);
        if (min == max) return min;
        return min + (int)(NextUInt64() % (uint)(max - min));
    }

    public static float RandFloat(float min, float max)
    {
        double unit = (NextUInt64() >> 11) * (1.0 / (1UL << 53));
        return (float)(unit * (max - min) + min);
    }

    public static ulong CaptureState() => _state;

    public static void RestoreState(int seed, ulong state)
    {
        Seed = seed;
        _state = state == 0 ? DefaultState : state;
    }

    private static ulong NextUInt64()
    {
        ulong value = _state;
        value ^= value >> 12;
        value ^= value << 25;
        value ^= value >> 27;
        _state = value;
        return value * 2685821657736338717UL;
    }

}
