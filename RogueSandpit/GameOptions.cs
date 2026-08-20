using System;

namespace RogueSandpit;

public sealed class GameOptions
{
    public const int DefaultWindowScale = 2;
    public int WindowScale { get; }

    private GameOptions(int windowScale)
    {
        WindowScale = windowScale;
    }

    public static GameOptions Parse(string[] args)
    {
        int windowScale = DefaultWindowScale;

        for (int i = 0; i < args.Length; i++)
        {
            string argument = args[i];
            string value;
            if (argument == "--scale")
            {
                if (++i >= args.Length) throw new ArgumentException("--scale requires a value from 1 to 4.");
                value = args[i];
            }
            else if (argument.StartsWith("--scale="))
            {
                value = argument["--scale=".Length..];
            }
            else
            {
                throw new ArgumentException($"Unknown argument: {argument}");
            }

            if (!int.TryParse(value, out windowScale) || windowScale is < 1 or > 4)
            {
                throw new ArgumentException("--scale must be an integer from 1 to 4.");
            }
        }

        return new GameOptions(windowScale);
    }
}
