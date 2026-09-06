
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using Random = UnityEngine.Random;

namespace CreatureTime
{
    public enum EMapSignal
    {
        MapUpdated
    }

    public enum EMapPoiType
    {
        Rest,
        Easy,
        Medium,
        Hard,
        Boss
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtMap : CtAbstractSignal
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtGameData gameData;

        [UdonSynced] private int _width;
        [UdonSynced] private int _length;
        [UdonSynced] private Vector2[] nodes = { };
        [UdonSynced] private EMapPoiType[] poiType = { };
        [UdonSynced] private int[] paths = { };
        [UdonSynced] private int[] completed = { };
        [UdonSynced] private int current = -1;
        [UdonSynced] private ushort _destinationId = CtConstants.InvalidId;

        public ushort DestinationId
        {
            get => _destinationId;
            private set
            {
                _destinationId = value;
                RequestSerialization();
            }
        }

        private Vector2Int[] _edges = { };
        private int[] _availablePaths = { };

        public int Width => _width;
        public int Length => _length;

        public Vector2[] Nodes => nodes;
        public EMapPoiType[] PoiType => poiType;
        public int[] Completed => completed;
        public Vector2Int[] Edges => _edges;
        public int Current => current;
        public int Last => completed.Length > 0 ? completed[completed.Length - 1] : -1;

        private DataList _GenerateValidIndexes()
        {
            var mid = Width / 2;
            var result = new DataList();
            var center = new Vector2(mid, mid);
            for (var i = 0; i < Width * Length; i++)
            {
                var point = new Vector2(i % Width, Mathf.RoundToInt(i / (float)Width));
                var distanceFromCenter = Vector2.SqrMagnitude(point - center);
                if (distanceFromCenter <= Length * Length / 4f)
                    result.Add(i);
            }

            return result;
        }

        private void _GenerateNodes(int nodeCount, DataList nodeOptions)
        {
            var mid = _width / 2;

            nodeCount += 2;
            nodes = new Vector2[nodeCount];

            nodes[0] = new Vector2(mid, -1);
            nodes[1] = new Vector2(mid, _length);

            for (var i = 2; i < nodeCount; ++i)
            {
                var index = Random.Range(0, nodeOptions.Count);
                var nodeChoice = nodeOptions[index].Int;
                nodeOptions.RemoveAt(index);

                nodes[i] = new Vector2(nodeChoice % _width, Mathf.RoundToInt(nodeChoice / (float)_width));
            }
        }

        private Vector2Int[] _PurgeDuplicateEdges(Vector3Int[] triangles)
        {
            var edges = new Vector2Int[] { };
            foreach (var triangle in triangles)
            {
                var e1 = new Vector2Int(triangle.x, triangle.y);
                var e2 = new Vector2Int(triangle.y, triangle.z);
                var e3 = new Vector2Int(triangle.x, triangle.z);

                var index = Array.IndexOf(edges, e1);
                if (index == -1)
                    CtArrayUtils.Insert(ref edges, e1, -1);

                index = Array.IndexOf(edges, e2);
                if (index == -1)
                    CtArrayUtils.Insert(ref edges, e2, -1);

                index = Array.IndexOf(edges, e3);
                if (index == -1)
                    CtArrayUtils.Insert(ref edges, e3, -1);
            }

            return edges;
        }

