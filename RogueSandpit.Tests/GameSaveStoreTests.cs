using System.Text.Json;
using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class GameSaveStoreTests
{
    [Fact]
    public void SaveAndLoadRoundTripRestoresRepresentativeRunState()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string path = Path.Combine(directory, "save-game.json");
            var map = new Map(54321);
            var player = new Player();
            player.Place(map, map.StartPosX, map.StartPosY);
            player.TryCollectItem(ItemFactory.CreateEquipment(ItemType.Weapon, 2), out _);
            player.ApplyStatus(StatusEffectType.Bleeding, 3, 2, "TEST");
            var game = new GameState(map, player);
            game.Update(PlayerCommand.Wait);

            Doorway door = map.Doors.First();
            door.State = DoorState.Open;
            BaseNPC npc = map.NPCs.First();
            npc.ReceiveInvestigation((player.X, player.Y), NPCInvestigationSource.Noise);
            map.PlacedTraps.Add(new PlacedTrap(player.X, player.Y - 1, 7, TrapKind.Snare));
            map.AddEnvironmentalEffect(EnvironmentalEffectType.Smoke, player.X, player.Y, 3);

            var store = new GameSaveStore(path);
            Assert.True(store.Save(game, realtimeMode: true).Success);
            SaveGameResult result = store.Load();

            Assert.True(result.Success);
            Assert.True(result.RealtimeMode);
            GameState restored = result.Game;
            Assert.Equal(map.Seed, restored.Map.Seed);
            Assert.Equal(game.TurnCount, restored.TurnCount);
            Assert.Equal(player.Health, restored.Player.Health);
            Assert.Equal(player.Inventory.Items.Single().Name, restored.Player.Inventory.Items.Single().Name);
            Assert.Equal(player.EquippedWeapon?.Name, restored.Player.EquippedWeapon?.Name);
            Assert.True(restored.Player.StatusEffects.Has(StatusEffectType.Bleeding));
            Assert.Equal(DoorState.Open, restored.Map.GetDoorAt(door.X1, door.Y1).State);
            Assert.Equal(map.NPCs.Count, restored.Map.NPCs.Count);
            BaseNPC restoredNpc = restored.Map.NPCs.Single(candidate => candidate.Id == npc.Id);
            Assert.Equal(npc.Awareness, restoredNpc.Awareness);
            Assert.Equal(npc.InvestigationOrigin, restoredNpc.InvestigationOrigin);
            Assert.Single(restored.Map.PlacedTraps);
            Assert.Single(restored.Map.EnvironmentalEffects);
            Assert.Equal(game.Statistics.Turns, restored.Statistics.Turns);
            Assert.Equal(game.EventLog.Entries, restored.EventLog.Entries);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void RestoredGameContinuesTheSameRandomSequence()
    {
        var map = new Map(9988);
        var player = new Player();
        player.Place(map, map.StartPosX, map.StartPosY);
        var game = new GameState(map, player);
        SaveGameSnapshot snapshot = SaveGameSnapshot.Capture(game);

        int[] expected = Enumerable.Range(0, 12).Select(_ => RandGen.RandInt(0, 10000)).ToArray();
        snapshot.Restore();
        int[] actual = Enumerable.Range(0, 12).Select(_ => RandGen.RandInt(0, 10000)).ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MissingAndMalformedSavesFailWithoutThrowing()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string path = Path.Combine(directory, "save-game.json");
            var store = new GameSaveStore(path);
            Assert.Equal("NO SAVED GAME", store.Load().Message);

            File.WriteAllText(path, "{ definitely not json }");
            SaveGameResult malformed = store.Load();
            Assert.False(malformed.Success);
            Assert.Equal("SAVE FILE INVALID", malformed.Message);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void UnsupportedSaveVersionFailsSafely()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string path = Path.Combine(directory, "save-game.json");
            var snapshot = new SaveGameSnapshot { Version = SaveGameSnapshot.CurrentVersion + 1 };
            File.WriteAllText(path, JsonSerializer.Serialize(snapshot));

            SaveGameResult result = new GameSaveStore(path).Load();

            Assert.False(result.Success);
            Assert.Equal("SAVE FILE INVALID", result.Message);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"RogueSandpitSaveTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
