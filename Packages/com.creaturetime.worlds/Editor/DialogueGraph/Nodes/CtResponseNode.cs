
using System;
using System.Collections.Generic;
using CreatureTime;
using CreatureTime.Editor.Graph.DialogueGraph;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Response", "#545aa7", "Response", false, false)]
    public class CtResponseNode : CtDialogueNodeBase
    {
        [CtInputPortInfo("conditions", typeof(CtDialoguePortTypes.ConditionPort)), SerializeField]
        private string[] conditions;

        [CtInputPortInfo("consequences", typeof(CtDialoguePortTypes.ConsequencePort)), SerializeField]
        private string[] consequences;

        [CtOutputPortInfo("response", typeof(CtDialoguePortTypes.ResponsePort)), SerializeField]
        private string response;

        [CtExposedProperty] public string dialogue;
        [CtExposedProperty] public EDialogueChoiceType responseType;

        public override void Process(CtDialogueGraphAsset asset)
        {
            Debug.Log($"Response: {dialogue}");
            asset.CreateResponse(dialogue, responseType);

            foreach (var edge in asset.Edges)
            {
                if (edge.InputId == Guid)
                {
                    if (Array.IndexOf(conditions, edge.InputPortId) == -1)
                    {
                        continue;
                    }

                    if (!asset.TryGetNode(edge.OutputId, out var node))
                    {
                        continue;
                    }

                    node.Process(asset);
                }
            }

            foreach (var edge in asset.Edges)
            {
                if (edge.InputId == Guid)
                {
                    if (Array.IndexOf(consequences, edge.InputPortId) == -1)
                    {
                        continue;
                    }

                    if (!asset.TryGetNode(edge.OutputId, out var node))
                    {
                        continue;
                    }

                    node.Process(asset);
                }
            }
        }
    }
}
