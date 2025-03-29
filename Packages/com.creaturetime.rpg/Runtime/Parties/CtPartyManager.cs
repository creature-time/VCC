
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
        [SerializeField, HideInInspector] private CtParty[] playerParties;
        [SerializeField, HideInInspector] private CtParty[] enemyParties;

        public void Init()
        {
            for (int i = 0; i < playerParties.Length; i++)
            {
                var party = playerParties[i];
                party.Init((ushort)i);
                party.Connect(EPartySignal.Started, this, nameof(_OnPlayerPartyStarted));
                party.Connect(EPartySignal.Disbanded, this, nameof(_OnPlayerPartyDisbanded));
            }

            for (int i = 0; i < enemyParties.Length; i++)
                enemyParties[i].Init((ushort)i);
        }

        public bool TryGetParty(ushort identifier, out CtParty party)
        {
            party = null;
            foreach (var other in playerParties)
            {
                if (other.Identifier == identifier)
                {
                    party = other;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetEntityParty(CtEntity entity, out CtParty party)
        {
            party = null;
            if (!entity)
            {
                LogWarning("Entity is null");
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