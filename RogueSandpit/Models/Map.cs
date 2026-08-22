using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace RogueSandpit.Models;



public class Map
{
    private const int TrailLifetime = 12;
    private const int TrailCapacity = 24;
    private const int DoorClusterRadius = 2;
    public const int EntranceSafetyDistance = 10;
    private long _nextTrailSequence;
    internal long NextTrailSequence => _nextTrailSequence;
    private Dictionary<(int X, int Y), int> _entranceDistances = [];
    public bool IsInitialising { get; private set; } = false;
    public int Seed { get; private set; }
    public int DoorCandidateCount { get; private set; }
    public int PrunedDoorwayCount { get; private set; }
    public int MaximumEntranceDistance { get; private set; }
    public int MinimumRoomEntranceDistance { get; private set; }
    public int MaximumRoomEntranceDistance { get; private set; }

    // these are 'config', really
    public int Width { get; } = 80;
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
    public List<GroundItem> GroundItems { get; set; } = new List<GroundItem>();
    public List<Doorway> Doors { get; set; } = new List<Doorway>();
    public List<PlacedTrap> PlacedTraps { get; } = new();
    public List<EnvironmentalEffect> EnvironmentalEffects { get; } = new();
    public List<PlayerTrailClue> PlayerTrail { get; } = new();

    // todo:
    // 1) create a kind of simple 'carved out' array so that we can easily do line-of-sight without thinking about rooms.
    // 2) block off each corridor entrance with a special piece of wall, that we can pass through but that we can't see through,
    // until we've passed through it the first time (opening it, leaving it open, kind of thing)


    public Map(int seed = 0)
    {
        Seed = seed;
        Initialise();
    }

    public void Initialise()
    {
        RandGen.SetSeed(Seed);
        IsInitialising = true;

        // clear the list of rooms
        RoomList = new List<Room>();
        // reset the 'root' room to be the whole map
        Root = new Room(0, 0, Width, Height, MinDimension);

        MapObstacles = new List<Obstacle>();
        Exits = new List<Corridor>();
        NPCs = new List<BaseNPC>();
        GroundItems = new List<GroundItem>();
        Doors = new List<Doorway>();
        PlacedTraps.Clear();
        EnvironmentalEffects.Clear();
        PlayerTrail.Clear();
        _nextTrailSequence = 0;

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

        // Name the rooms
        NameRooms();

        // figure out a starting point for the map
        AddExits();

        // now flatten this into a single big 2-d array
        CreateMapCells();
        AddDoorways();
        CalculateGenerationDepths();

        // Add the macguffin to a reachable, unoccupied floor cell
        AddSpecials();
        AddNPCs();
        AddLoot();

        CurrentPlayerX = StartPosX;
        CurrentPlayerY = StartPosY;

        IsInitialising = false;
    }

    public void Regenerate(int seed)
    {
        Seed = seed;
        Initialise();
    }

