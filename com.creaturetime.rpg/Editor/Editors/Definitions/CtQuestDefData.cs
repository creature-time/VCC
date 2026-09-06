
using System;
using UnityEngine;

namespace CreatureTime.Editor
{
    public enum EQuestObjectType
    {
        Flag,
        Kill,
        TalkTo
    }

    [Serializable]
    public class CtQuestObjectiveData
    {
        [SerializeField] private ushort identifier;
        [SerializeField] private EQuestObjectType questType;
        [SerializeField] private string flag;
        [SerializeField] private string description;
        [SerializeField] private CtNpcDefData killTarget;
        [SerializeField] private int killCount = 1;
        [SerializeField] private CtActor talkTo;

        public int Identifier => identifier;
        public EQuestObjectType QuestType => questType;
        public string Flag => flag;
        public string Description => description;
        public CtNpcDefData KillTarget => killTarget;
        public int KillCount => killCount;
        public CtActor TalkTo => talkTo;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "questData", menuName = "CreatureTime/Rpg/Quest Definition", order = 1)]
    public class CtQuestDefData : CtAbstractDefData
    {
        public override string GenerateName => 
            $"{identifier:00000}_{(string.IsNullOrEmpty(title) ? "NoName" : title.Replace(' ', '-'))}";

        [SerializeField] private ushort identifier;
        [SerializeField] private bool isPrimaryQuest;
        [SerializeField] private CtActor pickUpActor;
        [SerializeField] private CtActor turnInActor;
        [SerializeField] private string title;
        [SerializeField] private string description;
        [SerializeField, Range(1, 20)] private int levelReq = 1;
        [SerializeField] private CtQuestDefData[] questReq;
        [SerializeField] private CtQuestObjectiveData[] objectives;
        [SerializeField] private int expReward;
        [SerializeField] private int currencyReward;
        [CtItem, SerializeField] private string[] itemRewards;

        public override ushort Identifier => identifier;
        public bool IsPrimaryQuest => isPrimaryQuest;
        public CtActor PickUpActor => pickUpActor;
        public CtActor TurnInActor => turnInActor;
        public string Title => title;
        public string Description => description;
        public int LevelReq => levelReq;
        public CtQuestDefData[] QuestReq => questReq;
        public CtQuestObjectiveData[] Objectives => objectives;
        public int ExpReward => expReward;
        public int CurrencyReward => currencyReward;
        public string[] ItemRewards => itemRewards;
    }
}