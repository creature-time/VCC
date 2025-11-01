
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

            foreach (var edge in asset.Edges)
            {
                if (edge.OutputId == Guid && edge.OutputPortId == $"flowoutput_{Guid}")
                {
                    if (!asset.TryGetNode(edge.InputId, out var node))
                    {
                        continue;
                    }

                    node.Process(asset);
                }
            }
        }
    }
}
