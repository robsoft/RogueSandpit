using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public class Cell
{
    public int Id { get; set; }
    public int X1 { get; set; }
    public int X2 { get; set; }
    public int Y1 { get; set; }
    public int Y2 { get; set; }
    public Cell Left { get; set; }
    public Cell Right { get; set; }
    public int MinSize { get; set; }
    public Microsoft.Xna.Framework.Color Color { get; set; } = Microsoft.Xna.Framework.Color.White;

    public List<Cell> HNeighbours = new List<Cell>();
    public List<Cell> VNeighbours = new List<Cell>();
    public List<Corridor> HCorridors = new List<Corridor>();
    public List<Corridor> VCorridors = new List<Corridor>();

    public Cell(int X1, int Y1, int X2, int Y2, int MinSize)
    {
        this.X1 = X1;
        this.X2 = X2;
        this.Y1 = Y1;
        this.Y2 = Y2;
        this.MinSize = MinSize;
    }

    public bool IsLeaf()
    {
        return Left == null && Right == null;
    }

    public void GetLeaves(List<Cell> cells)
    { 
        if (IsLeaf())
        {
            cells.Add(this);
        }
        else
        {
            Left.GetLeaves(cells);
            Right.GetLeaves(cells);
        }
    }


    public void Shrink()
    {
        if (Left != null)
        {
            Left.Shrink();
            Right.Shrink();
        }
        else
        {
            Color = new Microsoft.Xna.Framework.Color((byte)RandGen.RandInt(30, 220), (byte)RandGen.RandInt(30, 220), (byte)RandGen.RandInt(30, 220));
            
            int width = X2 - X1;
            int height = Y2 - Y1;
            int newWidth = Math.Max(MinSize, (int)(width * RandGen.RandFloat(0.5F, 0.90F)));
            int newHeight = Math.Max(MinSize, (int)(height * RandGen.RandFloat(0.5F, 0.90F)));
            X1 = (int)(X1 + ((width - newWidth) * 0.5F));
            X2 = (int)(X2 - ((width - newWidth) * 0.5F));
            Y1 = (int)(Y1 + ((height - newHeight) * 0.5F));
            Y2 = (int)(Y2 - ((height - newHeight) * 0.5F));
           
        }
    }

    public void Fettle()
    { }

    public bool Divide()
    {
        int width = X2 - X1;
        int height = Y2 - Y1;
        if (width < MinSize && height < MinSize) { return false; }

        // if we are not a leaf, then we are already divided, so we need to divide one of our children
        if (Left != null)
        {
            if (RandGen.RandInt(0, 2) == 0)
            {
                return Left.Divide();
            }
            else
            {
                return Right.Divide();
            }
        }
        // so we are a leaf, so we need to divide ourselves
        if (width > height)
                {
                    int split = RandGen.RandInt(X1 + MinSize, X2 - MinSize);
                    Left = new Cell(X1, Y1, split, Y2, MinSize);
                    Right = new Cell(split, Y1, X2, Y2, MinSize);
                    return true;
                }
            else
            {
                    int split = RandGen.RandInt(Y1 + MinSize, Y2 - MinSize);
                    Left = new Cell(X1, Y1, X2, split, MinSize);
                    Right = new Cell(X1, split, X2, Y2, MinSize);
                    return true;
                }
    }

}
