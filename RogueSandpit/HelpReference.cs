using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace RogueSandpit;

public sealed record HelpRow(string Section, string Action, string Keys);

public static class HelpReference
{
    public static IReadOnlyList<HelpRow> Build(InputBindings bindings)
    {
        HelpRow Row(string section, string action, InputAction input) =>
            new(section, action, string.Join(" / ", bindings.GetKeys(input).Select(KeyLabel)));
        return
        [
        Row("MOVEMENT", "MOVE UP", InputAction.MoveUp),
        Row("MOVEMENT", "MOVE DOWN", InputAction.MoveDown),
        Row("MOVEMENT", "MOVE LEFT", InputAction.MoveLeft),
        Row("MOVEMENT", "MOVE RIGHT", InputAction.MoveRight),
        Row("MOVEMENT", "WAIT", InputAction.Wait),
        Row("INVENTORY", "OPEN INVENTORY", InputAction.Inventory),
        Row("INVENTORY", "SELECT PREVIOUS", InputAction.SelectPreviousItem),
        Row("INVENTORY", "SELECT NEXT", InputAction.SelectNextItem),
        Row("INVENTORY", "EQUIP", InputAction.Equip),
        Row("INVENTORY", "USE POTION", InputAction.UsePotion),
        Row("INVENTORY", "USE BANDAGE", InputAction.UseBandage),
        Row("INVENTORY", "DROP", InputAction.Drop),
        Row("ACTIONS", "TOGGLE DOOR", InputAction.ToggleDoor),
        Row("ACTIONS", "THROW ITEM", InputAction.ThrowItem),
        Row("ACTIONS", "FIRE RANGED", InputAction.FireRanged),
        Row("ACTIONS", "PLACE TRAP", InputAction.PlaceTrap),
        Row("ACTIONS", "LAY FALSE TRAIL", InputAction.LayFalseTrail),
        new("FIXED", "PAUSE / CANCEL", "ESC"),
        new("FIXED", "DEBUG VIEW", "F1"),
        new("FIXED", "TEST LOADOUT", "F11"),
        new("FIXED", "REAL-TIME MODE", "F12"),
        new("FIXED", "INVENTORY SLOTS", "1-8")
        ];
    }

    public static string KeyLabel(Keys key) => key switch
    {
        Keys.OemPeriod => ".",
        Keys.OemOpenBrackets => "[",
        Keys.OemCloseBrackets => "]",
        Keys.Space => "SPACE",
        _ => key.ToString().ToUpperInvariant()
    };

}
