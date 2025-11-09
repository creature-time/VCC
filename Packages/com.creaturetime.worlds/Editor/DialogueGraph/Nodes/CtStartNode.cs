
using System;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Start", "#014421", "Start", false, true)]
    public class CtStartNode : CtDialogueNodeBase
    {
        public override void Process(CtDialogueGraphAsset asset)
        {
            Debug.Log("Start");
            asset.CreateConversation();

            if (asset.TryGetNodeFromOutput(Guid, $"flowoutput_{Guid}", out var node))
                node.Process(asset);
            //
            // foreach (var edge in asset.Graph.Edges)
            // {
            //     if (edge.OutputId == Guid && edge.OutputPortId == $"flowoutput_{Guid}")
            //     {
            //         if (!asset.TryGetNode(edge.InputId, out var node))
            //         {
            //             continue;
            //         }
            //
            //         node.Process(asset);
            //     }
            // }
        }
    }
}