    internal void RestorePersistence(SaveGameSnapshot snapshot, IReadOnlyDictionary<Guid, Item> items)
    {
        foreach (CellSnapshot saved in snapshot.Cells)
        {
            MapCell cell = MapCells[saved.X, saved.Y];
            cell.SetCellType(saved.CellType);
            cell.IsVisible = saved.IsVisible;
            cell.IsDiscovered = saved.IsDiscovered;
            if (cell.ParentElement != null) cell.ParentElement.HasVisited = saved.ParentVisited;
        }

        foreach (Room room in RoomList) room.Specials.Clear();
        foreach (CellSnapshot saved in snapshot.Cells.Where(cell => cell.CellType == MapCellType.Special))
        {
            if (MapCells[saved.X, saved.Y].ParentElement is Room room)
                room.Specials.Add(new Special(saved.X, saved.Y, room));
        }

        Doors = snapshot.Doors.Select(door => new Doorway(door.X, door.Y, door.State)).ToList();
        GroundItems = snapshot.GroundItems.Select(ground =>
            new GroundItem(items[ground.ItemId], ground.X, ground.Y)).ToList();
        PlacedTraps.Clear();
        PlacedTraps.AddRange(snapshot.Traps.Select(trap =>
            new PlacedTrap(trap.X, trap.Y, trap.Damage, trap.Kind)));
        EnvironmentalEffects.Clear();
        EnvironmentalEffects.AddRange(snapshot.Effects.Select(effect => new EnvironmentalEffect(
            effect.Type, effect.X, effect.Y, effect.RemainingTurns, effect.Power)));
        PlayerTrail.Clear();
        PlayerTrail.AddRange(snapshot.Trails.Select(trail => new PlayerTrailClue(trail.Sequence,
            trail.X, trail.Y, trail.NextX, trail.NextY, trail.RemainingTurns,
            trail.Strength, trail.IsAuthentic)));
        _nextTrailSequence = snapshot.NextTrailSequence;

        NPCs.Clear();
        foreach (NpcSaveSnapshot saved in snapshot.Npcs)
        {
            BaseContainingElement room = MapCells[saved.X, saved.Y].ParentElement;
            BaseNPC npc = NPCFactory.CreateNPC(this, saved.CharacterType, saved.X, saved.Y, room);
            npc.RestorePersistence(saved, items);
            NPCs.Add(npc);
        }
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
        foreach (Room room in RoomList)
        {
            // and add a number of NPCs based on that area (1 per 35 squares, on average)
            int npcCount = (int)(room.Area / 35F);
            if (room.Specials.Count > 0) npcCount = Math.Max(2, npcCount);
            var candidates = new List<Point>();
            for (int x = room.X1; x < room.X2; x++)
            {
                for (int y = room.Y1; y < room.Y2; y++)
                {
                    if (MapCells[x, y].ParentElement == room
                        && MapCells[x, y].CellType == MapCellType.Floor
                        && CanNpcEnter(x, y)
                        && GetEntranceDistance(x, y) > EntranceSafetyDistance)
                    {
                        candidates.Add(new Point(x, y));
                    }
                }
            }

            for (int i = 0; i < npcCount && candidates.Count > 0; i++)
            {
                int candidateIndex = RandGen.RandInt(0, candidates.Count);
                Point position = candidates[candidateIndex];
                candidates.RemoveAt(candidateIndex);

                GenerationDepthBand depth = GetDepthBand(position.X, position.Y);
                CharacterTypes characterType = ChooseNpcType(depth);
                var npc = NPCFactory.CreateNPC(this, characterType, position.X, position.Y, room);
                npc.State = NPCState.Active;
                if (RandGen.RandInt(0, 100) < 30)
                {
                    npc.HeldItem = ItemFactory.CreateForDepth(depth);
                }

                NPCs.Add(npc);
            }
        }
    }

    private void NameRooms()
    {
        int roomNum = 1;
        foreach (Room room in RoomList)
        {
            room.Name = $"Room {roomNum}";
            roomNum++;
        }
    }

