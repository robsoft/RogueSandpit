using System.Collections.Generic;
using System;
using System.Linq;

namespace RogueSandpit.Models;

public static class Pathfinding
{
    /*
    public static List<(int, int)> AStar(Dungeon dungeon, int startX, int startY, int goalX, int goalY)
    {
        var openSet = new List<Node>();
        var closedSet = new HashSet<(int, int)>();
        var cameFrom = new Dictionary<(int, int), (int, int)>();

        openSet.Add(new Node(startX, startY, 0, Heuristic(startX, startY, goalX, goalY)));

        while (openSet.Count > 0)
        {
            openSet.Sort((a, b) => a.F.CompareTo(b.F));
            var current = openSet[0];
            openSet.RemoveAt(0);

            if (current.X == goalX && current.Y == goalY)
            {
                return ReconstructPath(cameFrom, (goalX, goalY));
            }

            closedSet.Add((current.X, current.Y));

            foreach (var neighbor in GetNeighbors(dungeon, current.X, current.Y))
            {
                if (closedSet.Contains(neighbor)) continue;

                var tentativeG = current.G + 1;
                var node = openSet.Find(n => n.X == neighbor.Item1 && n.Y == neighbor.Item2);
                if (node == null)
                {
                    node = new Node(neighbor.Item1, neighbor.Item2, tentativeG, Heuristic(neighbor.Item1, neighbor.Item2, goalX, goalY));
                    openSet.Add(node);
                    cameFrom[neighbor] = (current.X, current.Y);
                }
                else if (tentativeG < node.G)
                {
                    node.G = tentativeG;
                    node.F = node.G + node.H;
                    cameFrom[neighbor] = (current.X, current.Y);
                }
            }
        }

        return new List<(int, int)>(); // No path
    }

    private static int Heuristic(int x1, int y1, int x2, int y2)
    {
        return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
    }

    private static List<(int, int)> GetNeighbors(Dungeon dungeon, int x, int y)
    {
        var neighbors = new List<(int, int)>();
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };
        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];
            if (dungeon.IsWalkable(nx, ny) && !dungeon.Enemies.Any(e => e.X == nx && e.Y == ny))
            {
                neighbors.Add((nx, ny));
            }
        }
        return neighbors;
    }

    private static List<(int, int)> ReconstructPath(Dictionary<(int, int), (int, int)> cameFrom, (int, int) current)
    {
        var path = new List<(int, int)>();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }

    private class Node
    {
        public int X, Y, G, H, F;
        public Node(int x, int y, int g, int h)
        {
            X = x; Y = y; G = g; H = h; F = g + h;
        }
    }
    */
}