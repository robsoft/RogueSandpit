using System;

try
{
    RogueSandpit.GameOptions options = RogueSandpit.GameOptions.Parse(args);
    using var game = new RogueSandpit.GameWrapper(options.WindowScale, options.TurnSeconds,
        options.Fullscreen, options.StartRealtime);
    game.Run();
}
catch (ArgumentException exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}
