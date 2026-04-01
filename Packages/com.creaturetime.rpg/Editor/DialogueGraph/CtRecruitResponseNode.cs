
using System;
using System.ComponentModel;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Recruit Response", "#856A00", "Recruit Response", false, false)]
    public class CtRecruitResponseNode : CtAbstractResponseNode
    {
        [CtExposedProperty, SerializeField] private CtNpcDefData npc;
        [CtExposedProperty, SerializeField] private ERecruitResponseNodeType action;
        [CtExposedProperty, SerializeField] private string dialogue;

        public override void Process(CtDialogueGraphAsset asset)
        {
            if (npc is null)
                throw new WarningException($"Quest must have a defined (asset={asset}, guid={Guid}).");

            Debug.Log($"Quest Response: {dialogue}");
            asset.CreateResponse(dialogue, EDialogueChoiceType.Recruit);

            var questCondition = new CtRecruitConditionNode(npc, action);
            questCondition.Process(asset);

            var questConsequence = new CtRecruitConsequenceNode(npc, action);
            questConsequence.Process(asset);

            ProcessConditions(asset);
            ProcessConsequences(asset);
        }
    }
}
