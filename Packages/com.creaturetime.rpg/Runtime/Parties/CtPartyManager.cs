
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EPartyManagerSignal
    {
        PartyStarted,
        PartyDisbanded
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtPartyManager : CtAbstractSignal
    {
        [SerializeField] private CtParty[] parties;

        private void Start()
        {
            for (int i = 0; i < parties.Length; i++)
            {
                var party = parties[i];
                party.Init((ushort)i);
                party.Connect(EPartySignal.Started, this, nameof(_OnPartyStarted));
                party.Connect(EPartySignal.Disbanded, this, nameof(_OnPartyDisbanded));
            }
        }

        public bool TryGetParty(ushort identifier, out CtParty party)
        {
            party = null;
            foreach (var other in parties)
            {
                if (other.Identifier == identifier)
                {
                    party = other;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetParty(CtEntity entity, out CtParty party)
        {
            party = null;
            if (!entity)
            {
                LogWarning("Entity is null");
                return false;
            }

            foreach (var other in parties)
            {
                if (other.HasMember(entity))
                {
                    party = other;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetAvailableParty(out CtParty party)
        {
            party = null;
            foreach (var other in parties)
            {
                if (other.IsEmpty)
                {
                    party = other;
                    return true;
                }
            }
            return false;
        }

        public void _OnPartyStarted()
        {
            var party = (CtParty)GetArgs[0].Reference;

            SetArgs.Add(party.Identifier);
            this.Emit(EPartyManagerSignal.PartyStarted);
        }

        public void _OnPartyDisbanded()
        {
            var party = (CtParty)GetArgs[0].Reference;

            SetArgs.Add(party.Identifier);
            this.Emit(EPartyManagerSignal.PartyDisbanded);
        }
    }
}