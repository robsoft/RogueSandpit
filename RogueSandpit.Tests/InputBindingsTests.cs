using Microsoft.Xna.Framework.Input;
using Xunit;

namespace RogueSandpit.Tests;

public class InputBindingsTests
{
    [Fact]
    public void WaitDefaultsSupportLaptopAndFullKeyboardKeys()
    {
        var bindings = new InputBindings();

        Assert.Equal(new[] { Keys.Space, Keys.OemPeriod, Keys.NumPad5 },
            bindings.GetKeys(InputAction.Wait));
    }

    [Fact]
    public void RemapRejectsConflictAndReservedKey()
    {
        var bindings = new InputBindings();

        Assert.False(bindings.TrySet(InputAction.Inventory, 0, Keys.E, out InputAction? conflict));
        Assert.Equal(InputAction.Equip, conflict);
        Assert.False(bindings.TrySet(InputAction.Inventory, 0, Keys.Escape, out _));
        Assert.Equal(Keys.I, bindings.GetKeys(InputAction.Inventory)[0]);
    }

    [Fact]
    public void RemapClearAndResetChangeOnlyRequestedSlots()
    {
        var bindings = new InputBindings();

        Assert.True(bindings.TrySet(InputAction.Inventory, 0, Keys.V, out _));
        Assert.True(bindings.TrySet(InputAction.Inventory, 1, Keys.N, out _));
        Assert.True(bindings.ClearSecondary(InputAction.Inventory));
        Assert.Equal(new[] { Keys.V }, bindings.GetKeys(InputAction.Inventory));

        bindings.Reset(InputAction.Inventory);
        Assert.Equal(new[] { Keys.I }, bindings.GetKeys(InputAction.Inventory));
    }

    [Fact]
    public void ImportIgnoresInvalidReservedAndDuplicateEntries()
    {
        var bindings = new InputBindings();
        var saved = new Dictionary<string, string[]>
        {
            [nameof(InputAction.Inventory)] = [nameof(Keys.V), nameof(Keys.Escape)],
            [nameof(InputAction.Equip)] = [nameof(Keys.V)],
            ["FutureAction"] = [nameof(Keys.Z)]
        };

        bindings.Import(saved);

        Assert.Equal(new[] { Keys.V }, bindings.GetKeys(InputAction.Inventory));
        Assert.Equal(new[] { Keys.E }, bindings.GetKeys(InputAction.Equip));
    }
}
