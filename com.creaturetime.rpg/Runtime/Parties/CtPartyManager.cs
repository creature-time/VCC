
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
    public class CtPartyManager : CtSingleton
    {
        [SerializeField, HideInInspector] private CtParty[] playerParties;
        [SerializeField, HideInInspector] private CtParty[] enemyParties;

        private DataDictionary _partyLookup = new DataDictionary();

        public override void Init()
        {
            for (ushort i = 0; i < playerParties.Length; i++)
            {
                var party = playerParties[i];
                party.Connect(EPartySignal.Started, this, nameof(_OnPlayerPartyStarted));
                party.Connect(EPartySignal.Disbanded, this, nameof(_OnPlayerPartyDisbanded));
                _partyLookup.Add(party.Identifier, party);
            }

            for (ushort i = 0; i < enemyParties.Length; i++)
            {
                var party = enemyParties[i];
                _partyLookup.Add(party.Identifier, party);
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

#if DEBUG_LOGS
                LogWarning($"Failed to find party by identifier (identifier={identifier}).");
#endif

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

            foreach (var other in enemyParties)
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

        // public bool TryGetConnectedParty(CtPlayerEntity entity, out CtParty party)
        // {
        //     foreach (var other in playerParties)
        //     {
        //         if (other.WasConnectedToParty(entity))
        //         {
        //             party = other;
        //             return true;
        //         }
        //     }
        //
        //     party = null;
        //     return false;
        // }

        public void _OnPlayerPartyStarted()
        {
            var party = (CtParty)Sender;

            SetArgs.Add(party);
            this.Emit(EPartyManagerSignal.PartyStarted);
        }

        public void _OnPlayerPartyDisbanded()
        {
            var party = (CtParty)Sender;

            SetArgs.Add(party);
            this.Emit(EPartyManagerSignal.PartyDisbanded);
        }
    }
}