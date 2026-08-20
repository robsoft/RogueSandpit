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
            PlayerCommand.UsePotion => UsePotion(),
            PlayerCommand.EquipWeapon => EquipWeapon(),
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
            Player.X = newX;
            Player.Y = newY;
            Map.CurrentPlayerX = newX;
            Map.CurrentPlayerY = newY;

            // have we now visited a new cell?
            var cell = Map.MapCells[newX, newY];
            if (cell.ParentElement != null)
            {
                cell.ParentElement.HasVisited = true;
            }
            if (cell.CellType==MapCellType.Door)
            {
                // TODO: open the door, for now just remove it
                Map.MapCells[newX, newY].SetCellType(MapCellType.Floor);
            }
            else if (cell.CellType == MapCellType.Special)
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

        if (!Player.Inventory.TryAdd(groundItem.Item))
        {
            EventLog.Add($"INVENTORY FULL {groundItem.Item.Name}");
            return;
        }

        Map.RemoveGroundItem(groundItem);
        EventLog.Add($"PICKED UP {groundItem.Item.Name}");
    }

    private bool UsePotion()
    {
        Item potion = Player.Inventory.FindFirst(ItemType.HealingPotion);
        if (potion == null)
        {
            EventLog.Add("NO HEALING POTION");
            return false;
        }

        int healed = Player.Heal(potion.Power);
        if (healed == 0)
        {
            EventLog.Add("HEALTH FULL");
            return false;
        }

        Player.Inventory.Remove(potion);
        EventLog.Add($"HEALED {healed}");
        return true;
    }

    private bool EquipWeapon()
    {
        Item weapon = Player.Inventory.Items.FirstOrDefault(item =>
            item.Type == ItemType.Weapon && item != Player.EquippedWeapon);
        if (weapon == null)
        {
            EventLog.Add("NO WEAPON TO EQUIP");
            return false;
        }

        Player.Equip(weapon);
        EventLog.Add($"EQUIPPED {weapon.Name}");
        return true;
    }

}
