
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public enum EPartySignal
    {
        Started,
        Disbanded,
        MemberAdded,
        MemberRemoved,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtParty : CtAbstractSignal
    {
        [SerializeField] private CtPartyManager partyManager;

        [SerializeField, HideInInspector, UdonSynced] private ushort[] members;
        [SerializeField, HideInInspector] private ushort[] membersCmp;

        public ushort GetMemberId(int index)
        {
            return membersCmp[index];
        }

        private DataList _entityCache = new DataList();

        public ushort Identifier {get; private set; }
        public bool IsEmpty => _entityCache.Count == 0;
        public bool IsFull => _entityCache.Count == members.Length;

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

                _entityCache.Remove(membersCmp[index]);

                if (_entityCache.Count == 0)
                {
                    SetArgs.Add(this);
                    this.Emit(EPartySignal.Disbanded);
                }
            }

            membersCmp[index] = members[index];

            if (membersCmp[index] != CtConstants.InvalidId)
            {
                if (_entityCache.Count == 0)
                {
                    SetArgs.Add(this);
                    this.Emit(EPartySignal.Started);
                }

                _entityCache.Add(membersCmp[index]);

                SetArgs.Add(index);
                this.Emit(EPartySignal.MemberAdded);
            }
        }

        private void _SetMemberId(int index, ushort entityId)
        {
            members[index] = entityId;
            RequestSerialization();
            _OnPartyMemberChanged(index);
        }

        public void Join(CtEntity entity)
        {
            int index = Array.IndexOf(members, CtConstants.InvalidId);
            if (index == -1)
            {
                CtLogger.LogCritical("Party", "Cannot add anymore members to party.");
                return;
            }

            _SetMemberId(index, entity.Identifier);
        }

        public bool HasMember(CtEntity entity)
        {
            return _entityCache.IndexOf(entity.Identifier) != -1;
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
                CtLogger.LogCritical("Party", "Cannot find member to remove from party.");
                return;
            }

            _SetMemberId(index, CtConstants.InvalidId);
        }
    }
}