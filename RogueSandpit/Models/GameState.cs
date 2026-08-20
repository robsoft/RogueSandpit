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

    public GameState(Map map, Player player)
    {
        this.Map = map;
        this.Player = player;
    }

    public void Update(PlayerCommand command)
    {
        if (Outcome != GameOutcome.Playing || command == PlayerCommand.None) return;

        switch (command)
        {
            case PlayerCommand.MoveUp:
                AttemptMove(0, -1);
                break;
            case PlayerCommand.MoveDown:
                AttemptMove(0, 1);
                break;
            case PlayerCommand.MoveLeft:
                AttemptMove(-1, 0);
                break;
            case PlayerCommand.MoveRight:
                AttemptMove(1, 0);
                break;
        }

        if (Outcome == GameOutcome.Won)
        {
            return;
        }

        MoveNPCs();
        Player.Update();

        if (Player.Dead)
        {
            Outcome = GameOutcome.Lost;
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
                npc.Move(Player);
                if (Player.Dead) return;
            }
        }
    }

    internal void AttemptMove(int deltaX, int deltaY)
    {
        int newX = Player.X + deltaX;
        int newY = Player.Y + deltaY;

        BaseNPC target = Map.GetLivingNPCAt(newX, newY);
        if (target != null)
        {
            target.TakeDamage(Player.Damage);
            return;
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
            }

            if (Player.HasSpecial && newX == Map.StartPosX && newY == Map.StartPosY)
            {
                Outcome = GameOutcome.Won;
            }
        }
    }

}
