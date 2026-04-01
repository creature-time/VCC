
using System;
using CreatureTime.RpgGame;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Accept Quest", "#5aa754", "Accept Quest", false, false)]
    public class CtQuestConsequenceNode : CtConsequenceNode
    {
        [CtExposedProperty, SerializeField] private CtQuestDefData quest;
        [CtExposedProperty, SerializeField] private EQuestDialogueAction action;

        public CtQuestConsequenceNode()
        {
            
        }

        public CtQuestConsequenceNode(CtQuestDefData quest, EQuestDialogueAction action)
        {
            this.quest = quest;
            this.action = action;
        }

        public override void Process(CtDialogueGraphAsset asset)
        {
            var questConsequence = asset.CreateConsequence<CtQuestConsequence>();

            var so = new SerializedObject(questConsequence);
            so.FindProperty("quest").objectReferenceValue = CtRpgNodeGraphUtils.FindQuest(quest.Identifier);
            so.FindProperty("action").enumValueIndex = Convert.ToInt32(action);
            so.ApplyModifiedProperties();
        }
    }
}
