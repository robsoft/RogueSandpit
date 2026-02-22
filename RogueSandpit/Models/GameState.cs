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

    public GameState(Map map, Player player)
    {
        this.Map = map;
        this.Player = player;      
    }

    public void Update(GameTime gameTime, KeyboardState currentKeyboardState, KeyboardState previousKeyboardState)
    {
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

        MoveNPCs(gameTime);
        Player.Update(gameTime);

        if (Player.Dead)
        {
            Console.WriteLine("Player is dead! Game over.");
            return; // by bailing before resetting PlayerTakenTurn, we stop the game
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
            }
        }
    }

    private void AttemptMove(int deltaX, int deltaY)
    {
        int newX = Player.X + deltaX;
        int newY = Player.Y + deltaY;

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
                Console.WriteLine($"Player moved to ({newX}, {newY}) which is part of {cell.ParentElement.GetType().Name} at ({cell.ParentElement.X1}, {cell.ParentElement.Y1}, {cell.ParentElement.X2}, {cell.ParentElement.Y2})");
                cell.ParentElement.HasVisited = true;
            }
        }
        PlayerTakenTurn = true;
    }

}