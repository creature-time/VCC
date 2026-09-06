
using System.Collections.Generic;
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtAbstractDefData : ScriptableObject
    {
        public abstract string GenerateName { get; }
        public abstract ushort Identifier { get; }
    }

    // public class CtDependencyGraph
    // {
    //     private CtAbstractDepNode[] nodes;
    //
    //     public void Run()
    //     {
    //         Dictionary<int, CtAbstractDepNode> nodeLookup = new Dictionary<int, CtAbstractDepNode>();
    //         foreach (var node in nodes)
    //         {
    //             nodeLookup.Add(node.Identifier, node);
    //         }
    //
    //         List<CtAbstractDepNode> orderedNodes = new List<CtAbstractDepNode>();
    //         foreach (var node in nodes)
    //         {
    //             if (!orderedNodes.Contains(node))
    //                 orderedNodes.Add(node);
    //
    //             var dependencies = node.Dependencies;
    //             if (dependencies.Length <= 0) continue;
    //
    //             foreach (var dependency in dependencies)
    //             {
    //                 if (!nodeLookup.TryGetValue(dependency, out var depNode)) continue;
    //                 if (orderedNodes.Contains(depNode)) continue;
    //
    //                 orderedNodes.Insert(orderedNodes.IndexOf(node), depNode);
    //             }
    //         }
    //
    //         
    //     }
    // }
    //
    // public abstract class CtAbstractDepNode
    // {
    //     public abstract int Identifier { get; }
    //     public abstract int[] Dependencies  { get; }
    //     public abstract bool Process(Dictionary<object, object> context);
    // }
}