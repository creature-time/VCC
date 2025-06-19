
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public enum EPartySignal
    {
        Started,
        Disbanded,
        MemberAdded,
        MemberRemoved,
        QuestChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtParty : CtAbstractSignal
    {
        [SerializeField] private CtPartyManager partyManager;

        [SerializeField, HideInInspector, UdonSynced] private ushort[] members;
        [SerializeField, HideInInspector] private ushort[] membersCmp;

        [UdonSynced, FieldChangeCallback(nameof(QuestCallback))]
        private ushort _questId = 0;

        public ushort QuestCallback
        {
            get => _questId;
            set
            {
                _questId = value;
                this.Emit(EPartySignal.QuestChanged);
            }
        }

        public ushort Quest
        {
            get => QuestCallback;
            set
            {
                QuestCallback = value;
                RequestSerialization();
            }
        }

        public ushort GetMemberId(int index)
        {
            return membersCmp[index];
        }

        private DataList _memberCache = new DataList();

        public ushort Identifier {get; private set; }
        public bool IsEmpty => _memberCache.Count == 0;
        public bool IsFull => _memberCache.Count == members.Length;
        public int Count => _memberCache.Count;
        public int MaxCount => members.Length;

        private void Start()
        {
            membersCmp = new ushort[members.Length];
            for (int i = 0; i < membersCmp.Length; i++)
                membersCmp[i] = CtConstants.InvalidId;
        }

        public void Init(ushort identifier)
        {
            Identifier = identifier;
        }

        public override void OnDeserialization()
        {
            base.OnDeserialization();

            for (int i = 0; i < members.Length; i++)
                if (membersCmp[i] != members[i])
                    _OnPartyMemberChanged(i);
        }

        private void _OnPartyMemberChanged(int index)
        {
            if (membersCmp[index] != CtConstants.InvalidId)
            {
                SetArgs.Add(index);
                this.Emit(EPartySignal.MemberRemoved);

                _memberCache.Remove(membersCmp[index]);

                if (_memberCache.Count == 0)
                {
                    SetArgs.Add(this);
                    this.Emit(EPartySignal.Disbanded);
                }
            }

            membersCmp[index] = members[index];

            if (membersCmp[index] != CtConstants.InvalidId)
            {
                if (_memberCache.Count == 0)
                {
                    SetArgs.Add(this);
                    this.Emit(EPartySignal.Started);
                }

                _memberCache.Add(membersCmp[index]);

                SetArgs.Add(index);
                this.Emit(EPartySignal.MemberAdded);
            }
        }

        private void _SetMemberId(int index, ushort identifier)
        {
            members[index] = identifier;
            RequestSerialization();
            _OnPartyMemberChanged(index);
        }

        public void Join(CtEntity entity)
        {
            int index = Array.IndexOf(members, CtConstants.InvalidId);
            if (index == -1)
            {
#if DEBUG_LOGS
                LogCritical("Cannot add anymore members to party.");
#endif
                return;
            }

            _SetMemberId(index, entity.Identifier);
        }

        public bool HasMember(CtEntity entity)
        {
            return _memberCache.IndexOf(entity.Identifier) != -1;
        }

        public void Clear()
        {
            for (int i = 0; i < members.Length; ++i)
                if (members[i] != CtConstants.InvalidId)
                    _SetMemberId(i, CtConstants.InvalidId);
        }

        public void Leave(CtEntity entity)
        {
            int index = Array.IndexOf(members, entity.Identifier);
            if (index == -1)
            {
#if DEBUG_LOGS
                LogCritical("Cannot find member to remove from party.");
#endif
                return;
            }

            _SetMemberId(index, CtConstants.InvalidId);
        }
    }
}