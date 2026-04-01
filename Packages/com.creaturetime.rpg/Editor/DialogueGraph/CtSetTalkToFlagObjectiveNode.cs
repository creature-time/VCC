
using System;
using CreatureTime.RpgGame.Dialogue;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Set Talk To Objective", "#5aa754", "Set Talk To Objective", false, false)]
    public class CtSetTalkToFlagObjectiveNode : CtConsequenceNode
    {
        [CtExposedProperty, SerializeField] private CtDialogueActor actor;
        [CtExposedProperty, SerializeField] private string flag;
        [CtExposedProperty, SerializeField] private bool value;

        public override void Process(CtDialogueGraphAsset asset)
        {
            var questConsequence = asset.CreateConsequence<CtSetFlagObjective>();

            var so = new SerializedObject(questConsequence);
            so.FindProperty("actor").objectReferenceValue = actor;
            so.FindProperty("flag").stringValue = flag;
            so.FindProperty("value").boolValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
