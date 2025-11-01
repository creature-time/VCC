using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Dialogue", "#0c2340", "Dialogue")]
    public class CtDialogueNode : CtDialogueNodeBase
    {
        [SerializeField]
        [CtInputPortInfo(null, typeof(CtDialoguePortTypes.ResponsePort))]
        [CtOutputPortInfo(null, typeof(CtGraphPortTypes.FlowPort))]
        private string[] responses;

        [CtExposedProperty, SerializeField] private string dialogue;
        [CtExposedProperty, SerializeField] private CtDialogueActor actor;
        [CtExposedProperty, SerializeField] private CtDialogueActor conversant;

        public override void Process(CtDialogueGraphAsset asset)
        {
            Debug.Log($"Dialogue: {dialogue}");
            if (asset.CreateDialogue(Guid,
                dialogue,
                actor ? actor.Identifier : CtConstants.InvalidId,
                conversant ? conversant.Identifier : CtConstants.InvalidId))
            {
                if (asset.TryGetNodeFromOutput(Guid, $"flowoutput_{Guid}", out var node))
                    node.Process(asset);

                foreach (var response in responses)
                {
                    if (asset.TryGetNodeFromInput(Guid, response, out node))
                        node.Process(asset);
                    if (asset.TryGetNodeFromOutput(Guid, response, out node))
                        node.Process(asset);
                }
            }

            asset.PopDialogue();
        }
    }
}
