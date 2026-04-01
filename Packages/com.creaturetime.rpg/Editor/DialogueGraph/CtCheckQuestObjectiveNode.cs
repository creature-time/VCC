
using System;
using CreatureTime.RpgGame.Dialogue;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Check Quest Objective", "#a7545a", "Check Quest Objective", false, false)]
    public class CtCheckQuestObjectiveNode : CtConditionNode
    {
        [CtExposedProperty, SerializeField] private CtQuestDefData quest;
        [CtExposedProperty, SerializeField] private string flag;
        [CtExposedProperty, SerializeField] private EMathExpression expression;
        [CtExposedProperty, SerializeField] private int value;

        public override void Process(CtDialogueGraphAsset asset)
        {
            var questCondition = asset.CreateCondition<CtCheckQuestObjective>();

            var so = new SerializedObject(questCondition);
            so.FindProperty("quest").objectReferenceValue = CtRpgNodeGraphUtils.FindQuest(quest.Identifier);
            so.FindProperty("flag").stringValue = flag;
            so.FindProperty("expression").enumValueIndex = Convert.ToInt32(expression);
            so.FindProperty("value").intValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
