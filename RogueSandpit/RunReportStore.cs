using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RogueSandpit.Models;

namespace RogueSandpit;

public sealed record RunReportResult(bool Success, string Message, string FileName = null);

public sealed class CompletedRunReport
{
    public const int CurrentVersion = 1;
    public int Version { get; init; } = CurrentVersion;
    public DateTimeOffset CompletedAtUtc { get; init; }
    public int Seed { get; init; }
    public GameOutcome Outcome { get; init; }
    public bool RealtimeMode { get; init; }
    public double RealtimeTurnSeconds { get; init; }
    public int PlayerHealth { get; init; }
    public int PlayerMaxHealth { get; init; }
    public bool ObjectiveCarried { get; init; }
    public List<string> Inventory { get; init; } = [];
    public string EquippedWeapon { get; init; }
    public string EquippedArmor { get; init; }
    public string EquippedRangedWeapon { get; init; }
    public List<StatusEffectSnapshot> PlayerStatusEffects { get; init; } = [];
    public RunStatisticsSnapshot Statistics { get; init; }

    public static CompletedRunReport Capture(GameState game, DateTimeOffset completedAtUtc,
        bool realtimeMode, double realtimeTurnSeconds) => new()
    {
        CompletedAtUtc = completedAtUtc,
        Seed = game.Map.Seed,
        Outcome = game.Outcome,
        RealtimeMode = realtimeMode,
        RealtimeTurnSeconds = realtimeTurnSeconds,
        PlayerHealth = game.Player.Health,
        PlayerMaxHealth = game.Player.MaxHealth,
        ObjectiveCarried = game.Player.HasSpecial,
        Inventory = game.Player.Inventory.Items.Select(item => item.Name).ToList(),
        EquippedWeapon = game.Player.EquippedWeapon?.Name,
        EquippedArmor = game.Player.EquippedArmor?.Name,
        EquippedRangedWeapon = game.Player.EquippedRangedWeapon?.Name,
        PlayerStatusEffects = game.Player.StatusEffects.Effects.Select(StatusEffectSnapshot.From).ToList(),
        Statistics = RunStatisticsSnapshot.From(game.Statistics)
    };
}

public sealed class RunReportStore
{
    private readonly string _directory;
    private readonly Func<DateTimeOffset> _clock;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string DefaultDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RogueSandpit", "run-reports");

    public RunReportStore(string directory, Func<DateTimeOffset> clock = null)
    {
        _directory = directory;
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public RunReportResult Save(GameState game, bool realtimeMode, double realtimeTurnSeconds)
    {
        if (game?.Outcome == GameOutcome.Playing)
            return new RunReportResult(false, "RUN STILL ACTIVE");
        try
        {
            Directory.CreateDirectory(_directory);
            DateTimeOffset completedAt = _clock();
            string stem = $"run-{completedAt:yyyyMMdd-HHmmssfff}-seed-{game.Map.Seed}";
            string fileName = stem + ".json";
            string path = Path.Combine(_directory, fileName);
            int suffix = 2;
            while (File.Exists(path))
            {
                fileName = $"{stem}-{suffix++}.json";
                path = Path.Combine(_directory, fileName);
            }
            CompletedRunReport report = CompletedRunReport.Capture(game, completedAt,
                realtimeMode, realtimeTurnSeconds);
            File.WriteAllText(path, JsonSerializer.Serialize(report, JsonOptions));
            return new RunReportResult(true, "RUN REPORT SAVED", fileName);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
            or JsonException or NotSupportedException)
        {
            return new RunReportResult(false, "RUN REPORT FAILED");
        }
    }
}