    private void AddDoorways()
    {
        var candidates = new List<Point>();
        for (int x = 1; x < Width - 1; x++)
        {
            for (int y = 1; y < Height - 1; y++)
            {
                if (MapCells[x, y].ParentElement is not Corridor corridor || Exits.Contains(corridor)) continue;

                bool touchesRoom = MapCells[x - 1, y].ParentElement is Room
                    || MapCells[x + 1, y].ParentElement is Room
                    || MapCells[x, y - 1].ParentElement is Room
                    || MapCells[x, y + 1].ParentElement is Room;
                if (touchesRoom) candidates.Add(new Point(x, y));
            }
        }

        DoorCandidateCount = candidates.Count;
        var planned = candidates.Select((position, index) =>
            (Position: position, Index: index,
                State: index % 4 == 0 ? DoorState.Locked : DoorState.Closed)).ToList();
        var retained = planned.Where(candidate => candidate.State == DoorState.Locked).ToList();

        foreach (var candidate in planned.Where(candidate => candidate.State == DoorState.Closed))
        {
            bool clustered = retained.Any(existing =>
                Math.Abs(existing.Position.X - candidate.Position.X) <= DoorClusterRadius
                && Math.Abs(existing.Position.Y - candidate.Position.Y) <= DoorClusterRadius);
            if (!clustered) retained.Add(candidate);
        }

        PrunedDoorwayCount = planned.Count - retained.Count;
        foreach (var candidate in retained.OrderBy(candidate => candidate.Index))
        {
            Doors.Add(new Doorway(candidate.Position.X, candidate.Position.Y, candidate.State));
            MapCells[candidate.Position.X, candidate.Position.Y].SetCellType(MapCellType.Door);
        }
    }

    private static CharacterTypes ChooseNpcType(GenerationDepthBand depth)
    {
        int roll = RandGen.RandInt(0, 100);
        return depth switch
        {
            GenerationDepthBand.Shallow => roll < 40 ? CharacterTypes.Wretch
                : roll < 75 ? CharacterTypes.Goblin
                : roll < 90 ? CharacterTypes.Skeleton
                : CharacterTypes.Orc,
            GenerationDepthBand.Middle => roll < 20 ? CharacterTypes.Wretch
                : roll < 45 ? CharacterTypes.Goblin
                : roll < 70 ? CharacterTypes.Skeleton
                : roll < 90 ? CharacterTypes.Orc
                : CharacterTypes.Troll,
            _ => roll < 10 ? CharacterTypes.Wretch
                : roll < 25 ? CharacterTypes.Goblin
                : roll < 45 ? CharacterTypes.Skeleton
                : roll < 70 ? CharacterTypes.Orc
                : CharacterTypes.Troll
        };
    }

    private void AddLoot()
    {
        int entranceKeyY = StartPosY - 1;
        if (IsWalkable(StartPosX, entranceKeyY))
        {
            GroundItems.Add(new GroundItem(ItemFactory.Create(ItemType.Key), StartPosX, entranceKeyY));
        }

        var candidates = new List<Point>();
        foreach (Room room in RoomList)
        {
            for (int x = room.X1; x < room.X2; x++)
            {
                for (int y = room.Y1; y < room.Y2; y++)
                {
                    if (MapCells[x, y].CellType == MapCellType.Floor
                        && !IsOccupiedByLivingNPC(x, y)
                        && GetGroundItemAt(x, y) == null
                        && (x != StartPosX || y != StartPosY))
                    {
                        candidates.Add(new Point(x, y));
                    }
                }
            }
        }

        ItemType[] deepConsumables = [ItemType.Bandage, ItemType.SmokeBomb, ItemType.FireBomb];
        ItemType deepConsumable = deepConsumables[RandGen.RandInt(0, deepConsumables.Length)];
        var placements = new (ItemType Type, GenerationDepthBand Band)[]
        {
            (ItemType.HealingPotion, GenerationDepthBand.Shallow),
            (ItemType.Weapon, GenerationDepthBand.Shallow),
            (ItemType.Armor, GenerationDepthBand.Middle),
            (ItemType.Trap, GenerationDepthBand.Middle),
            (ItemType.RangedWeapon, GenerationDepthBand.Deep),
            (deepConsumable, GenerationDepthBand.Deep)
        };

        foreach ((ItemType type, GenerationDepthBand band) in placements)
        {
            if (candidates.Count == 0) break;
            Point position = RemoveLootCandidate(candidates, band);
            Item item = type is ItemType.Weapon or ItemType.Armor or ItemType.RangedWeapon
                ? ItemFactory.CreateEquipment(type, (int)band + 1)
                : ItemFactory.Create(type);
            GroundItems.Add(new GroundItem(item, position.X, position.Y));
        }
    }

