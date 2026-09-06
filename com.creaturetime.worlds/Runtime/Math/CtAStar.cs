
using System;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public static class CtAStar
    {
        public static bool TryGetPath(Vector2[] points, Vector2Int[] edges, int[] disabled, int start, int target, out int[] path)
        {
            var neighborLookUp = new int[points.Length][];
            for (var i = 0; i < points.Length; i++)
            {
                var neighbors = new int[] { };
                for (var j = 0; j < edges.Length; j++)
                {
                    var edge = edges[j];
                    if (edge.x == i || edge.y == i)
                    {
                        var neighborIndex = edge.x == i ? edge.y : edge.x;
                        CtArrayUtils.Insert(ref neighbors, neighborIndex, -1);
                    }
                }

                neighborLookUp[i] = neighbors;
            }

            var openSet = new DataList();
            var closedSet = new DataList();

            var gCosts = new float[points.Length];
            var fCosts = new float[points.Length]; // TODO: Expose this?
            var hCosts = new float[points.Length];
            var parents = new int[points.Length];

            openSet.Add(start);

            var targetPoint = points[target];

            while (openSet.Count > 0)
            {
                var currIndex = openSet[0].Int;
                for (int i = 1; i < openSet.Count; i++)
                {
                    var index = openSet[i].Int;

                    if (fCosts[index] < fCosts[currIndex] ||
                        (Mathf.Approximately(fCosts[index], fCosts[currIndex]) && hCosts[index] < hCosts[currIndex]))
                    {
                        currIndex = index;
                    }
                }

                openSet.Remove(currIndex);
                closedSet.Add(currIndex);

                if (currIndex == target)
                {
                    path = new int[] { };
                    var curr = target;
                    while (curr != start)
                    {
                        CtArrayUtils.Insert(ref path, curr, -1);
                        curr = parents[curr];
                    }

                    path = CtArrayUtils.Reverse(path);

                    return true;
                }

                var currPoint = points[currIndex];
                var neighbors = neighborLookUp[currIndex];

                for (var i = 0; i < neighbors.Length; i++)
                {
                    var neighborIndex = neighbors[i];
                    if (disabled != null && Array.IndexOf(disabled, neighborIndex) != -1) continue;
                    if (closedSet.Contains(neighborIndex)) continue;

                    var newCost = gCosts[neighborIndex] + Vector2.Distance(currPoint, points[neighborIndex]);

                    if (newCost < gCosts[neighborIndex] || !openSet.Contains(neighborIndex))
                    {
                        gCosts[neighborIndex] = newCost;
                        hCosts[neighborIndex] = Vector2.Distance(points[neighborIndex], targetPoint);
                        parents[neighborIndex] = currIndex;

                        if (!openSet.Contains(neighborIndex))
                            openSet.Add(neighborIndex);
                    }
                }
            }

            path = null;
            return false;
        }
    }
}