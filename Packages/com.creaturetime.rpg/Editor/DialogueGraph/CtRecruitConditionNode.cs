
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
        [CtExposedProperty, SerializeField] private ushort npcIdentifier;
        [CtExposedProperty, SerializeField] private bool recruitLeave;

        public override void Process(CtDialogueGraphAsset asset)
        {
            Debug.Log($"Recruit: {recruitLeave}");
            var recruitCondition = asset.CreateCondition<CtRecruitCondition>();

            var so = new SerializedObject(recruitCondition);
            so.FindProperty("dialogueActor").objectReferenceValue = asset.FindActor(npcIdentifier);
            so.FindProperty("recruitLeave").boolValue = recruitLeave;
            so.ApplyModifiedProperties();
        }
    }
}
