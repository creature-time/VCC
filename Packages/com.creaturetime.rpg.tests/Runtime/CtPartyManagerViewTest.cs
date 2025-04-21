
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtPartyManagerViewTest : CtAbstractSignal
    {
        [SerializeField] private CtPartyManager partyManager;
        [SerializeField] private CtEntityManager entityManager;

        [SerializeField] private GameObject partyPrefab;

        private DataDictionary _partyPrefabs = new DataDictionary();
        private int _count;

        private void Start()
        {
            partyManager.Connect(EPartyManagerSignal.PartyStarted, this, nameof(_OnPartyStarted));
            partyManager.Connect(EPartyManagerSignal.PartyDisbanded, this, nameof(_OnPartyDisbanded));

            partyPrefab.SetActive(false);
        }

        public void _OnPartyStarted()
        {
            var identifier = GetArgs[0].UShort;

            partyManager.TryGetParty(identifier, out var party);
            party.Connect(EPartySignal.MemberAdded, this, nameof(_OnMemberAdded));
            party.Connect(EPartySignal.MemberRemoved, this, nameof(_OnMemberRemoved));
        }

        public void _OnPartyDisbanded()
        {
            var identifier = GetArgs[0].UShort;

            partyManager.TryGetParty(identifier, out var party);
            party.Disconnect(EPartySignal.MemberAdded, this, nameof(_OnMemberAdded));
            party.Disconnect(EPartySignal.MemberRemoved, this, nameof(_OnMemberRemoved));
        }

        public void _OnMemberAdded()
        {
            var party = (CtParty)Sender;
            var index = GetArgs[0].Int;

            var prefab = Instantiate(partyPrefab, partyPrefab.transform.parent);
            prefab.SetActive(true);
            prefab.transform.position = new Vector3(party.Identifier * 3, 7 + -index, 10);

            ushort identifier = party.GetMemberId(index);
            if (!entityManager.TryGetEntity(identifier, out var entity))
            {
                LogCritical($"[_OnMemberAdded] Failed to find entity (identifier={identifier}).");
                return;
            }

            _partyPrefabs.Add(identifier, prefab);

            _OnIdentifierChangedRaw(entity);

            entity.Connect(EEntitySignal.EntityStatsChanged, this, nameof(_OnIdentifierChanged));
        }

        public void _OnIdentifierChangedRaw(CtEntity entity)
        {
            Debug.Log($"entity.EntityStats {entity.EntityDef}");
            if (!entity.EntityDef)
                return;

            if (!_partyPrefabs.TryGetValue(entity.Identifier, out var token))
            {
                LogCritical($"[_OnIdentifierChangedRaw] Failed to find entity (identifier={entity.Identifier}).");
                return;
            }

            var prefab = (GameObject)token.Reference;
            prefab.name = entity.DisplayName;

            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetTexture("_MainTex", entity.Icon);

            prefab.GetComponent<MeshRenderer>().SetPropertyBlock(props);
        }

        public void _OnIdentifierChanged()
        {
            var entity = (CtEntity)Sender;
            _OnIdentifierChangedRaw(entity);
        }

        public void _OnMemberRemoved()
        {
            var party = (CtParty)Sender;
            var index = GetArgs[0].Int;

            ushort identifier = party.GetMemberId(index);
            if (!entityManager.TryGetEntity(identifier, out var entity))
            {
                LogCritical($"[_OnMemberRemoved] Failed to find entity (identifier={identifier}).");
                return;
            }

            entity.Disconnect(EEntitySignal.IdentifierChanged, this, nameof(_OnIdentifierChanged));

            if (!_partyPrefabs.TryGetValue(identifier, out var token))
            {
                LogCritical($"[_OnMemberRemoved] Failed to find prefab (identifier={identifier}).");
                return;
            }

            var prefab = (GameObject)token.Reference;
            Destroy(prefab);

            _partyPrefabs.Remove(identifier);
        }
    }
}