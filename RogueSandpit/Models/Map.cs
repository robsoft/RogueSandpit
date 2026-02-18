using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSandpit.Graphics;
using Microsoft.Xna.Framework;

namespace RogueSandpit.Models;

public class Map
{
    public bool IsInitialising { get; set; } = false;
    public int Width { get; } = 79;
    public int Height { get; } = 58;
    public int Scale { get; } = 10;
    public int MinRooms { get; set; } = 17;

    public int MinDimension { get; set; } = 3;

    public Cell Root { get; set; }
    public List<Cell> CellList { get; set; }

    private PrimitiveDrawer _primDrawer;
    private GraphicsDevice _graphicsDevice;

    public Map(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _primDrawer = new PrimitiveDrawer(_graphicsDevice);
        Initialise();
        RandGen.SetSeed(123);
    }

    public void Initialise()
    {
        IsInitialising = true;

        // bad ones - 123
        //RandGen.SetSeed(124);

        CellList = new List<Cell>();
        Root = new Cell(0, 0, Width, Height, MinDimension);
        
        Divide();
        Root.GetLeaves(CellList);
        FindNeighbours();
        Root.Shrink();
        AddHalls();

        IsInitialising = false;
    }

    public void Display(SpriteBatch spriteBatch)
    {
        if (IsInitialising) return;

        foreach (var cell in CellList)
        {
            foreach (var hall in cell.HCorridors)
            {
                _primDrawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(hall.X1 * Scale, hall.Y1 * Scale,
                    (hall.X2 - hall.X1) * Scale, (hall.Y2 - hall.Y1) * Scale),
                    hall.Color);
            }
            foreach (var hall in cell.VCorridors)
            {
                _primDrawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(hall.X1 * Scale, hall.Y1 * Scale,
                    (hall.X2 - hall.X1) * Scale, (hall.Y2 - hall.Y1) * Scale),
                    hall.Color);
            }

            _primDrawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(cell.X1 * Scale, cell.Y1 * Scale,
                (cell.X2 - cell.X1) * Scale, (cell.Y2 - cell.Y1) * Scale),
                cell.Color);
        }


        for (int i = 0; i <= Width; i++)
        {
            _primDrawer.DrawLine(spriteBatch, new Vector2(i * Scale, 0), new Vector2(i * Scale, Height * Scale), Color.Black);
        }
        for (int i = 0; i <= Height; i++)
        {
            _primDrawer.DrawLine(spriteBatch, new Vector2(0, i * Scale), new Vector2(Width * Scale, i * Scale), Color.Black);
        }
    }

    public void Divide()
    {
        int rooms = 0;
        while (rooms < MinRooms)
        {
            if (Root.Divide())
            { rooms++; }
        }
    }

    public void AddHalls()
    { 
        foreach(var cell in CellList)
        {
            foreach(var neighbourCell in cell.HNeighbours)
            {
                // add a horizontal hall between cell and neighbourCell
                if ((Math.Min(cell.Y2, neighbourCell.Y2) - Math.Max(cell.Y1, neighbourCell.Y1)) > 0)
                {
                    int hallY = RandGen.RandInt(
                        Math.Max(cell.Y1, neighbourCell.Y1), 
                        Math.Min(cell.Y2, neighbourCell.Y2) -1 );
                    cell.HCorridors.Add(new Corridor(cell.X2, hallY, neighbourCell.X1, hallY + 1));
                }
            }

            foreach (var neighbourCell in cell.VNeighbours)
            {
                // add a vertical hall between cell and neighbourCell
                if ((Math.Min(cell.X2, neighbourCell.X2) - Math.Max(cell.X1, neighbourCell.X1)) > 0)
                {
                    int hallX = RandGen.RandInt(
                        Math.Max(cell.X1, neighbourCell.X1),
                        Math.Min(cell.X2, neighbourCell.X2) - 1);
                    cell.VCorridors.Add(new Corridor(hallX, cell.Y2, hallX + 1, neighbourCell.Y1));
                }
            }
        }


    }


    public void FindNeighbours()
    {
        foreach (var cell in CellList)
        {
            foreach (var other in CellList)
            {
                if (cell != other)
                {
                    if (cell.X2 == other.X1)
                    {
                        if (Math.Max(cell.Y1, other.Y1) < Math.Min(cell.Y2, other.Y2))
                        {
                            cell.HNeighbours.Add(other);
                        }
                    }
                    if (cell.Y2 == other.Y1)
                    {
                        if (Math.Max(cell.X1, other.X1) < Math.Min(cell.X2, other.X2))
                        {
                            cell.VNeighbours.Add(other);
                        }
                    }
                }
            }
        }
    }


}