        public void GenerateMap(CtLocationDef locationDef, int width, int length, int pathCount, int maxNodeCountOverride)
        {
            if (pathCount < 1)
            {
                LogWarning($"Path count must be greater than or equal to zero (pathCount={pathCount}).");
                pathCount = 1;
            }

            if (maxNodeCountOverride != -1 && maxNodeCountOverride < 1)
            {
                LogWarning("Max node count must be greater than or equal to zero " +
                           $"or -1 to not override the node count (maxNodeCount={maxNodeCountOverride}).");
                maxNodeCountOverride = 1;
            }

            DestinationId = locationDef.Identifier;

            _width = Mathf.Max(width, 1);
            _length = Mathf.Max(length, 1);

            var nodeOptions = _GenerateValidIndexes();

            var nodeCount = maxNodeCountOverride;
            if (nodeCount == -1)
                nodeCount = Mathf.Max(nodeOptions.Count / pathCount, 1);
            _GenerateNodes(nodeCount, nodeOptions);

            var triangles = CtTriangulationDelaunay2D.Calculate(nodes);
            var edges = _PurgeDuplicateEdges(triangles);

            var pathArray = new int[pathCount][];

            poiType = new EMapPoiType[nodes.Length];
            poiType[0] = EMapPoiType.Rest;
            poiType[1] = EMapPoiType.Rest;

            if (nodes.Length == 3)
            {
                pathArray[0] = new int[] { 0, 2, 1 };
            }
            else
            {
                var disabled = new int[] { };
                for (var i = 0; i < pathCount; ++i)
                {
                    if (!CtAStar.TryGetPath(nodes, edges, disabled, 0, 1, out var path)) continue;
                    if (path.Length <= 1) continue;

                    var poiTable = new float[5] { 0, 3, 1.5f, 0, 0 };
                    for (var j = 0; j < path.Length - 1; ++j)
                    {
                        var isLast = j >= path.Length - 2;
                        if (isLast)
                            poiTable[0] = 0;

                        var choice = CtRandomizer.GetRandomFromArray(poiTable);
                        poiType[path[j]] = (EMapPoiType)choice;

                        if (isLast) continue;

                        for (var k = 0; k < poiTable.Length; ++k)
                        {
                            if (choice == k)
                            {
                                if (k == 0)
                                    poiTable[k] = 0;
                                else
                                    poiTable[k] = 1;
                            }
                            else
                                poiTable[k] += 0.2f;
                        }
                    }

                    pathArray[i] = path;

                    var removeCount = Random.Range(0, 1000) % 2 + 1;
                    for (var j = 0; j < removeCount; j++)
                    {
                        var index = Random.Range(0, 1000) % (path.Length - 1);
                        CtArrayUtils.Insert(ref disabled, path[index], -1);
                    }
                }
            }

            var checkEdges = new Vector2Int[] { };
            for (var i = 0; i < pathArray.Length; i++)
            {
                var path = pathArray[i];
                if (path == null) break;

                var edge = new Vector2Int(0, path[0]);
                if (Array.IndexOf(checkEdges, edge) == -1)
                    CtArrayUtils.Insert(ref checkEdges, edge, -1);
                for (var j = 0; j < path.Length - 1; j++)
                {
                    edge = new Vector2Int(path[j], path[j + 1]);
                    if (Array.IndexOf(checkEdges, edge) == -1)
                        CtArrayUtils.Insert(ref checkEdges, edge, -1);
                }
            }

            paths = new int[checkEdges.Length * 2];
            for (var i = 0; i < checkEdges.Length; i++)
            {
                var edge = checkEdges[i];
                paths[i * 2] = edge.x;
                paths[i * 2 + 1] = edge.y;
            }

            // Remove unused nodes.
            for (var i = nodes.Length - 1; i >= 0; --i)
            {
                if (Array.IndexOf(paths, i) != -1) continue;
                CtArrayUtils.Pop(ref nodes, i);
                for (var j = 0; j < paths.Length; j++)
                {
                    if (paths[j] >= i)
                        paths[j]--;
                }
            }

            completed = new int[] { 0 };
            current = -1;

            RequestSerialization();
            OnDeserialization();
        }

        public void Clear()
        {
            _width = 0;
            _length = 0;
            nodes = new Vector2[] { };
            poiType = new EMapPoiType[] { };
            paths = new int[] { };
            completed = new  int[] { };
            current = -1;
            DestinationId = CtConstants.InvalidId;

            RequestSerialization();
            OnDeserialization();
        }

        public bool IsAvailablePath(int nextNodeId) => Array.IndexOf(_availablePaths, nextNodeId) != -1;

        public bool IsCompleted(int nodeId) => Array.IndexOf(completed, nodeId) != -1;

        public bool IsLastNode() => current == 1;

        public bool TryGoToNext(int nodeId)
        {
#if DEBUG_LOGS
            LogDebug($"GoTo next node (nodeId={nodeId}).");
#endif

            if (current != -1)
            {
#if DEBUG_LOGS
                LogWarning($"Queue was already set (queued={current}).");
#endif
                return false;
            }

            if (Array.IndexOf(_availablePaths, nodeId) == -1)
            {
#if DEBUG_LOGS
                LogWarning($"Node was not in available paths (nodeId={nodeId}).");
#endif
                return false;
            }

            current = nodeId;

            RequestSerialization();
            OnDeserialization();

            return true;
        }

        public void SetCompleted()
        {
            CtArrayUtils.Insert(ref completed, current, -1);
            current = -1;

            RequestSerialization();
            OnDeserialization();
        }

        public override void OnDeserialization()
        {
            var edgeCount = paths.Length / 2;
            _edges = new Vector2Int[edgeCount];
            for (var i = 0; i < edgeCount; i++)
            {
                _edges[i] = new Vector2Int(paths[i * 2], paths[i * 2 + 1]);
            }

            _availablePaths = new int[] { };
            var count = completed.Length;
            if (count > 0 && current == -1)
            {
                var last = completed[count - 1];
                foreach (var edge in _edges)
                    if (last == edge.x)
                        CtArrayUtils.Insert(ref _availablePaths, edge.y, -1);
            }

#if DEBUG_LOGS
            LogDebug($"Nodes: {CtArrayUtils.DebugToString(nodes)}");
            LogDebug($"Edges: {CtArrayUtils.DebugToString(_edges)}");
            LogDebug($"Available Paths: {CtArrayUtils.DebugToString(_availablePaths)}");
#endif

            this.Emit(EMapSignal.MapUpdated);
        }
    }
}