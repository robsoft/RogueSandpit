using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RogueSandpit;

public sealed record LoadedSettings(RuntimeSettings Runtime, InputBindings Bindings);

public sealed class SettingsStore
{
    private readonly string _path;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RogueSandpit", "settings.json");

    public SettingsStore(string path)
    {
        _path = path;
    }

    public LoadedSettings Load(double defaultRealtimeSeconds)
    {
        var bindings = new InputBindings();
        if (!File.Exists(_path)) return new LoadedSettings(
            new RuntimeSettings(defaultRealtimeSeconds), bindings);

        try
        {
            SettingsDocument document = JsonSerializer.Deserialize<SettingsDocument>(
                File.ReadAllText(_path), JsonOptions);
            if (document == null) throw new JsonException("Settings document was empty.");

            bindings.Import(document.Bindings);
            return new LoadedSettings(new RuntimeSettings(
                document.RealtimeTurnSeconds ?? defaultRealtimeSeconds,
                document.MasterVolume ?? 100,
                document.EffectsVolume ?? 100,
                document.MusicVolume ?? 100,
                document.MuteWhileUnfocused ?? true), bindings);
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException or JsonException or NotSupportedException)
        {
            return new LoadedSettings(new RuntimeSettings(defaultRealtimeSeconds), bindings);
        }
    }

    public bool Save(RuntimeSettings runtime, InputBindings bindings)
    {
        try
        {
            string directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            var document = new SettingsDocument
            {
                RealtimeTurnSeconds = runtime.RealtimeTurnSeconds,
                MasterVolume = runtime.MasterVolume,
                EffectsVolume = runtime.EffectsVolume,
                MusicVolume = runtime.MusicVolume,
                MuteWhileUnfocused = runtime.MuteWhileUnfocused,
                Bindings = bindings.Export()
            };
            File.WriteAllText(_path, JsonSerializer.Serialize(document, JsonOptions));
            return true;
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException or NotSupportedException)
        {
            return false;
        }
    }

    private sealed class SettingsDocument
    {
        public double? RealtimeTurnSeconds { get; set; }
        public int? MasterVolume { get; set; }
        public int? EffectsVolume { get; set; }
        public int? MusicVolume { get; set; }
        public bool? MuteWhileUnfocused { get; set; }
        public Dictionary<string, string[]> Bindings { get; set; } = [];
    }
}
