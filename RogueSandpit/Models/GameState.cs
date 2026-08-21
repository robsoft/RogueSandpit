using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueSandpit.Models;

public class GameState
{
    private readonly NpcTurnScheduler _npcTurnScheduler = new();

    public Map Map { get; private set; }
    public Player Player { get; private set; }
    public GameOutcome Outcome { get; private set; } = GameOutcome.Playing;
    public GameEventLog EventLog { get; } = new();
    public int NextNpcInitiativeOffset => _npcTurnScheduler.InitiativeOffset;

    public GameState(Map map, Player player)
    {
        this.Map = map;
        this.Player = player;
        Player.Place(Map, player.X, player.Y);
    }

    public void Update(PlayerCommand command)
    {
        if (Outcome != GameOutcome.Playing || command == PlayerCommand.None) return;

        bool stunnedTurn = IsPotentialTurnCommand(command)
            && Player.StatusEffects.Has(StatusEffectType.Stunned);
        bool turnTaken = stunnedTurn || command switch
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
            PlayerCommand.ToggleDoorUp => ToggleDoor(0, -1),
            PlayerCommand.ToggleDoorDown => ToggleDoor(0, 1),
            PlayerCommand.ToggleDoorLeft => ToggleDoor(-1, 0),
            PlayerCommand.ToggleDoorRight => ToggleDoor(1, 0),
            PlayerCommand.LayFalseTrailUp => LayFalseTrail(0, -1),
            PlayerCommand.LayFalseTrailDown => LayFalseTrail(0, 1),
            PlayerCommand.LayFalseTrailLeft => LayFalseTrail(-1, 0),
            PlayerCommand.LayFalseTrailRight => LayFalseTrail(1, 0),
            PlayerCommand.ThrowItemUp => ThrowItem(0, -1),
            PlayerCommand.ThrowItemDown => ThrowItem(0, 1),
            PlayerCommand.ThrowItemLeft => ThrowItem(-1, 0),
            PlayerCommand.ThrowItemRight => ThrowItem(1, 0),
            PlayerCommand.PlaceTrapUp => PlaceTrap(0, -1),
            PlayerCommand.PlaceTrapDown => PlaceTrap(0, 1),
            PlayerCommand.PlaceTrapLeft => PlaceTrap(-1, 0),
            PlayerCommand.PlaceTrapRight => PlaceTrap(1, 0),
            _ => false
        };

        if (!turnTaken) return;

        StatusTurnResult playerStatus = Player.AdvanceStatusTurn();
        if (playerStatus.BleedingDamage > 0)
            EventLog.Add($"PLAYER BLED {playerStatus.BleedingDamage}");
        if (stunnedTurn) EventLog.Add("PLAYER STUNNED");

        if (Player.Dead)
        {
            Outcome = GameOutcome.Lost;
            EventLog.Add("PLAYER DIED");
            return;
        }

        if (Outcome == GameOutcome.Won)
        {
            return;
        }

        MoveNPCs();
        Map.AgePlayerTrail();
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
        foreach (BaseNPC npc in _npcTurnScheduler.CreateTurnOrder(Map.NPCs))
        {
            npc.Move(Player, EventLog.Add);
            if (Player.Dead) return;
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
            EmitNoise("DOOR NOISE", 6);
            Map.UpdateVisibility(Player.X, Player.Y);
            return true;
        }

        BaseNPC target = Map.GetLivingNPCAt(newX, newY);
        if (target != null)
        {
            target.TakeDamage(Player.Damage);
            EventLog.Add($"PLAYER HIT {target.Name} {Player.Damage}");
            EmitNoise("COMBAT", 10);
            if (target.State == NPCState.Dead) target.ResolveDeathConsequences(EventLog.Add);
            return true;
        }

