
using System;
using CreatureTime.Progression;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtQuestDef : CtAbstractDefinition
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private bool isPrimaryQuest;
        [SerializeField] private CtDialogueActor pickUpActor;
        [SerializeField] private CtDialogueActor turnInActor;
        [SerializeField] private string title;
        [TextArea, SerializeField] private string description;
        [SerializeField] private CtAbstractPrerequisite[] prerequisites;
        [SerializeField] private CtAbstractQuestObjective[] objectives;
        [SerializeField] private CtAbstractQuestReward[] rewards;

        public bool IsPrimaryQuest => isPrimaryQuest;
        public CtDialogueActor PickUpActor => pickUpActor;
        public CtDialogueActor TurnInActor => turnInActor;
        public string Title => title;
        public string Description => description;
        public CtAbstractPrerequisite[] Prerequisites => prerequisites;
        public CtAbstractQuestObjective[] Objectives => objectives;
        public CtAbstractQuestReward[] Rewards => rewards;

        public bool TryGetObjective(string checkFlag, out CtAbstractQuestObjective objective)
        {
            foreach (var obj in objectives)
            {
                if (obj.Flag != checkFlag) continue;
        
                objective = obj;
                return true;
            }
        
            objective = null;
            return false;
        }

        public bool TryGetObjectiveValue(ulong data, string checkFlag, out int value)
        {
            value = -1;
            if (!TryGetObjective(checkFlag, out var objective)) return false;
        
            value = CtDataBlock.GetQuestObjective(Array.IndexOf(Objectives, objective), data);
            return true;
        }

        public bool TrySetObjectiveValue(string checkFlag, int value, ref ulong data)
        {
            if (!TryGetObjective(checkFlag, out var objective)) return false;
        
            data = CtDataBlock.UpdateQuestObjective(Array.IndexOf(Objectives, objective), value, data);
        
            return true;
        }
    }
}