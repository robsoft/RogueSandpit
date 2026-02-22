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
    public bool IsInitialising { get; private set; } = false;

    // these are 'config', really
    public int Width { get; } = 79;
    public int Height { get; } = 58;
    public int CellScale { get; } = 10;
    public int MinRooms { get; set; } = 11;
    public int MinDimension { get; set; } = 3;
    public bool ShowGrid { get; set; } = true;
    public RenderMode RenderMode { get; set; } = RenderMode.Rooms;


    public int StartPosX { get; private set; } = 0;
    public int StartPosY { get; private set; } = 0;
    public int CurrentPlayerX { get; set; } = 0;
    public int CurrentPlayerY { get; set; } = 0;

    public MapCell[,] MapCells { get; set; }
    public Room Root { get; set; }
    public List<Obstacle> MapObstacles { get; set; }
    public List<Corridor> Exits { get; set; }
    public List<Room> RoomList { get; set; }
    public List<BaseNPC> NPCs { get; set; } = new List<BaseNPC>();

    private int _nativeWidth;
    private int _nativeHeight;
    private PrimitiveDrawer _primDrawer;
    private GraphicsDevice _graphicsDevice;

    private Color MapBackgroundColor = Color.CornflowerBlue;

    // todo:
    // 1) create a kind of simple 'carved out' array so that we can easily do line-of-sight without thinking about rooms.
    // 2) block off each corridor entrance with a special piece of wall, that we can pass through but that we can't see through,
    // until we've passed through it the first time (opening it, leaving it open, kind of thing)


    public Map(GraphicsDevice graphicsDevice, int nativeWidth, int nativeHeight, int seed = 0)
    {
        _graphicsDevice = graphicsDevice;
        _primDrawer = new PrimitiveDrawer(_graphicsDevice);
        _nativeWidth = nativeWidth;
        _nativeHeight = nativeHeight;

        RandGen.SetSeed(seed);
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
        NPCs = new List<BaseNPC>();

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
        Root.AddColumns();

        // now calculate the area of each room
        CalculateRoomAreas();
        // figure out a starting point for the map
        AddExits();
        AddNPCs();
        AddLoot();
        AddSpecials();

        // now flatten this into a single big 2-d array
        CreateMapCells();

        CurrentPlayerX = StartPosX;
        CurrentPlayerY = StartPosY;

        IsInitialising = false;
    }

    private void CalculateRoomAreas()
    {
        foreach (Room room in RoomList)
        {
            // basic area of the room
            int area = (room.X2 - room.X1) * (room.Y2 - room.Y1);
            // remove any obstacles that are in the room
            foreach (Obstacle obstacle in room.Obstacles)
            {
                int obsArea = (obstacle.X2 - obstacle.X1) * (obstacle.Y2 - obstacle.Y1);
                area -= obsArea;
            }
            room.Area = area;
        }
    }

    private void AddNPCs()
    {
        List<Point> occupiedSpaces = new List<Point>();

        foreach (Room room in RoomList)
        {
            // blank out any 'obstacles' as places we can't put NPCs
            foreach (Obstacle obstacle in room.Obstacles)
            {
                for (int x = obstacle.X1; x < obstacle.X2; x++)
                {
                    for (int y = obstacle.Y1; y < obstacle.Y2; y++)
                    {
                        occupiedSpaces.Add(new Point(x, y));
                    }
                }
            }

            // and add a number of NPCs based on that area (1 per 30 squares, on average)
            int npcCount = (int)(room.Area / 30F);

            for (int i = 0; i < npcCount; i++)
            {
                var x = room.X1 + RandGen.RandInt(0, room.X2 - room.X1);
                var y = room.Y1 + RandGen.RandInt(0, room.Y2 - room.Y1);
                var failCount = 0;
                while (occupiedSpaces.Contains(new Point(x, y)))
                {
                    x = room.X1 + RandGen.RandInt(0, room.X2 - room.X1);
                    y = room.Y1 + RandGen.RandInt(0, room.Y2 - room.Y1);
                    failCount++;
                    if (failCount > 30) break;
                }
                if (failCount > 30) continue;

                occupiedSpaces.Add(new Point(x, y));

                int characterType = RandGen.RandInt(0, Enum.GetValues(typeof(CharacterTypes)).Length);
                var npc = NPCFactory.CreateNPC(this, (CharacterTypes)characterType, x, y, room);
                npc.State = NPCState.Active;

                NPCs.Add(npc);
            }
        }
    }

    private void AddLoot()
    { }

    private void AddSpecials()
    { }

    // this flattens the room/corridor/obstacle structure into a single 2-d array of 'cells' that we can easily query for line-of-sight and pathfinding, without having to think about rooms and corridors etc. We can still use the room/corridor/obstacle structure for rendering, and for any room-specific logic we want to add later on
    private void CreateMapCells()
    {
        MapCells = new MapCell[Width + 1, Height + 1];  // adding one here just makes zero-based indexing easier to follow
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                MapCells[x, y] = new MapCell(x, y, MapCellType.Wall);
            }
        }

        foreach (Room room in RoomList)
        {
            Console.WriteLine($"Room at {room.X1}, {room.Y1}, to {room.X2}, {room.Y2}");

            for (int x = room.X1; x < room.X2; x++)
            {
                for (int y = room.Y1; y < room.Y2; y++)
                {
                    MapCells[x, y] = new MapCell(x, y, MapCellType.Floor, room);
                }
            }

            foreach (Corridor corridor in room.Corridors)
            {
                Console.WriteLine($"Corridor at {corridor.X1}, {corridor.Y1}, to {corridor.X2}, {corridor.Y2}");

                for (int x = corridor.X1; x <= corridor.X2; x++)
                {
                    for (int y = corridor.Y1; y <= corridor.Y2; y++)
                    {
                        MapCells[x, y] = new MapCell(x, y, MapCellType.Floor, corridor);
                    }
                }
                MapCells[corridor.X1, corridor.Y1] = new MapCell(corridor.X1, corridor.Y1, MapCellType.Door);
                MapCells[corridor.X2, corridor.Y2] = new MapCell(corridor.X2, corridor.Y2, MapCellType.Door);
            }

            foreach (Obstacle obstacle in room.Obstacles)
            {
                Console.WriteLine($"Obstacle at {obstacle.X1}, {obstacle.Y1}, to {obstacle.X2}, {obstacle.Y2}");

                for (int x = obstacle.X1; x < obstacle.X2; x++)
                {
                    for (int y = obstacle.Y1; y < obstacle.Y2; y++)
                    {
                        MapCells[x, y] = new MapCell(x, y, MapCellType.Wall, obstacle);
                    }
                }
            }
        }

        // exits are a special case of corridors where one side doesn't have a room        
        foreach (Corridor corridor in Exits)
        {
            for (int x = corridor.X1; x <= corridor.X2; x++)
            {
                for (int y = corridor.Y1; y <= corridor.Y2; y++)
                {
                    MapCells[x, y] = new MapCell(x, y, MapCellType.Floor);
                }
            }
        }
    }


    private bool HasLineOfSight(int x1, int y1, int x2, int y2)
    {
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (!IsWalkable(x1, y1)) return false;
            if (x1 == x2 && y1 == y2) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x1 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y1 += sy;
            }
        }
        return true;
    }

    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
        return MapCells[x, y].CellType == MapCellType.Floor || MapCells[x, y].CellType == MapCellType.Door;
    }

    private void AddExits()
    {
        // start at bottom left and work upwards, try to hit a corridor or a room. If we can, we make
        // this trail a corridor. If we cant, we move along 1 more to the right, and repeat.

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

        // ToDo: - sometimes this can cross over a room to get to the corridor we're adjoining. We need to detect that,
        // and simply connect the corridor to the room in question

        if (candidate != null)
        {
            // midpoint of the corridor
            var x = candidate.X1 + (int)((candidate.X2 - candidate.X1) / 2);
            var y = Math.Max(candidate.Y1, candidate.Y2);
            Console.WriteLine($"Adding exit at {x}, {y}, connecting to corridor at {candidate.X1}, {candidate.Y1}, {candidate.X2}, {candidate.Y2} ");
            // fudge the exit corridor to butt-up to the candidate, but not overlap
            var corridor = new Corridor(x, y + 1, x, Height - 1);
            corridor.HasVisited = true; // we want this to be visible from the start, so mark it as visited
            Exits.Add(corridor);

            StartPosX = x;
            StartPosY = Height - 1;
            return;
        }

        StartPosX = 0;
        StartPosY = Height - 1;

    }


    public void Display(SpriteBatch spriteBatch)
    {
        if (IsInitialising) return;

        if (RenderMode == RenderMode.Cells)
        {
            RenderMapCells(spriteBatch);
        }
        else
        {
            RenderRooms(spriteBatch);
        }

        _primDrawer.DrawFilledRectangle(spriteBatch,
            new Rectangle(CurrentPlayerX * CellScale, CurrentPlayerY * CellScale, CellScale, CellScale),
            Color.White);


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

    // draw the map as 'cells', ie, based on type and indicating line of sight etc
    private void RenderMapCells(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var cell = MapCells[x, y];
                Color color;
                switch (cell.CellType)
                {
                    case MapCellType.Wall:
                        color = Color.DarkGray;
                        break;
                    case MapCellType.Floor:
                        color = Color.LightGray;
                        break;
                    case MapCellType.Door:
                        color = Color.Gray;
                        break;
                    default:
                        color = MapBackgroundColor;
                        break;
                }
                _primDrawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(x * CellScale, y * CellScale, CellScale, CellScale),
                    color);
            }
        }

        foreach (BaseNPC npc in NPCs)
        {
            _primDrawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(npc.X * CellScale, npc.Y * CellScale, CellScale, CellScale),
                Color.Red);
        }

    }

    // draw the map as rooms and corridors
    private void RenderRooms(SpriteBatch spriteBatch)
    {
        foreach (var room in RoomList)
        {
            // draw the corridors first, we might be leading up to a room we haven't visited yet
            foreach (var corridor in room.Corridors)
            {
                if (corridor.HasVisited)
                {
                    _primDrawer.DrawFilledRectangle(spriteBatch,
                        new Rectangle(corridor.X1 * CellScale, corridor.Y1 * CellScale,
                        (1 + corridor.X2 - corridor.X1) * CellScale, (1 + corridor.Y2 - corridor.Y1) * CellScale),
                        corridor.Color);
                }
            }

            if (room.HasVisited)
            {
                _primDrawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(room.X1 * CellScale, room.Y1 * CellScale,
                    (room.X2 - room.X1) * CellScale, (room.Y2 - room.Y1) * CellScale),
                    room.Color);

                foreach (var obstacle in room.Obstacles)
                {
                    _primDrawer.DrawFilledRectangle(spriteBatch,
                        new Rectangle(obstacle.X1 * CellScale, obstacle.Y1 * CellScale,
                        (obstacle.X2 - obstacle.X1) * CellScale, (obstacle.Y2 - obstacle.Y1) * CellScale),
                        MapBackgroundColor); // obstacle.Color);
                }
            }
        }

        foreach (var obstacle in MapObstacles)
        {
            if (obstacle.HasVisited)
            {
                _primDrawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(obstacle.X1 * CellScale, obstacle.Y1 * CellScale,
                    (obstacle.X2 - obstacle.X1) * CellScale, (obstacle.Y2 - obstacle.Y1) * CellScale),
                    MapBackgroundColor); // obstacle.Color);
            }
        }

        foreach (var corridor in Exits)
        {
            _primDrawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(corridor.X1 * CellScale, corridor.Y1 * CellScale,
                (1 + corridor.X2 - corridor.X1) * CellScale, (1 + corridor.Y2 - corridor.Y1) * CellScale),
                corridor.Color);
        }

        BaseContainingElement currentPlayerRoom = MapCells[CurrentPlayerX, CurrentPlayerY].ParentElement;
        foreach (BaseNPC npc in NPCs)
        {
            //if (npc.CurrentRoom.HasVisited)
            // only show NPCs in the current room
            if (currentPlayerRoom != null && npc.CurrentRoom == currentPlayerRoom)
            {
                _primDrawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(npc.X * CellScale, npc.Y * CellScale, CellScale, CellScale),
                Color.Red);
            }
        }


    }

    private void DivideIntoRooms()
    {
        int rooms = 0;
        while (rooms < MinRooms)
        {
            if (Root.DivideRoom())
            { rooms++; }
        }
    }

    private void AddCorridors()
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
                    room.Corridors.Add(new Corridor(room.X2, hallY, neighbourRoom.X1 - 1, hallY));
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
                    room.Corridors.Add(new Corridor(hallX, room.Y2, hallX, neighbourCell.Y1 - 1));
                }
            }
        }


    }


    private void FindNeighbours()
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
