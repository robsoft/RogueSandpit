using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace RogueSandpit;

public enum InputAction
{
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Wait,
    SelectPreviousItem,
    SelectNextItem,
    Inventory,
    Equip,
    UsePotion,
    UseBandage,
    Drop,
    ToggleDoor,
    LayFalseTrail,
    ThrowItem,
    PlaceTrap,
    FireRanged
}

public sealed class InputBindings
{
    private static readonly IReadOnlyDictionary<InputAction, Keys[]> Defaults =
        new Dictionary<InputAction, Keys[]>
        {
            [InputAction.MoveUp] = [Keys.Up],
            [InputAction.MoveDown] = [Keys.Down],
            [InputAction.MoveLeft] = [Keys.Left],
            [InputAction.MoveRight] = [Keys.Right],
            [InputAction.Wait] = [Keys.Space, Keys.OemPeriod, Keys.NumPad5],
            [InputAction.SelectPreviousItem] = [Keys.OemOpenBrackets],
            [InputAction.SelectNextItem] = [Keys.OemCloseBrackets],
            [InputAction.Inventory] = [Keys.I],
            [InputAction.Equip] = [Keys.E],
            [InputAction.UsePotion] = [Keys.H],
            [InputAction.UseBandage] = [Keys.B],
            [InputAction.Drop] = [Keys.D],
            [InputAction.ToggleDoor] = [Keys.C],
            [InputAction.LayFalseTrail] = [Keys.T],
            [InputAction.ThrowItem] = [Keys.F],
            [InputAction.PlaceTrap] = [Keys.P],
            [InputAction.FireRanged] = [Keys.R]
        };

    private readonly Dictionary<InputAction, List<Keys>> _bindings;

    public InputBindings()
    {
        _bindings = Defaults.ToDictionary(pair => pair.Key, pair => pair.Value.ToList());
    }

    public IReadOnlyList<Keys> GetKeys(InputAction action) => _bindings[action];

    public bool IsPressed(InputAction action, KeyboardState current, KeyboardState previous) =>
        _bindings[action].Any(key => current.IsKeyDown(key) && previous.IsKeyUp(key));

    public bool TrySet(InputAction action, int slot, Keys key, out InputAction? conflict)
    {
        conflict = FindConflict(action, key);
        if (conflict.HasValue || slot is < 0 or > 1 || IsReserved(key)) return false;

        List<Keys> keys = _bindings[action];
        if (keys.Where((_, index) => index != slot).Contains(key)) return false;
        while (keys.Count <= slot) keys.Add(Keys.None);
        keys[slot] = key;
        keys.RemoveAll(candidate => candidate == Keys.None);
        if (keys.Count > 2) keys.RemoveRange(2, keys.Count - 2);
        return true;
    }

    public bool ClearSecondary(InputAction action)
    {
        List<Keys> keys = _bindings[action];
        if (keys.Count < 2) return false;
        keys.RemoveRange(1, keys.Count - 1);
        return true;
    }

    public void Reset(InputAction action) => _bindings[action] = Defaults[action].ToList();

    public void ResetAll()
    {
        foreach (InputAction action in Enum.GetValues<InputAction>()) Reset(action);
    }

    public Dictionary<string, string[]> Export() => _bindings.ToDictionary(
        pair => pair.Key.ToString(), pair => pair.Value.Select(key => key.ToString()).ToArray());

    public void Import(IReadOnlyDictionary<string, string[]> saved)
    {
        if (saved == null) return;
        var used = _bindings.Values.SelectMany(keys => keys).ToHashSet();

        foreach ((string actionName, string[] keyNames) in saved)
        {
            if (!Enum.TryParse(actionName, out InputAction action) || keyNames == null) continue;
            List<Keys> original = _bindings[action];
            foreach (Keys key in original) used.Remove(key);

            var keys = new List<Keys>();
            foreach (string keyName in keyNames)
            {
                if (!Enum.TryParse(keyName, out Keys key) || key == Keys.None
                    || IsReserved(key) || !used.Add(key)) continue;
                keys.Add(key);
            }

            if (keys.Count > 0) _bindings[action] = keys;
            else foreach (Keys key in original) used.Add(key);
        }
    }

    public static bool IsReserved(Keys key) => key is Keys.Escape or Keys.Enter
        or Keys.F1 or Keys.F11 or Keys.F12;

    public static int? InventorySlotForKey(Keys key) => key switch
    {
        Keys.D1 or Keys.NumPad1 => 0,
        Keys.D2 or Keys.NumPad2 => 1,
        Keys.D3 or Keys.NumPad3 => 2,
        Keys.D4 or Keys.NumPad4 => 3,
        Keys.D5 or Keys.NumPad5 => 4,
        Keys.D6 or Keys.NumPad6 => 5,
        Keys.D7 or Keys.NumPad7 => 6,
        Keys.D8 or Keys.NumPad8 => 7,
        _ => null
    };

    private InputAction? FindConflict(InputAction action, Keys key)
    {
        foreach ((InputAction candidate, List<Keys> keys) in _bindings)
        {
            if (candidate != action && keys.Contains(key)) return candidate;
        }
        return null;
    }
}
