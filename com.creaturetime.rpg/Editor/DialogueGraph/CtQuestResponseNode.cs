
using System;
using System.ComponentModel;
using CreatureTime.RpgGame;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Quest Response", "#856A00", "Quest Response", false, false)]
    public class CtQuestResponseNode : CtAbstractResponseNode
    {
        [CtExposedProperty, SerializeField] private CtQuestDefData quest;
        [CtExposedProperty, SerializeField] private EQuestDialogueAction action;
        [CtExposedProperty, SerializeField] private bool applyQuestAction = true;
        [CtExposedProperty, SerializeField] private string dialogue;                                        

        public override void Process(CtDialogueGraphAsset asset)
        {
            if (quest is null)
                throw new WarningException($"Quest must have a defined (asset={asset}, guid={Guid}).");

            var text = string.IsNullOrEmpty(dialogue) ? quest.Title : dialogue;
            var choiceType = action == EQuestDialogueAction.PickUp ? EDialogueChoiceType.QuestAccept : EDialogueChoiceType.QuestTurnIn;

            Debug.Log($"Quest Response: {dialogue}");
            asset.CreateResponse(text, choiceType);

            var questCondition = new CtQuestConditionNode(quest, action);
            questCondition.Process(asset);

            if (applyQuestAction)
            {
                var questConsequence = new CtQuestConsequenceNode(quest, action);
                questConsequence.Process(asset);
            }

            ProcessConditions(asset);
            ProcessConsequences(asset);
        }
    }
}