    private Point RemoveLootCandidate(List<Point> candidates, GenerationDepthBand desiredBand)
    {
        List<Point> preferred = candidates.Where(position =>
            GetDepthBand(position.X, position.Y) == desiredBand).ToList();
        List<Point> pool = preferred.Count > 0 ? preferred : candidates;
        Point selected = pool[RandGen.RandInt(0, pool.Count)];
        candidates.Remove(selected);
        return selected;
    }

    private void AddSpecials()
    {
        Room specialRoom = null;
        Point specialPosition = Point.Zero;
        int greatestDistance = -1;
        Dictionary<(int X, int Y), int> distances = _entranceDistances;

        foreach (Room room in RoomList)
        {
            for (int x = room.X1; x < room.X2; x++)
            {
                for (int y = room.Y1; y < room.Y2; y++)
                {
                    if (MapCells[x, y].CellType != MapCellType.Floor || IsOccupiedByLivingNPC(x, y)) continue;
                    if (!distances.TryGetValue((x, y), out int distance)) continue;

                    if (distance > greatestDistance)
                    {
                        greatestDistance = distance;
                        specialRoom = room;
                        specialPosition = new Point(x, y);
                    }
                }
            }
        }

        if (specialRoom == null) return;

        specialRoom.Specials.Add(new Special(specialPosition.X, specialPosition.Y, specialRoom));
        MapCells[specialPosition.X, specialPosition.Y].SetCellType(MapCellType.Special);
    }

    private Dictionary<(int X, int Y), int> CalculateTerrainDistances()
    {
        var distances = new Dictionary<(int X, int Y), int>
        {
            [(StartPosX, StartPosY)] = 0
        };
        var frontier = new Queue<(int X, int Y)>();
        frontier.Enqueue((StartPosX, StartPosY));

        while (frontier.TryDequeue(out (int X, int Y) current))
        {
            foreach ((int dx, int dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
            {
                var next = (X: current.X + dx, Y: current.Y + dy);
                if (next.X < 0 || next.X >= Width || next.Y < 0 || next.Y >= Height) continue;
                if (MapCells[next.X, next.Y].CellType == MapCellType.Wall || distances.ContainsKey(next)) continue;

                distances[next] = distances[current] + 1;
                frontier.Enqueue(next);
            }
        }

        return distances;
    }

    private void CalculateGenerationDepths()
    {
        _entranceDistances = CalculateTerrainDistances();
        MaximumEntranceDistance = _entranceDistances.Count == 0
            ? 0
            : _entranceDistances.Values.Max();
        List<int> roomDistances = _entranceDistances
            .Where(pair => MapCells[pair.Key.X, pair.Key.Y].ParentElement is Room
                && MapCells[pair.Key.X, pair.Key.Y].CellType == MapCellType.Floor)
            .Select(pair => pair.Value)
            .ToList();
        MinimumRoomEntranceDistance = roomDistances.Count == 0 ? 0 : roomDistances.Min();
        MaximumRoomEntranceDistance = roomDistances.Count == 0 ? 0 : roomDistances.Max();
    }

    public int GetEntranceDistance(int x, int y) =>
        _entranceDistances.TryGetValue((x, y), out int distance) ? distance : -1;

    public GenerationDepthBand GetDepthBand(int x, int y)
    {
        int distance = GetEntranceDistance(x, y);
        if (distance < 0 || MaximumRoomEntranceDistance <= MinimumRoomEntranceDistance)
            return GenerationDepthBand.Shallow;
        double depth = (double)(distance - MinimumRoomEntranceDistance)
            / (MaximumRoomEntranceDistance - MinimumRoomEntranceDistance);
        if (depth < 1.0 / 3.0) return GenerationDepthBand.Shallow;
        return depth < 2.0 / 3.0 ? GenerationDepthBand.Middle : GenerationDepthBand.Deep;
    }

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

        }

        foreach (Room room in RoomList)
        {
            foreach (Corridor corridor in room.HCorridors)
            {
                PaintCorridor(corridor);
            }

            foreach (Corridor corridor in room.VCorridors)
            {
                PaintCorridor(corridor);
            }
        }

        foreach (Room room in RoomList)
        {
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
            PaintCorridor(corridor);
        }
    }

