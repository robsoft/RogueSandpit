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
    public int Width { get; } = 50;
    public int Height { get; } = 36;
    public int Scale { get; } = 16;
    public int MinRooms { get; set; } = 12;

    public int MinDimension { get; set; } = 2;

    public Cell Root { get; set; }
    public List<Cell> CellList { get; set; }

    private PrimitiveDrawer _primDrawer;
    private GraphicsDevice _graphicsDevice;

    public Map(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _primDrawer = new PrimitiveDrawer(_graphicsDevice);
        Initialise();

    }

    public void Initialise()
    {
        IsInitialising = true;
        Root = new Cell(0, 0, Width, Height, MinDimension);
        Divide();
        CellList = Root.GetLeaves();
        Root.Shrink();
        IsInitialising = false;
    }

    public void Display(SpriteBatch spriteBatch)
    {
        if (IsInitialising) return;

        foreach (var cell in CellList)
        {
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
