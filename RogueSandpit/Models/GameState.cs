using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RogueSandpit.Models;

public class GameState
{
    public Map Map { get; private set; }
    public Player Player { get; private set; }
    public bool PlayerTakenTurn { get; private set; } = false;
    public GameOutcome Outcome { get; private set; } = GameOutcome.Playing;

    public GameState(Map map, Player player)
    {
        this.Map = map;
        this.Player = player;
    }

    public void Update(GameTime gameTime, KeyboardState currentKeyboardState, KeyboardState previousKeyboardState)
    {
        if (Outcome != GameOutcome.Playing) return;

        if (!PlayerTakenTurn)
        {
            if (currentKeyboardState.IsKeyDown(Keys.Up) && !previousKeyboardState.IsKeyDown(Keys.Up))
            {
                AttemptMove(0, -1);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Down) && !previousKeyboardState.IsKeyDown(Keys.Down))
            {
                AttemptMove(0, 1);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Left) && !previousKeyboardState.IsKeyDown(Keys.Left))
            {
                AttemptMove(-1, 0);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Right) && !previousKeyboardState.IsKeyDown(Keys.Right))
            {
                AttemptMove(1, 0);
            }

        }
        if (!PlayerTakenTurn)
        {
            return;
        }

        if (Outcome == GameOutcome.Won)
        {
            PlayerTakenTurn = false;
            return;
        }

        MoveNPCs(gameTime);
        Player.Update(gameTime);

        if (Player.Dead)
        {
            Outcome = GameOutcome.Lost;
            Console.WriteLine("Player is dead! Game over.");
            PlayerTakenTurn = false;
            return;
        }
        PlayerTakenTurn = false;
    }

    private void MoveNPCs(GameTime gameTime)
    {
        foreach (BaseNPC npc in Map.NPCs)
        {
            if (npc.State == NPCState.Active)
            {
                npc.Move(gameTime, Player);
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
            PlayerTakenTurn = true;
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
        PlayerTakenTurn = true;
    }

}
