
using System;
using CreatureTime.RpgGame.Dialogue;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Set Quest Objective", "#5aa754", "Set Quest Objective", false, false)]
    public class CtSetFlagObjectiveNode : CtConsequenceNode
    {
        [CtExposedProperty, SerializeField] private string flag;
        [CtExposedProperty, SerializeField] private int value;

        public override void Process(CtDialogueGraphAsset asset)
        {
            var questConsequence = asset.CreateConsequence<CtSetFlagObjective>();

            var so = new SerializedObject(questConsequence);
            so.FindProperty("flag").stringValue = flag;
            so.FindProperty("value").intValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
