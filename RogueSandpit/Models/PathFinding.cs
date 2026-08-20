using System;
using System.Collections.Generic;

namespace RogueSandpit.Models;

public static class Pathfinding
{
    private static readonly (int X, int Y)[] Directions =
    [
        (0, -1),
        (-1, 0),
        (1, 0),
        (0, 1)
    ];

    public static List<(int X, int Y)> FindPath(
        Map map,
        int startX,
        int startY,
        int goalX,
        int goalY,
        BaseNPC movingNpc = null)
    {
        var start = (X: startX, Y: startY);
        var goal = (X: goalX, Y: goalY);
        if (start == goal) return [];
        if (!map.CanNpcEnter(goalX, goalY, movingNpc, allowClosedDoor: movingNpc != null)) return [];

        var frontier = new PriorityQueue<(int X, int Y), int>();
        var cameFrom = new Dictionary<(int X, int Y), (int X, int Y)>();
        var costSoFar = new Dictionary<(int X, int Y), int> { [start] = 0 };
        frontier.Enqueue(start, Heuristic(start, goal));

        while (frontier.TryDequeue(out var current, out _))
        {
            if (current == goal)
            {
                return ReconstructPath(cameFrom, start, goal);
            }

            foreach ((int dx, int dy) in Directions)
            {
                var next = (X: current.X + dx, Y: current.Y + dy);
                if (!CanEnter(map, next, movingNpc)) continue;

                int newCost = costSoFar[current] + 1;
                if (costSoFar.TryGetValue(next, out int existingCost) && newCost >= existingCost) continue;

                costSoFar[next] = newCost;
                cameFrom[next] = current;
                frontier.Enqueue(next, newCost + Heuristic(next, goal));
            }
        }

        return [];
    }

    private static bool CanEnter(
        Map map,
        (int X, int Y) position,
        BaseNPC movingNpc)
    {
        return map.CanNpcEnter(position.X, position.Y, movingNpc, allowClosedDoor: movingNpc != null);
    }

    private static int Heuristic((int X, int Y) from, (int X, int Y) to)
    {
        return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
    }

    private static List<(int X, int Y)> ReconstructPath(
        Dictionary<(int X, int Y), (int X, int Y)> cameFrom,
        (int X, int Y) start,
        (int X, int Y) goal)
    {
        var path = new List<(int X, int Y)>();
        var current = goal;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }
}
