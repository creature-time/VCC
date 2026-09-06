
using System;
using CreatureTime.RpgGame;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Check Recruit", "#a7545a", "Check Recruit", false, false)]
    public class CtRecruitConditionNode : CtConditionNode
    {
        [CtExposedProperty, SerializeField] private CtNpcDefData npc;
        [CtExposedProperty, SerializeField] private ERecruitResponseNodeType action;

        public CtRecruitConditionNode() { }

        public CtRecruitConditionNode(CtNpcDefData npc, ERecruitResponseNodeType action)
        {
            this.npc = npc;
            this.action = action;
        }

        public override void Process(CtDialogueGraphAsset asset)
        {
            var recruitCondition = asset.CreateCondition<CtRecruitCondition>();

            var so = new SerializedObject(recruitCondition);
            so.FindProperty("npc").objectReferenceValue = CtRpgNodeGraphUtils.FindNpc(npc.Identifier);
            so.FindProperty("action").enumValueIndex = Convert.ToInt32(action);
            so.ApplyModifiedProperties();
        }
    }
}
