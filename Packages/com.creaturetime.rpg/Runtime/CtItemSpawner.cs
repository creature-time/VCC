
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtItemSpawner : CtAbstractSignal
    {
        [SerializeField] private CtGameData data;
        [SerializeField] private CtEntityDef entityDef;
        [SerializeField] private CtPlayerTurn playerTurn;
        [SerializeField] private VRCPickup playerWeapon;
        [SerializeField] private Transform weaponSpawner;

        private CtWeaponAttack _spawnedMainHandWeapon;

        void Start()
        {
            entityDef.Connect(EEntityStatsSignal.MainHandChanged, this, nameof(OnMainHandChanged));

            OnMainHandChanged();
        }

        public void OnMainHandChanged()
        {
            if (_spawnedMainHandWeapon)
            {
                Destroy(_spawnedMainHandWeapon.gameObject);
                _spawnedMainHandWeapon = null;
            }

            ulong mainHandWeapon = entityDef.MainHandWeapon;
            if (CtDataBlock.IsValid(mainHandWeapon))
            {
                ushort weaponId = CtDataBlock.GetWeaponIdentifier(entityDef.MainHandWeapon);
                CtWeaponDef weaponDef = data.GetWeaponDef(weaponId);
                var userData = weaponDef.UserData;
                Debug.Log($"weaponId {userData}");
                if (userData)
                {
                    playerWeapon.InteractionText = weaponDef.DisplayName;
                    var interactCollider = playerWeapon.GetComponent<BoxCollider>();

                    playerWeapon.GetComponent<MeshFilter>().sharedMesh = userData.GetComponent<MeshFilter>().sharedMesh;
                    playerWeapon.GetComponent<MeshRenderer>().material = userData.GetComponent<MeshRenderer>().material;

                    _spawnedMainHandWeapon = Instantiate(userData.gameObject, playerWeapon.transform).GetComponent<CtWeaponAttack>();

                    _spawnedMainHandWeapon.gameObject.SetActive(true);
                    _spawnedMainHandWeapon.PlayerTurn = playerTurn;

                    var colliders = _spawnedMainHandWeapon.GetComponents<BoxCollider>();
                    interactCollider.center = colliders[1].center;
                    interactCollider.size = colliders[1].size;

                    _spawnedMainHandWeapon.transform.localPosition = Vector3.zero;
                    _spawnedMainHandWeapon.transform.localRotation = Quaternion.identity;
                    _spawnedMainHandWeapon.transform.localScale = Vector3.one;

                    _RespawnWeapon();
                }
            }

            if (!_spawnedMainHandWeapon)
            {
                playerWeapon.InteractionText = null;
            }

            playerWeapon.gameObject.SetActive(_spawnedMainHandWeapon);
        }

        private void _RespawnWeapon()
        {
            if (!Networking.IsOwner(gameObject))
                return;

            Vector3 position = Networking.LocalPlayer.GetRotation() * Vector3.forward;
            position += Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Head);
            weaponSpawner.position = position;
            weaponSpawner.rotation = Networking.LocalPlayer.GetRotation();

            var objectSync = playerWeapon.GetComponent<VRCObjectSync>();
            objectSync.FlagDiscontinuity();
            objectSync.TeleportTo(weaponSpawner);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                _RespawnWeapon();
            }
        }
    }
}