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
    public int CellScale { get; } = 10;
    public int MinRooms { get; set; } = 11;
    public int MinDimension { get; set; } = 3;
    public bool ShowGrid { get; set; } = true;
    public Room Root { get; set; }
    public List<Obstacle> MapObstacles { get; set; }
    public List<Corridor> Exits { get; set; }
    public List<Room> RoomList { get; set; }


    private int _nativeWidth;
    private int _nativeHeight;
    private PrimitiveDrawer _primDrawer;
    private GraphicsDevice _graphicsDevice;

    // todo:
    // 1) create a kind of simple 'carved out' array so that we can easily do line-of-sight without thinking about rooms.
    // 2) block off each corridor entrance with a special piece of wall, that we can pass through but that we can't see through,
    // until we've passed through it the first time (opening it, leaving it open, kind of tihng)


    public Map(GraphicsDevice graphicsDevice, int nativeWidth, int nativeHeight)
    {
        _graphicsDevice = graphicsDevice;
        _primDrawer = new PrimitiveDrawer(_graphicsDevice);
        _nativeWidth = nativeWidth;
        _nativeHeight = nativeHeight;

        RandGen.SetSeed(123);
        Initialise();
    }

    public void Initialise()
    {
        IsInitialising = true;

        // clear the list of rooms
        RoomList = new List<Room>();
        // reset the 'root' room to be the whole map
        Root = new Room(0, 0, Width, Height, MinDimension);
        MapObstacles = new List<Obstacle>();
        Exits = new List<Corridor>();

        // carve into spaces (this is the BSP routine)
        DivideIntoRooms();
        // get a flat list of rooms (leaves)
        Root.GetLeaves(RoomList);
        // work out which rooms touch each other
        FindNeighbours();
        // now decrease the room sizes
        Root.ShrinkRoom();
        // and join them up with corridors based on our 'neighbours' list
        AddCorridors();
        // and eat away a little more at the space in the rooms
        Root.Fettle();

        AddExtras();
        IsInitialising = false;
    }

    public void AddExtras()
    {
        //MapObstacles.Add(new Obstacle(0, 0, 1, 1, Microsoft.Xna.Framework.Color.Orange));
        //start at bottom left and work upwards, try to hit a corridor or a room. If we can, we make
        // this trail a corridor. If we cant, we move along 1 more to the right, and repeat.
        // If we somehow can't find a way in from the bottom, we go up the left hand side, working across, instead

        // get the bottom-most corridor
        Corridor candidate = null;
        foreach (Room room in RoomList)
        {
            foreach (Corridor corridor in room.Corridors)
            {
                // skip if a vertical corridor
                if (corridor.X1 + 1 < corridor.X2)
                {
                    if (candidate == null)
                    {
                        candidate = corridor;
                    }
                    else
                    {
                        if (Math.Max(corridor.Y1, corridor.Y2) > Math.Max(candidate.Y1, candidate.Y2))
                        {
                            candidate = corridor;
                        }
                    }
                }
            }
        }

        //todo - sometimes this can cross over a room to get to the corridor we're adjoining. We need to detect that,
        // and simply connect the corridor to the room in question

        if (candidate != null)
        {
            // midpoint of the corridor
            var x = candidate.X1 + (int)((candidate.X2 - candidate.X1) / 2);
            var y = Math.Max(candidate.Y1, candidate.Y2);
            Exits.Add(new Corridor(x, y, x + 1, Height));
            return;
        }

    }

    public void Display(SpriteBatch spriteBatch)
    {
        if (IsInitialising) return;

        foreach (var room in RoomList)
        {
            foreach (var corridor in room.Corridors)
            {
                _primDrawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(corridor.X1 * CellScale, corridor.Y1 * CellScale,
                    (corridor.X2 - corridor.X1) * CellScale, (corridor.Y2 - corridor.Y1) * CellScale),
                    corridor.Color);
            }

            _primDrawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(room.X1 * CellScale, room.Y1 * CellScale,
                (room.X2 - room.X1) * CellScale, (room.Y2 - room.Y1) * CellScale),
                room.Color);

            foreach (var obstacle in room.Obstacles)
            {
                _primDrawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(obstacle.X1 * CellScale, obstacle.Y1 * CellScale,
                    (obstacle.X2 - obstacle.X1) * CellScale, (obstacle.Y2 - obstacle.Y1) * CellScale),
                    obstacle.Color);
            }
        }

        foreach (var obstacle in MapObstacles)
        {
            _primDrawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(obstacle.X1 * CellScale, obstacle.Y1 * CellScale,
                (obstacle.X2 - obstacle.X1) * CellScale, (obstacle.Y2 - obstacle.Y1) * CellScale),
                obstacle.Color);
        }

        foreach (var corridor in Exits)
        {
            _primDrawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(corridor.X1 * CellScale, corridor.Y1 * CellScale,
                (corridor.X2 - corridor.X1) * CellScale, (corridor.Y2 - corridor.Y1) * CellScale),
                corridor.Color);
        }


        if (ShowGrid)
        {
            for (int i = 0; i <= Width; i++)
            {
                _primDrawer.DrawLine(spriteBatch, new Vector2(i * CellScale, 0), new Vector2(i * CellScale, Height * CellScale), Color.Black);
            }
            for (int i = 0; i <= Height; i++)
            {
                _primDrawer.DrawLine(spriteBatch, new Vector2(0, i * CellScale), new Vector2(Width * CellScale, i * CellScale), Color.Black);
            }
        }
    }

    public void DivideIntoRooms()
    {
        int rooms = 0;
        while (rooms < MinRooms)
        {
            if (Root.DivideRoom())
            { rooms++; }
        }
    }

    public void AddCorridors()
    {
        foreach (var room in RoomList)
        {
            foreach (var neighbourRoom in room.RightNeighbours)
            {
                // add a horizontal hall between cell and neighbourCell
                if ((Math.Min(room.Y2, neighbourRoom.Y2) - Math.Max(room.Y1, neighbourRoom.Y1)) > 0)
                {
                    int hallY = RandGen.RandInt(
                        Math.Max(room.Y1, neighbourRoom.Y1),
                        Math.Min(room.Y2, neighbourRoom.Y2) - 1);
                    room.Corridors.Add(new Corridor(room.X2, hallY, neighbourRoom.X1, hallY + 1));
                }
            }

            foreach (var neighbourCell in room.DownNeighbours)
            {
                // add a vertical hall between cell and neighbourCell
                if ((Math.Min(room.X2, neighbourCell.X2) - Math.Max(room.X1, neighbourCell.X1)) > 0)
                {
                    int hallX = RandGen.RandInt(
                        Math.Max(room.X1, neighbourCell.X1),
                        Math.Min(room.X2, neighbourCell.X2) - 1);
                    room.Corridors.Add(new Corridor(hallX, room.Y2, hallX + 1, neighbourCell.Y1));
                }
            }
        }


    }


    public void FindNeighbours()
    {
        foreach (var cell in RoomList)
        {
            foreach (var other in RoomList)
            {
                if (cell != other)
                {
                    if (cell.X2 == other.X1)
                    {
                        if (Math.Max(cell.Y1, other.Y1) < Math.Min(cell.Y2, other.Y2))
                        {
                            cell.RightNeighbours.Add(other);
                        }
                    }
                    if (cell.Y2 == other.Y1)
                    {
                        if (Math.Max(cell.X1, other.X1) < Math.Min(cell.X2, other.X2))
                        {
                            cell.DownNeighbours.Add(other);
                        }
                    }
                }
            }
        }

    }


}
