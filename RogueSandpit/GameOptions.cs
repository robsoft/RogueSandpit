using System;

namespace RogueSandpit;

public sealed class GameOptions
{
    public const int DefaultWindowScale = 2;
    public const double DefaultTurnSeconds = 1.0;
    public int WindowScale { get; }
    public double TurnSeconds { get; }

    private GameOptions(int windowScale, double turnSeconds)
    {
        WindowScale = windowScale;
        TurnSeconds = turnSeconds;
    }

    public static GameOptions Parse(string[] args)
    {
        int windowScale = DefaultWindowScale;
        double turnSeconds = DefaultTurnSeconds;

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
            else if (argument == "--turn-seconds")
            {
                if (++i >= args.Length) throw new ArgumentException("--turn-seconds requires a value from 0.1 to 10.");
                value = args[i];
                if (!double.TryParse(value, out turnSeconds) || turnSeconds is < 0.1 or > 10)
                    throw new ArgumentException("--turn-seconds must be a number from 0.1 to 10.");
                continue;
            }
            else if (argument.StartsWith("--turn-seconds="))
            {
                value = argument["--turn-seconds=".Length..];
                if (!double.TryParse(value, out turnSeconds) || turnSeconds is < 0.1 or > 10)
                    throw new ArgumentException("--turn-seconds must be a number from 0.1 to 10.");
                continue;
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

        return new GameOptions(windowScale, turnSeconds);
    }
}
