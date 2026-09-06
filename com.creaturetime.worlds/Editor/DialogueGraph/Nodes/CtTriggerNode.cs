using System;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Trigger", "#003153", "Trigger", false, false)]
    public class CtTriggerNode : CtDialogueNodeBase
    {
        [SerializeField]
        [CtOutputPortInfo(null, typeof(CtDialoguePortTypes.TriggerPort))]
        private string trigger;

        [CtExposedProperty, SerializeField] public string path;
        [CtExposedProperty, SerializeField] public string eventTrigger;

        public override void Process(CtDialogueGraphAsset asset)
        {
            asset.CreateTrigger(path, eventTrigger);
        }
    }
}
