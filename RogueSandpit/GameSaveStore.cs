using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RogueSandpit.Models;

namespace RogueSandpit;

public sealed record SaveGameResult(bool Success, string Message, GameState Game = null,
    bool RealtimeMode = false);

public sealed class GameSaveStore
{
    private readonly string _path;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RogueSandpit", "save-game.json");

    public GameSaveStore(string path) => _path = path;

    public SaveGameResult Save(GameState game, bool realtimeMode = false)
    {
        string temporaryPath = _path + ".tmp";
        try
        {
            string directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            SaveGameSnapshot snapshot = SaveGameSnapshot.Capture(game, realtimeMode);
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(snapshot, JsonOptions));
            File.Move(temporaryPath, _path, true);
            return new SaveGameResult(true, "GAME SAVED");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
            or JsonException or NotSupportedException or InvalidOperationException)
        {
            TryDeleteTemporary(temporaryPath);
            return new SaveGameResult(false, "SAVE FAILED");
        }
    }

    public SaveGameResult Load()
    {
        if (!File.Exists(_path)) return new SaveGameResult(false, "NO SAVED GAME");
        try
        {
            SaveGameSnapshot snapshot = JsonSerializer.Deserialize<SaveGameSnapshot>(
                File.ReadAllText(_path), JsonOptions);
            if (snapshot == null) throw new JsonException("Save document was empty.");
            GameState game = snapshot.Restore();
            return new SaveGameResult(true, "GAME LOADED", game, snapshot.RealtimeMode);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
            or JsonException or NotSupportedException or InvalidOperationException
            or ArgumentException or KeyNotFoundException or NullReferenceException
            or IndexOutOfRangeException or OverflowException)
        {
            return new SaveGameResult(false, "SAVE FILE INVALID");
        }
    }

    private static void TryDeleteTemporary(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
