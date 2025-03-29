
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtPlayerManagerViewTest : CtAbstractSignal
    {
        [SerializeField] private CtPlayerManager playerManager;

        [SerializeField] private GameObject playerPrefab;

        private DataDictionary _playerPrefabs = new DataDictionary();

        private void Start()
        {
            playerManager.Connect(EPlayerManagerSignal.PlayerAdded, this, nameof(_OnPlayerAdded));
            playerManager.Connect(EPlayerManagerSignal.PlayerRemoved, this, nameof(_OnPlayerRemoved));

            playerPrefab.SetActive(false);
        }

        public void _OnPlayerAdded()
        {
            var index = GetArgs[0].Int;

            var prefab = Instantiate(playerPrefab, playerPrefab.transform.parent);
            prefab.SetActive(true);
            prefab.transform.position = new Vector3(index * 3, 10, 10);

            var playerDef = playerManager.GetPlayerDefByIndex(index);
            prefab.name = playerDef.DisplayName;

            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetTexture("_MainTex", playerDef.Icon);

            prefab.GetComponent<MeshRenderer>().SetPropertyBlock(props);
            _playerPrefabs.Add(index, prefab);
        }

        public void _OnPlayerRemoved()
        {
            var index = GetArgs[0].Int;
            if (_playerPrefabs.TryGetValue(index, out var token))
            {
                _playerPrefabs.Remove(index);

                var prefab = (GameObject)token.Reference;
                Destroy(prefab);
            }
        }
    }
}