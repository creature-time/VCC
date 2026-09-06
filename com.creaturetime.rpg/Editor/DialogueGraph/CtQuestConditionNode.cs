
using System;
using CreatureTime.RpgGame;
using CreatureTime.RpgGame.Dialogue;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Check Quest", "#a7545a", "Check Quest", false, false)]
    public class CtQuestConditionNode : CtConditionNode
    {
        [CtExposedProperty, SerializeField] private CtQuestDefData quest;
        [CtExposedProperty, SerializeField] private EQuestDialogueAction action;

        public CtQuestConditionNode()
        {
            
        }

        public CtQuestConditionNode(CtQuestDefData quest, EQuestDialogueAction action)
        {
            this.quest = quest;
            this.action = action;
        }

        public override void Process(CtDialogueGraphAsset asset)
        {
            var questCondition = asset.CreateCondition<CtQuestCondition>();

            var so = new SerializedObject(questCondition);
            so.FindProperty("quest").objectReferenceValue = CtRpgNodeGraphUtils.FindQuest(quest.Identifier);
            so.FindProperty("action").enumValueIndex = Convert.ToInt32(action);
            so.ApplyModifiedProperties();
        }
    }
}
