using System;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Trigger Event", "#7851a9", "Trigger Event", true, true)]
    public class CtEventNode : CtDialogueNodeBase
    {
        [CtExposedProperty, SerializeField] private string triggerEvent;

        public override void Process(CtDialogueGraphAsset asset)
        {
            
        }
    }
}
