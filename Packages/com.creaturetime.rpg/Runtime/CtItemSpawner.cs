
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace CreatureTime.RpgGame.Ui
{
    public enum EItemSpawnerSignal
    {
        PickupChange
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtItemSpawner : CtAbstractSignal
    {
        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtEntityDef entityDef;
        [SerializeField] private CtPlayerTurn playerTurn;
        [SerializeField] private VRCPickup playerWeapon;
        [SerializeField] private Transform weaponSpawner;

        private CtWeaponAttack _spawnedMainHandWeapon;
        private VRC_Pickup.PickupHand _pickupHand;

        void Start()
        {
            entityDef.Connect(EEntityDefSignal.MainHandChanged, this, nameof(OnMainHandChanged));
            OnMainHandChanged();
        }

        public void OnMainHandChanged()
        {
            if (_spawnedMainHandWeapon)
            {
                Destroy(_spawnedMainHandWeapon.gameObject);
                _spawnedMainHandWeapon = null;
            }

            // _attackType = EWeaponAttackType.None;
            ulong mainHandWeapon = entityDef.MainHandWeapon;
            if (CtDataBlock.IsValid(mainHandWeapon))
            {
                ushort weaponId = CtDataBlock.GetWeaponIdentifier(mainHandWeapon);
                CtWeaponDef weaponDef = gameData.GetWeaponDef(weaponId);
                if (weaponDef)
                {
                    var userData = weaponDef.UserData;
                    if (userData)
                    {
                        playerWeapon.InteractionText = weaponDef.DisplayName;
                        var interactCollider = playerWeapon.GetComponent<BoxCollider>();

                        _spawnedMainHandWeapon = Instantiate(userData.gameObject, playerWeapon.transform)
                            .GetComponent<CtWeaponAttack>();

                        var tempMeshFilter = _spawnedMainHandWeapon.GetComponent<MeshFilter>();
                        var tempMeshRenderer = _spawnedMainHandWeapon.GetComponent<MeshRenderer>();
                        playerWeapon.GetComponent<MeshFilter>().sharedMesh = tempMeshFilter.sharedMesh;
                        var material = tempMeshRenderer.sharedMaterial;
                        var meshRenderer = playerWeapon.GetComponent<MeshRenderer>();
                        meshRenderer.material = material;

                        _spawnedMainHandWeapon.gameObject.SetActive(true);
                        _spawnedMainHandWeapon.PlayerTurn = playerTurn;

                        var colliders = _spawnedMainHandWeapon.GetComponents<BoxCollider>();
                        interactCollider.center = colliders[1].center;
                        interactCollider.size = colliders[1].size;

                        Destroy(tempMeshFilter);
                        Destroy(tempMeshRenderer);
                        Destroy(colliders[1]);

                        const float size = 4f;
                        const float uvRange = 1.0f / 4f;

                        float palette = _spawnedMainHandWeapon.Palette;
                        var textureVector = new Vector4(
                            uvRange,
                            uvRange,
                            uvRange * (palette % size),
                            uvRange * Mathf.Floor(palette / size));
                        MaterialPropertyBlock props = new MaterialPropertyBlock();
                        props.SetVector("_MainTex_ST", textureVector);
                        meshRenderer.SetPropertyBlock(props);

                        _spawnedMainHandWeapon.transform.localPosition = Vector3.zero;
                        _spawnedMainHandWeapon.transform.localRotation = Quaternion.identity;
                        _spawnedMainHandWeapon.transform.localScale = Vector3.one;

                        _RespawnWeapon();
                    }
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

        public override void OnPickup()
        {
#if DEBUG_LOGS
            LogDebug($"Pickup weapon (currentHand={playerWeapon.currentHand}).");
#endif

            SetArgs.Add(Convert.ToInt32(playerWeapon.currentHand));
            this.Emit(EItemSpawnerSignal.PickupChange);
        }

        public override void OnDrop()
        {
#if DEBUG_LOGS
            LogDebug($"Drop weapon (currentHand={playerWeapon.currentHand}).");
#endif

            SetArgs.Add(Convert.ToInt32(VRC_Pickup.PickupHand.None));
            this.Emit(EItemSpawnerSignal.PickupChange);
        }
    }
}