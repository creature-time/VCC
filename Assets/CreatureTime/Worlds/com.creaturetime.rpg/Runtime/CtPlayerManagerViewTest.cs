
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtPlayerManagerViewTest : CtAbstractSignal
    {
        [SerializeField] private CtPlayerPersistenceManager playerPersistenceManager;

        [SerializeField] private GameObject playerPrefab;

        private DataDictionary _playerPrefabs = new DataDictionary();

        private void Start()
        {
            playerPersistenceManager.Connect(EPlayerPersistenceManagerSignal.PlayerAdded, this, nameof(_OnPlayerAdded));
            playerPersistenceManager.Connect(EPlayerPersistenceManagerSignal.PlayerRemoved, this, nameof(_OnPlayerRemoved));

            playerPrefab.SetActive(false);
        }

        public void _OnPlayerAdded()
        {
            var playerWorldPersistenceData = (CtPlayerWorldPersistenceData)GetArgs[0].Reference;
            var playerPersistenceData = playerWorldPersistenceData.PlayerPersistenceData;
#if DEBUG_LOGS
            LogDebug("Player added " +
                     $"(worldData={playerWorldPersistenceData}, worldDataGuid={playerWorldPersistenceData.PlayerGuid}, " +
                     $"playerData={playerPersistenceData}, playerDataGuid={playerPersistenceData.PlayerGuid}).");
#endif

            var playerDef = (CtPlayerDef)playerPersistenceData.Extension;

            var playerEntity = playerWorldPersistenceData.GetComponent<CtPlayerEntity>();
            playerEntity.PlayerDef = playerDef;

            var prefab = Instantiate(playerPrefab, playerPrefab.transform.parent);
            prefab.SetActive(true);
            prefab.transform.position = new Vector3(playerEntity.Identifier * 3, 10, 10);
            prefab.name = $"{playerEntity.EntityDef.DisplayName} ({playerEntity.Identifier})";

            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetTexture("_MainTex", playerEntity.EntityDef.Icon);

            prefab.GetComponent<MeshRenderer>().SetPropertyBlock(props);
            _playerPrefabs.Add(playerEntity.Identifier, prefab);
        }

        public void _OnPlayerRemoved()
        {
            var playerWorldPersistenceData = (CtPlayerWorldPersistenceData)GetArgs[0].Reference;
            var playerEntity = playerWorldPersistenceData.GetComponent<CtPlayerEntity>();

            if (_playerPrefabs.TryGetValue(playerEntity.Identifier, out var token))
            {
                _playerPrefabs.Remove(playerEntity.Identifier);

                var prefab = (GameObject)token.Reference;
                Destroy(prefab);
            }
        }
    }
}