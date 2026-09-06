using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureTime.Editor.Graph
{
    [Serializable]
    public class CtGraph
    {
        [SerializeReference] private List<CtGraphNode> nodes = new List<CtGraphNode>();
        [SerializeField] private List<CtGraphEdge> edges = new List<CtGraphEdge>();

        public List<CtGraphNode> Nodes => nodes;
        public List<CtGraphEdge> Edges => edges;
    }
}
