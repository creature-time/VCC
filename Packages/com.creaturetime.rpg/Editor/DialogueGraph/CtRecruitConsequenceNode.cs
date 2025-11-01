
using System;
using CreatureTime.RpgGame;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Recruit", "#5aa754", "Recruit", false, false)]
    public class CtRecruitConsequenceNode : CtConsequenceNode
    {
        [CtExposedProperty, SerializeField] private ushort npcIdentifier;
        [CtExposedProperty, SerializeField] private bool recruitLeave;

        public override void Process(CtDialogueGraphAsset asset)
        {
            Debug.Log($"Recruit: {recruitLeave}");
            var recruitConsequence = asset.CreateConsequence<CtRecruitConsequence>();

            var so = new SerializedObject(recruitConsequence);
            so.FindProperty("dialogueActor").objectReferenceValue = asset.FindActor(npcIdentifier);
            so.FindProperty("recruitLeave").boolValue = recruitLeave;
            so.ApplyModifiedProperties();
        }
    }
}
