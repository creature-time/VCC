
using System;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Response", "#545aa7", "Response", false, false)]
    public class CtResponseNode : CtAbstractResponseNode
    {
        [CtExposedProperty, TextArea, SerializeField] private string dialogue;
        [CtExposedProperty, SerializeField] private EDialogueChoiceType responseType;

        public override void Process(CtDialogueGraphAsset asset)
        {
            Debug.Log($"Response: {dialogue}");
            asset.CreateResponse(dialogue, responseType);

            ProcessConditions(asset);
            ProcessConsequences(asset);
        }
    }
}