        if (Map.IsWalkable(newX, newY))
        {
            int previousX = Player.X;
            int previousY = Player.Y;
            Player.Place(Map, newX, newY);
            Map.RecordPlayerMovement(previousX, previousY, newX, newY);
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
        EmitNoise("DROP NOISE", 4);
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

    private void EmitNoise(string label, int radius)
    {
        int listeners = Map.NotifyNoise(Player.X, Player.Y, radius);
        if (listeners > 0) EventLog.Add($"{label} DREW {listeners} NPCS");
    }

    private bool ToggleDoor(int deltaX, int deltaY)
    {
        Doorway door = Map.GetDoorAt(Player.X + deltaX, Player.Y + deltaY);
        if (door == null || door.State == DoorState.Locked)
        {
            EventLog.Add("NO OPERABLE DOOR THAT WAY");
            return false;
        }
        if (door.State == DoorState.Open && Map.IsOccupiedByLivingNPC(door.X1, door.Y1))
        {
            EventLog.Add("DOORWAY BLOCKED");
            return false;
        }

        bool opening = door.State == DoorState.Closed;
        door.State = opening ? DoorState.Open : DoorState.Closed;
        EventLog.Add(opening ? "OPENED DOOR" : "CLOSED DOOR");
        EmitNoise("DOOR NOISE", 6);
        Map.UpdateVisibility(Player.X, Player.Y);
        return true;
    }

    private bool LayFalseTrail(int deltaX, int deltaY)
    {
        if (!Map.RecordFalseTrail(Player.X, Player.Y, deltaX, deltaY))
        {
            EventLog.Add("CANNOT LAY TRAIL THAT WAY");
            return false;
        }

        EventLog.Add("LAID FALSE TRAIL");
        return true;
    }

    private bool ThrowItem(int deltaX, int deltaY)
    {
        Item item = Player.Inventory.SelectedItem;
        if (item == null)
        {
            EventLog.Add("SELECT AN ITEM TO THROW");
            return false;
        }

        ThrowTrajectory trajectory = Map.TraceThrow(Player.X, Player.Y, deltaX, deltaY);
        if (trajectory == null)
        {
            EventLog.Add("CANNOT THROW THAT WAY");
            return false;
        }

        Player.RemoveFromInventory(item);
        EventLog.Add($"THREW {item.Name}");

        if (trajectory.Target != null)
        {
            if (item.Type == ItemType.Weapon)
            {
                trajectory.Target.TakeDamage(item.Power);
                EventLog.Add($"{item.Name} HIT {trajectory.Target.Name} {item.Power}");
                if (trajectory.Target.State == NPCState.Active)
                {
                    trajectory.Target.ApplyStatus(StatusEffectType.Bleeding, 3, 2, item.Name);
                    EventLog.Add($"{trajectory.Target.Name} BLEEDING");
                }
                else
                {
                    trajectory.Target.ResolveDeathConsequences(EventLog.Add);
                }
            }
            else
            {
                EventLog.Add($"{item.Name} HIT {trajectory.Target.Name}");
            }
        }

        if (item.Type == ItemType.HealingPotion)
        {
            EventLog.Add("HEALING POTION SHATTERED");
        }
        else if (!Map.DropItemNear(item, trajectory.LandingX, trajectory.LandingY, out _))
        {
            Player.Inventory.TryAdd(item);
            EventLog.Add("ITEM RETURNED - NO LANDING SPACE");
        }

        EmitNoiseAt("IMPACT", trajectory.ImpactX, trajectory.ImpactY, 7);
        return true;
    }

    private bool PlaceTrap(int deltaX, int deltaY)
    {
        Item item = Player.Inventory.SelectedItem;
        if (item?.Type != ItemType.Trap)
        {
            EventLog.Add("SELECT A HUNTING TRAP");
            return false;
        }

        int trapX = Player.X + deltaX;
        int trapY = Player.Y + deltaY;
        if (!Map.PlaceTrap(trapX, trapY, item.Power, Player))
        {
            EventLog.Add("CANNOT PLACE TRAP THERE");
            return false;
        }

        Player.RemoveFromInventory(item);
        EventLog.Add("PLACED HUNTING TRAP");
        return true;
    }

    private void EmitNoiseAt(string label, int x, int y, int radius)
    {
        int listeners = Map.NotifyNoise(x, y, radius);
        if (listeners > 0) EventLog.Add($"{label} DREW {listeners} NPCS");
    }

    private static bool IsPotentialTurnCommand(PlayerCommand command)
    {
        return command is not PlayerCommand.None
            and not PlayerCommand.SelectPreviousItem
            and not PlayerCommand.SelectNextItem;
    }

}