    private void PaintCorridor(Corridor corridor)
    {
        Console.WriteLine($"Corridor at {corridor.X1}, {corridor.Y1}, to {corridor.X2}, {corridor.Y2}");
        for (int x = corridor.X1; x <= corridor.X2; x++)
        {
            for (int y = corridor.Y1; y <= corridor.Y2; y++)
            {
                MapCells[x, y] = new MapCell(x, y, MapCellType.Floor, corridor);
            }
        }
    }


    public bool HasLineOfSight(int x1, int y1, int x2, int y2)
    {
        return TraceLineOfSight(x1, y1, x2, y2, false);
    }

    public void UpdateVisibility(int playerX, int playerY, int sightRange = 12)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                MapCells[x, y].IsVisible = false;
                int dx = x - playerX;
                int dy = y - playerY;
                if (dx * dx + dy * dy > sightRange * sightRange) continue;
                if (!TraceLineOfSight(playerX, playerY, x, y, true)) continue;

                MapCells[x, y].IsVisible = true;
                MapCells[x, y].IsDiscovered = true;
            }
        }
    }

    private bool TraceLineOfSight(int x1, int y1, int x2, int y2, bool includeOpaqueTarget)
    {
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x1 == x2 && y1 == y2)
            {
                return includeOpaqueTarget || IsWalkable(x1, y1);
            }
            if (!IsWalkable(x1, y1) || BlocksSight(x1, y1)) return false;
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
    }

    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
        return MapCells[x, y].CellType == MapCellType.Floor
            || (MapCells[x, y].CellType == MapCellType.Door && GetDoorAt(x, y)?.CanTraverse == true)
            || MapCells[x, y].CellType == MapCellType.Special;
    }

    public Doorway GetDoorAt(int x, int y)
    {
        return Doors.FirstOrDefault(door => door.X1 == x && door.Y1 == y);
    }

    public BaseNPC GetLivingNPCAt(int x, int y, BaseNPC except = null)
    {
        return NPCs.FirstOrDefault(npc =>
            npc != except &&
            npc.State != NPCState.Dead &&
            npc.X == x &&
            npc.Y == y);
    }

    public bool IsOccupiedByLivingNPC(int x, int y, BaseNPC except = null)
    {
        return GetLivingNPCAt(x, y, except) != null;
    }

    public bool CanNpcEnter(
        int x,
        int y,
        BaseNPC movingNpc = null,
        Player player = null,
        bool allowClosedDoor = false)
    {
        bool terrainAllowsEntry = IsWalkable(x, y)
            || (allowClosedDoor && GetDoorAt(x, y)?.State == DoorState.Closed);
        return terrainAllowsEntry
            && !IsFireAt(x, y)
            && !IsOccupiedByLivingNPC(x, y, movingNpc)
            && (player == null || x != player.X || y != player.Y);
    }

    public (int X, int Y)? PredictContinuation(int x, int y, int deltaX, int deltaY, int maximumSteps = 4)
    {
        if (Math.Abs(deltaX) + Math.Abs(deltaY) != 1) return null;

        (int X, int Y)? prediction = null;
        for (int step = 1; step <= maximumSteps; step++)
        {
            int targetX = x + deltaX * step;
            int targetY = y + deltaY * step;
            if (!CanProjectThrough(targetX, targetY)) break;
            prediction = (targetX, targetY);
        }
        return prediction;
    }

    public void RecordPlayerMovement(int fromX, int fromY, int toX, int toY)
    {
        bool doorwayPassage = MapCells[fromX, fromY].CellType == MapCellType.Door
            || MapCells[toX, toY].CellType == MapCellType.Door;
        bool corridorPassage = MapCells[fromX, fromY].ParentElement is Corridor;
        int strength = doorwayPassage ? 3 : corridorPassage ? 2 : 1;
        int lifetime = doorwayPassage ? 18 : corridorPassage ? 16 : TrailLifetime;
        AddTrailClue(fromX, fromY, toX, toY, lifetime, strength, true);
    }

    public bool BlocksSight(int x, int y) => EnvironmentalEffects.Any(effect =>
        effect.Type == EnvironmentalEffectType.Smoke && effect.X == x && effect.Y == y);

    public EnvironmentalEffect GetEnvironmentalEffectAt(int x, int y, EnvironmentalEffectType type) =>
        EnvironmentalEffects.FirstOrDefault(effect => effect.Type == type && effect.X == x && effect.Y == y);

    public bool IsFireAt(int x, int y) => GetEnvironmentalEffectAt(x, y, EnvironmentalEffectType.Fire) != null;

    public void AddEnvironmentalEffect(EnvironmentalEffectType type, int x, int y,
        int duration = 4, int power = 0)
    {
        EnvironmentalEffect existing = GetEnvironmentalEffectAt(x, y, type);
        if (existing != null)
        {
            existing.RemainingTurns = Math.Max(existing.RemainingTurns, duration);
            return;
        }
        EnvironmentalEffects.Add(new EnvironmentalEffect(type, x, y, duration, power));
    }

    public void AgeEnvironmentalEffects()
    {
        foreach (EnvironmentalEffect effect in EnvironmentalEffects) effect.RemainingTurns--;
        EnvironmentalEffects.RemoveAll(effect => effect.RemainingTurns <= 0);
    }

    public bool RecordFalseTrail(int x, int y, int deltaX, int deltaY)
    {
        int nextX = x + deltaX;
        int nextY = y + deltaY;
        if (Math.Abs(deltaX) + Math.Abs(deltaY) != 1 || !IsWalkable(nextX, nextY)) return false;
        AddTrailClue(x, y, nextX, nextY, 6, 1, false);
        return true;
    }

    public List<Doorway> GetAdjacentOperableDoors(int x, int y)
    {
        return Doors.Where(door => door.State != DoorState.Locked
            && Math.Abs(door.X1 - x) + Math.Abs(door.Y1 - y) == 1
            && (door.State == DoorState.Closed
                || !IsOccupiedByLivingNPC(door.X1, door.Y1))).ToList();
    }

    private void AddTrailClue(int x, int y, int nextX, int nextY,
        int lifetime, int strength, bool isAuthentic)
    {
        PlayerTrail.RemoveAll(clue => clue.X == x && clue.Y == y);
        PlayerTrail.Add(new PlayerTrailClue(++_nextTrailSequence,
            x, y, nextX, nextY, lifetime, strength, isAuthentic));
        if (PlayerTrail.Count > TrailCapacity) PlayerTrail.RemoveAt(0);
    }

    public void AgePlayerTrail()
    {
        foreach (PlayerTrailClue clue in PlayerTrail) clue.RemainingTurns--;
        PlayerTrail.RemoveAll(clue => clue.RemainingTurns <= 0);
    }

    public PlayerTrailClue FindNewestTrailNear(int x, int y, long afterSequence, int detectionRange = 1)
    {
        if (detectionRange < 0) return null;
        return PlayerTrail
            .Where(clue => clue.Sequence > afterSequence
                && Math.Abs(clue.X - x) + Math.Abs(clue.Y - y) <= detectionRange)
            .OrderByDescending(clue => clue.Sequence)
            .FirstOrDefault();
    }

    private bool CanProjectThrough(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
        if (IsWalkable(x, y)) return true;
        return GetDoorAt(x, y)?.State == DoorState.Closed;
    }

    public int NotifyNoise(int x, int y, int radius, BaseNPC sourceNpc = null)
    {
        int notified = 0;
        foreach (BaseNPC npc in NPCs)
        {
            if (npc == sourceNpc) continue;
            int distance = Math.Abs(npc.X - x) + Math.Abs(npc.Y - y);
            int hearingRadius = Math.Max(0, radius + npc.AwarenessProfile.HearingAdjustment);
            if (distance <= hearingRadius
                && npc.ReceiveInvestigation((x, y), NPCInvestigationSource.Noise))
            {
                notified++;
            }
        }
        return notified;
    }

    public int AlertNearbyAllies(BaseNPC sourceNpc, int observedPlayerX, int observedPlayerY)
    {
        return AlertNearbyAllies(sourceNpc, observedPlayerX, observedPlayerY,
            sourceNpc.AwarenessProfile.AllyAlertRadius);
    }

    public int AlertNearbyAllies(BaseNPC sourceNpc, int observedPlayerX, int observedPlayerY, int radius)
    {
        int alerted = 0;
        List<(int X, int Y)> assignments = CoordinatedSearchAssignments(observedPlayerX, observedPlayerY);
        foreach (BaseNPC npc in NPCs)
        {
            if (npc == sourceNpc) continue;
            int distance = Math.Abs(npc.X - sourceNpc.X) + Math.Abs(npc.Y - sourceNpc.Y);
            (int X, int Y) target = alerted < assignments.Count
                ? assignments[alerted]
                : (observedPlayerX, observedPlayerY);
            if (distance <= radius
                && npc.ReceiveInvestigation(target, NPCInvestigationSource.AllyAlert))
            {
                alerted++;
            }
        }
        return alerted;
    }

    public (int X, int Y)? FindRetreatTarget(BaseNPC npc, int threatX, int threatY, int searchRadius = 6)
    {
        (int X, int Y)? bestTarget = null;
        int bestScore = int.MinValue;
        for (int x = Math.Max(0, npc.X - searchRadius); x <= Math.Min(Width - 1, npc.X + searchRadius); x++)
        {
            for (int y = Math.Max(0, npc.Y - searchRadius); y <= Math.Min(Height - 1, npc.Y + searchRadius); y++)
            {
                if (x == npc.X && y == npc.Y) continue;
                if (!CanNpcEnter(x, y, npc, allowClosedDoor: true)) continue;
                List<(int X, int Y)> path = Pathfinding.FindPath(this, npc.X, npc.Y, x, y, npc);
                if (path.Count == 0) continue;

                int threatDistance = Math.Abs(x - threatX) + Math.Abs(y - threatY);
                int homeDistance = Math.Abs(x - npc.HomeX) + Math.Abs(y - npc.HomeY);
                int score = threatDistance * 10 - path.Count - homeDistance;
                if (GetDoorAt(x, y) != null) score += 8;
                if (score <= bestScore) continue;
                bestScore = score;
                bestTarget = (x, y);
            }
        }
        return bestTarget;
    }

    private List<(int X, int Y)> CoordinatedSearchAssignments(int x, int y)
    {
        var assignments = new List<(int X, int Y)>();
        foreach ((int dx, int dy) in new[] { (0, -1), (1, 0), (0, 1), (-1, 0) })
        {
            if (IsWalkable(x + dx, y + dy)) assignments.Add((x + dx, y + dy));
        }
        return assignments;
    }

    public GroundItem GetGroundItemAt(int x, int y)
    {
        return GroundItems.FirstOrDefault(groundItem => groundItem.X == x && groundItem.Y == y);
    }

    public bool RemoveGroundItem(GroundItem groundItem)
    {
        return groundItem != null && GroundItems.Remove(groundItem);
    }

    public bool DropItem(Item item, int x, int y)
    {
        if (item == null || !IsWalkable(x, y) || GetGroundItemAt(x, y) != null) return false;
        GroundItems.Add(new GroundItem(item, x, y));
        return true;
    }

    public (int X, int Y)? FindThrowLanding(int x, int y, int deltaX, int deltaY, int range = 6)
    {
        ThrowTrajectory trajectory = TraceThrow(x, y, deltaX, deltaY, range);
        return trajectory == null ? null : (trajectory.LandingX, trajectory.LandingY);
    }

    public ThrowTrajectory TraceThrow(int x, int y, int deltaX, int deltaY, int range = 6)
    {
        if (Math.Abs(deltaX) + Math.Abs(deltaY) != 1) return null;
        (int X, int Y)? landing = null;
        for (int step = 1; step <= range; step++)
        {
            int targetX = x + deltaX * step;
            int targetY = y + deltaY * step;
            if (!IsWalkable(targetX, targetY)) break;
            BaseNPC target = GetLivingNPCAt(targetX, targetY);
            if (target != null)
            {
                (int X, int Y) itemLanding = landing ?? (targetX, targetY);
                return new ThrowTrajectory(targetX, targetY,
                    itemLanding.X, itemLanding.Y, target);
            }
            if (GetGroundItemAt(targetX, targetY) == null && GetTrapAt(targetX, targetY) == null)
                landing = (targetX, targetY);
        }
        return landing is { } finalLanding
            ? new ThrowTrajectory(finalLanding.X, finalLanding.Y,
                finalLanding.X, finalLanding.Y, null)
            : null;
    }

    public bool DropItemNear(Item item, int x, int y, out (int X, int Y) position)
    {
        foreach ((int candidateX, int candidateY) in new[]
        {
            (x, y), (x, y - 1), (x + 1, y), (x, y + 1), (x - 1, y)
        })
        {
            if (GetTrapAt(candidateX, candidateY) != null) continue;
            if (!DropItem(item, candidateX, candidateY)) continue;
            position = (candidateX, candidateY);
            return true;
        }
        position = default;
        return false;
    }

    public bool CanPlaceTrap(int x, int y, Player player = null)
    {
        return IsWalkable(x, y)
            && !IsOccupiedByLivingNPC(x, y)
            && (player == null || player.X != x || player.Y != y)
            && GetGroundItemAt(x, y) == null
            && GetTrapAt(x, y) == null;
    }

    public bool PlaceTrap(int x, int y, int damage, Player player = null, TrapKind kind = TrapKind.Hunting)
    {
        if (!CanPlaceTrap(x, y, player)) return false;
        PlacedTraps.Add(new PlacedTrap(x, y, damage, kind));
        return true;
    }

    public PlacedTrap GetTrapAt(int x, int y)
    {
        return PlacedTraps.FirstOrDefault(trap => trap.X == x && trap.Y == y);
    }

    public bool RemoveTrap(PlacedTrap trap)
    {
        return trap != null && PlacedTraps.Remove(trap);
    }

    private void AddExits()
    {
        // start at bottom left and work upwards, try to hit a corridor or a room. If we can, we make
        // this trail a corridor. If we cant, we move along 1 more to the right, and repeat.

        // get the bottom-most corridor
        Corridor candidate = null;
        foreach (Room room in RoomList)
        {
            foreach (Corridor corridor in room.HCorridors)
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
                    room.HCorridors.Add(new Corridor(room.X2, hallY, neighbourRoom.X1 - 1, hallY));
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
                    room.VCorridors.Add(new Corridor(hallX, room.Y2, hallX, neighbourCell.Y1 - 1));
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
                    if (cell.X2 + 1 == other.X1)
                    {
                        if (Math.Max(cell.Y1, other.Y1) < Math.Min(cell.Y2, other.Y2))
                        {
                            cell.RightNeighbours.Add(other);
                        }
                    }
                    if (cell.Y2 + 1 == other.Y1)
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
