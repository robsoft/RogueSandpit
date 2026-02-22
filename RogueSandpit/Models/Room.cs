using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public class Room : BaseContainingElement
{
    public Room LeftLeaves { get; set; }
    public Room RightLeaves { get; set; }
    public int MinimumSize { get; set; }
    public Microsoft.Xna.Framework.Color Color { get; set; } = Microsoft.Xna.Framework.Color.White;

    public int Area { get; set; } = 0;

    public List<Room> RightNeighbours = new List<Room>();
    public List<Room> DownNeighbours = new List<Room>();
    public List<Corridor> Corridors = new List<Corridor>();
    public List<Obstacle> Obstacles = new List<Obstacle>();

    public Room(int X1, int Y1, int X2, int Y2, int MinSize) : base(X1, Y1, X2, Y2)
    {
        this.MinimumSize = MinSize;
    }

    public bool IsLeaf()
    {
        return LeftLeaves == null && RightLeaves == null;
    }

    public void GetLeaves(List<Room> cells)
    {
        if (IsLeaf())
        {
            cells.Add(this);
        }
        else
        {
            LeftLeaves.GetLeaves(cells);
            RightLeaves.GetLeaves(cells);
        }
    }


    public void ShrinkRoom()
    {
        if (!IsLeaf())
        {
            LeftLeaves.ShrinkRoom();
            RightLeaves.ShrinkRoom();
        }
        else
        {
            Color = new Microsoft.Xna.Framework.Color((byte)RandGen.RandInt(30, 220), (byte)RandGen.RandInt(30, 220), (byte)RandGen.RandInt(30, 220));

            int width = X2 - X1;
            int height = Y2 - Y1;
            int newWidth = Math.Max(MinimumSize, (int)(width * RandGen.RandFloat(0.5F, 0.90F)));
            int newHeight = Math.Max(MinimumSize, (int)(height * RandGen.RandFloat(0.5F, 0.90F)));
            var xDiff = (int)((width - newWidth) * 0.5F);
            var yDiff = (int)((height - newHeight) * 0.5F);
            X1 += xDiff;
            X2 -= xDiff;
            Y1 += yDiff;
            Y2 -= yDiff;

        }
    }

    public void AddColumns()
    {
        // add some columns (2x2, 2x3 etc) into larger rooms
        if (!IsLeaf())
        {
            LeftLeaves.AddColumns();
            RightLeaves.AddColumns();
        }
        else
        {
            // are we big enough to add a column?
            var xSize = RandGen.RandInt(1, MinimumSize * 4);
            var ySize = RandGen.RandInt(1, MinimumSize * 4);

            // the '2' is to allow for a gap around the column, so we don't end up with a 1x1 room
            if ((X2 - X1) > (xSize + 2) && (Y2 - Y1) > (ySize + 2))
            {
                // plonk roughly in middle for now
                var x = X1 + (int)((X2 - X1 - xSize) / 2);
                var y = Y1 + (int)((Y2 - Y1 - ySize) / 2);

                Obstacles.Add(new Obstacle(x, y, x + xSize, y + ySize,
                    Microsoft.Xna.Framework.Color.Black));
            }

            if (RandGen.RandInt(0, 2) == 0)
            {
                var corner = new int[4, 2] { { X1, Y1 }, { X2, Y1 }, { X1, Y2 }, { X2, Y2 } };
                for (int i = 0; i < 4; i++)
                {
                    if (RandGen.RandInt(0, 2) == 0)
                    {
                        var x = corner[i, 0];
                        var y = corner[i, 1];
                        // check that there isn't a corridor, or another room immediately adjacent to this corner, and if not, add a little obstacle to make it a bit more interesting
                        //Obstacles.Add(new Obstacle(x, y, x + RandGen.RandInt(1, 3), y + RandGen.RandInt(1, 3),
                        //        Microsoft.Xna.Framework.Color.Black));
                    }
                }
            }
        }
    }

    public bool DivideRoom()
    {
        int width = X2 - X1;
        int height = Y2 - Y1;
        if (width < MinimumSize && height < MinimumSize) { return false; }

        // if we are not a leaf, then we are already divided, so we need to divide one of our children
        if (!IsLeaf())
        {
            if (RandGen.RandInt(0, 2) == 0)
            {
                return LeftLeaves.DivideRoom();
            }
            else
            {
                return RightLeaves.DivideRoom();
            }
        }
        // so we are a leaf, so we need to divide ourselves
        if (width > height)
        {
            int split = RandGen.RandInt(X1 + MinimumSize, X2 - MinimumSize);
            LeftLeaves = new Room(X1, Y1, split, Y2, MinimumSize);
            RightLeaves = new Room(split, Y1, X2, Y2, MinimumSize);
            return true;
        }
        else
        {
            int split = RandGen.RandInt(Y1 + MinimumSize, Y2 - MinimumSize);
            LeftLeaves = new Room(X1, Y1, X2, split, MinimumSize);
            RightLeaves = new Room(X1, split, X2, Y2, MinimumSize);
            return true;
        }
    }

}
