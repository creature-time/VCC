
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

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
        [SerializeField, HideInInspector] private CtParty[] playerParties;
        [SerializeField, HideInInspector] private CtParty[] enemyParties;

        private DataDictionary _partyLookup = new DataDictionary();

        public void Init()
        {
            ushort identifier = 0;
            for (ushort i = 0; i < playerParties.Length; i++)
            {
                var party = playerParties[i];
                party.Init(identifier);
                party.Connect(EPartySignal.Started, this, nameof(_OnPlayerPartyStarted));
                party.Connect(EPartySignal.Disbanded, this, nameof(_OnPlayerPartyDisbanded));
                _partyLookup.Add(identifier, party);
                identifier++;
            }

            for (ushort i = 0; i < enemyParties.Length; i++)
            {
                var party = enemyParties[i];
                party.Init(identifier);
                _partyLookup.Add(identifier, party);
                identifier++;
            }
        }

        public bool TryGetParty(ushort identifier, out CtParty party)
        {
            party = null;
            if (_partyLookup.TryGetValue(identifier, out var token))
            {
                party = (CtParty)token.Reference;
                return true;
            }
            return false;
        }

        public bool TryGetEntityParty(CtEntity entity, out CtParty party)
        {
            party = null;
            if (!entity)
            {
#if DEBUG_LOGS
                LogWarning("Entity is null");
#endif
                return false;
            }

            foreach (var other in playerParties)
            {
                if (other.HasMember(entity))
                {
                    party = other;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetAvailablePlayerParty(out CtParty party)
        {
            party = null;
            foreach (var other in playerParties)
            {
                if (other.IsEmpty)
                {
                    party = other;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetAvailableEnemyParty(out CtParty party)
        {
            party = null;
            foreach (var other in enemyParties)
            {
                if (other.IsEmpty)
                {
                    party = other;
                    return true;
                }
            }
            return false;
        }

        public void _OnPlayerPartyStarted()
        {
            var party = (CtParty)GetArgs[0].Reference;

            SetArgs.Add(party.Identifier);
            this.Emit(EPartyManagerSignal.PartyStarted);
        }

        public void _OnPlayerPartyDisbanded()
        {
            var party = (CtParty)GetArgs[0].Reference;

            SetArgs.Add(party.Identifier);
            this.Emit(EPartyManagerSignal.PartyDisbanded);
        }
    }
}