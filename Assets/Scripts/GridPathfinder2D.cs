using System;
using System.Collections.Generic;
using UnityEngine;

// A* algorithm to search the grid to determine which is the "best" path.
public sealed class GridPathfinder2D
{
    private readonly Rect bounds;
    private readonly float cellSize;
    private readonly int columns;
    private readonly int rows;
    private readonly Func<Vector2, Vector2, bool> canTravel;

    public GridPathfinder2D(Rect bounds, float cellSize, Func<Vector2, Vector2, bool> canTravel)
    {
        this.bounds = bounds;
        this.cellSize = Mathf.Max(1f, cellSize);
        columns = Mathf.FloorToInt(bounds.width / this.cellSize) + 1;
        rows = Mathf.FloorToInt(bounds.height / this.cellSize) + 1;
        this.canTravel = canTravel;

        // Debug.Log($"Created grid size: {columns} columns x {rows} rows. Total nodes: {columns * rows}");
    }

    public bool FindPath(Vector2 start, Vector2 goal, List<Vector2> path)
    {
        path.Clear();

        if (!Contains(start) || !Contains(goal) || columns <= 0 || rows <= 0 ||
            (long)columns * rows > 16384) // max grid size, 128 * 128. If more than then the pathfinding algorithm will fail. 
        {
            // Debug.LogWarning("grid size exceeds the 128*128i");
            return false;
        }

        // get start and goal index as integers/grid box
        int startIndex = FindConnectingNode(start);
        int goalIndex = FindConnectingNode(goal);

        if (startIndex < 0 || goalIndex < 0)
        {
            //Debug.LogWarning($"{startIndex} Goal {goalIndex}");
            return false;
        }

        // init tracking arrays for path scoring and history
        int count = columns * rows;
        var costs = new float[count];
        var parents = new int[count];
        var closed = new bool[count];
        var inOpen = new bool[count];
        var open = new List<int> { startIndex };

        for (int i = 0; i < count; i++)
        {
            costs[i] = float.PositiveInfinity;
            parents[i] = -1;
        }

        // Start node costs 0 at first
        costs[startIndex] = 0f;
        inOpen[startIndex] = true;
        Vector2 gridGoal = Position(goalIndex);


        while (open.Count > 0)
        {
            // Find the node in the open list with the absolute lowest F score (G cost + H heuristic)
            int best = 0;
            float bestScore = float.PositiveInfinity;
            for (int i = 0; i < open.Count; i++)
            {
                int candidate = open[i];
                float score = costs[candidate] + Vector2.Distance(Position(candidate), gridGoal);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }

            int current = open[best];
            open.RemoveAt(best);
            inOpen[current] = false;

            if (current == goalIndex)
            {
                // Reconstruct the final route by tracking backwards through the parent node chain
                for (int node = current; node >= 0; node = parents[node])
                    path.Add(Position(node));

                path.Reverse();
                path.Add(goal);
                return true;
            }

            closed[current] = true;

            // Convert the 1D index back into 2D X and Y coords
            int x = current % columns;
            int y = current / columns;


            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if ((dx == 0 && dy == 0) || nx < 0 || nx >= columns || ny < 0 || ny >= rows)
                        continue;

                    int next = ny * columns + nx;

                    float cost = costs[current] + Vector2.Distance(Position(current), Position(next));

                    // Skip is closed, the new path is more expensive, or if blocked (obstacles)
                    if (closed[next] || cost >= costs[next] || !canTravel(Position(current), Position(next)))
                        continue;

                    costs[next] = cost;
                    parents[next] = current;

                    if (!inOpen[next])
                    {
                        open.Add(next);
                        inOpen[next] = true;
                    }
                }
            }
        }

        return false;
    }

    // function to select the closest grid tile the goomba can move to
    private int FindConnectingNode(Vector2 point)
    {
        int centerX = Mathf.RoundToInt((point.x - bounds.xMin) / cellSize);
        int centerY = Mathf.RoundToInt((point.y - bounds.yMin) / cellSize);
        int best = -1;
        float bestDistance = float.PositiveInfinity;

        // Check closest 3x3 area around the point to find the best entry tile
        for (int y = Mathf.Max(0, centerY - 1); y <= Mathf.Min(rows - 1, centerY + 1); y++)
        {
            for (int x = Mathf.Max(0, centerX - 1); x <= Mathf.Min(columns - 1, centerX + 1); x++)
            {
                int index = y * columns + x;
                Vector2 position = Position(index);
                float distance = (position - point).sqrMagnitude; // sqrMagnitude is faster than Distance()

                if (distance < bestDistance && canTravel(point, position))
                {
                    best = index;
                    bestDistance = distance;
                }
            }
        }
        return best;
    }

    // Helper to convert a flat 1D array index back into a real 2D Unity World Vector position
    private Vector2 Position(int index)
    {
        return new Vector2(bounds.xMin + index % columns * cellSize,
            bounds.yMin + index / columns * cellSize);
    }

    private bool Contains(Vector2 point)
    {
        return point.x >= bounds.xMin && point.x <= bounds.xMax &&
            point.y >= bounds.yMin && point.y <= bounds.yMax;
    }
}
