
using System;
using CreatureTime.RpgGame;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Set Recruit", "#5aa754", "Set Recruit", false, false)]
    public class CtRecruitConsequenceNode : CtConsequenceNode
    {
        [CtExposedProperty, SerializeField] private CtNpcDefData npc;
        [CtExposedProperty, SerializeField] private ERecruitResponseNodeType action;

        public CtRecruitConsequenceNode() { }

        public CtRecruitConsequenceNode(CtNpcDefData npc, ERecruitResponseNodeType action)
        {
            this.npc = npc;
            this.action = action;
        }

        public override void Process(CtDialogueGraphAsset asset)
        {
            Debug.Log($"Recruit: {action}");
            var recruitConsequence = asset.CreateConsequence<CtRecruitConsequence>();

            var so = new SerializedObject(recruitConsequence);
            so.FindProperty("npc").objectReferenceValue = CtRpgNodeGraphUtils.FindNpc(npc.Identifier);
            so.FindProperty("action").enumValueIndex = Convert.ToInt32(action);
            so.ApplyModifiedProperties();
        }
    }
}
