using System;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace RogueSandpit.Tests;

public class SettingsStoreTests
{
    [Fact]
    public void SaveAndLoadRoundTripRuntimeAndBindings()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"rogue-settings-{Guid.NewGuid():N}");
        string path = Path.Combine(directory, "settings.json");
        try
        {
            var store = new SettingsStore(path);
            var runtime = new RuntimeSettings(1.7, 80, 70, 60, false);
            var bindings = new InputBindings();
            Assert.True(bindings.TrySet(InputAction.Inventory, 0, Keys.V, out _));

            Assert.True(store.Save(runtime, bindings));
            LoadedSettings loaded = store.Load(1.0);

            Assert.Equal(1.7, loaded.Runtime.RealtimeTurnSeconds);
            Assert.Equal(80, loaded.Runtime.MasterVolume);
            Assert.False(loaded.Runtime.MuteWhileUnfocused);
            Assert.Equal(Keys.V, loaded.Bindings.GetKeys(InputAction.Inventory)[0]);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void CorruptFileFallsBackToDefaults()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "{ definitely not json");

            LoadedSettings loaded = new SettingsStore(path).Load(2.3);

            Assert.Equal(2.3, loaded.Runtime.RealtimeTurnSeconds);
            Assert.Equal(Keys.I, loaded.Bindings.GetKeys(InputAction.Inventory)[0]);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
