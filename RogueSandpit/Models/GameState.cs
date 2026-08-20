using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueSandpit.Models;

public class GameState
{
    public Map Map { get; private set; }
    public Player Player { get; private set; }
    public GameOutcome Outcome { get; private set; } = GameOutcome.Playing;
    public GameEventLog EventLog { get; } = new();

    public GameState(Map map, Player player)
    {
        this.Map = map;
        this.Player = player;
        Player.Place(Map, player.X, player.Y);
    }

    public void Update(PlayerCommand command)
    {
        if (Outcome != GameOutcome.Playing || command == PlayerCommand.None) return;

        bool turnTaken = command switch
        {
            PlayerCommand.MoveUp => AttemptMove(0, -1),
            PlayerCommand.MoveDown => AttemptMove(0, 1),
            PlayerCommand.MoveLeft => AttemptMove(-1, 0),
            PlayerCommand.MoveRight => AttemptMove(1, 0),
            PlayerCommand.Wait => Wait(),
            PlayerCommand.SelectPreviousItem => SelectItem(false),
            PlayerCommand.SelectNextItem => SelectItem(true),
            PlayerCommand.UsePotion => UsePotion(),
            PlayerCommand.EquipItem => EquipItem(),
            PlayerCommand.DropItem => DropItem(),
            _ => false
        };

        if (!turnTaken) return;

        if (Outcome == GameOutcome.Won)
        {
            return;
        }

        MoveNPCs();
        Player.Update();

        if (Player.Dead)
        {
            Outcome = GameOutcome.Lost;
            EventLog.Add("PLAYER DIED");
            Console.WriteLine("Player is dead! Game over.");
            return;
        }
    }

    private void MoveNPCs()
    {
        foreach (BaseNPC npc in Map.NPCs)
        {
            if (npc.State == NPCState.Active)
            {
                npc.Move(Player, EventLog.Add);
                if (Player.Dead) return;
            }
        }
    }

    internal bool AttemptMove(int deltaX, int deltaY)
    {
        int newX = Player.X + deltaX;
        int newY = Player.Y + deltaY;

        Doorway door = Map.GetDoorAt(newX, newY);
        if (door != null && !door.CanTraverse)
        {
            bool wasLocked = door.State == DoorState.Locked;
            if (wasLocked && Player.Inventory.FindFirst(ItemType.Key) == null)
            {
                EventLog.Add("DOOR LOCKED");
                return true;
            }

            door.State = DoorState.Open;
            EventLog.Add(wasLocked ? "UNLOCKED DOOR" : "OPENED DOOR");
            Map.UpdateVisibility(Player.X, Player.Y);
            return true;
        }

        BaseNPC target = Map.GetLivingNPCAt(newX, newY);
        if (target != null)
        {
            target.TakeDamage(Player.Damage);
            EventLog.Add($"PLAYER HIT {target.Name} {Player.Damage}");
            if (target.State == NPCState.Dead)
            {
                EventLog.Add($"{target.Name} DIED");
                if (Map.DropItem(target.HeldItem, target.X, target.Y))
                {
                    EventLog.Add($"{target.Name} DROPPED {target.HeldItem.Name}");
                    target.HeldItem = null;
                }
            }
            return true;
        }

        if (Map.IsWalkable(newX, newY))
        {
            Player.Place(Map, newX, newY);
            var cell = Map.MapCells[newX, newY];
            if (cell.CellType == MapCellType.Special)
            {
                Player.CollectSpecial();
                cell.SetCellType(MapCellType.Floor);
                EventLog.Add("SPECIAL COLLECTED");
            }

            TryPickupGroundItem(newX, newY);

            if (Player.HasSpecial && newX == Map.StartPosX && newY == Map.StartPosY)
            {
                Outcome = GameOutcome.Won;
                EventLog.Add("YOU ESCAPED WITH SPECIAL");
            }
        }
        return true;
    }

    private void TryPickupGroundItem(int x, int y)
    {
        GroundItem groundItem = Map.GetGroundItemAt(x, y);
        if (groundItem == null) return;

        if (!Player.TryCollectItem(groundItem.Item, out bool autoEquipped))
        {
            EventLog.Add($"INVENTORY FULL {groundItem.Item.Name}");
            return;
        }

        Map.RemoveGroundItem(groundItem);
        EventLog.Add($"PICKED UP {groundItem.Item.Name}");
        if (autoEquipped) EventLog.Add($"AUTO-EQUIPPED {groundItem.Item.Name}");
    }

    private bool UsePotion()
    {
        PlayerItemActionResult result = Player.UseSelectedPotion(out int healed);
        if (result is PlayerItemActionResult.NoSelection or PlayerItemActionResult.WrongItemType)
        {
            EventLog.Add("SELECT A HEALING POTION");
            return false;
        }

        if (result == PlayerItemActionResult.NoEffect)
        {
            EventLog.Add("HEALTH FULL");
            return false;
        }

        EventLog.Add($"HEALED {healed}");
        return true;
    }

    private bool Wait()
    {
        EventLog.Add("PLAYER WAITS");
        return true;
    }

    private bool DropItem()
    {
        PlayerItemActionResult result = Player.DropSelectedItem(Map, out Item item);
        if (result == PlayerItemActionResult.NoSelection)
        {
            EventLog.Add("INVENTORY EMPTY");
            return false;
        }

        if (result == PlayerItemActionResult.Blocked)
        {
            EventLog.Add("CANNOT DROP HERE");
            return false;
        }

        EventLog.Add($"DROPPED {item.Name}");
        return true;
    }

    private bool SelectItem(bool next)
    {
        bool selected = Player.SelectInventoryItem(next);
        if (!selected)
        {
            EventLog.Add("INVENTORY EMPTY");
            return false;
        }

        EventLog.Add($"SELECTED {Player.Inventory.SelectedItem.Name}");
        return false;
    }

    private bool EquipItem()
    {
        PlayerItemActionResult result = Player.EquipSelectedItem(out Item item);
        if (result == PlayerItemActionResult.Success)
        {
            EventLog.Add($"EQUIPPED {item.Name}");
            return true;
        }

        EventLog.Add("SELECT EQUIPMENT");
        return false;
    }

}
