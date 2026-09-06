
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerDef : CtEntityDef
    {
        [SerializeField] private CtPlayerPersistenceData playerPersistenceData;
        [SerializeField] private CtPlayerTurn playerTurn;
        [SerializeField] private CtPlayerProgressionDatabase primaryQuestProgression;
        [SerializeField] private CtPlayerProgressionDatabase secondaryQuestProgression;
        [SerializeField] private CtPlayerInventory playerInventory;
        [SerializeField] private CtPlayerWallet playerWallet;
        [SerializeField] private CtPlayerRoll playerRoll;

        public CtPlayerTurn PlayerTurn => playerTurn;
        public CtPlayerProgressionDatabase PrimaryQuestProgression => primaryQuestProgression;
        public CtPlayerProgressionDatabase SecondaryQuestProgression => secondaryQuestProgression;
        public CtPlayerInventory PlayerInventory => playerInventory;
        public CtPlayerWallet PlayerWallet => playerWallet;
        public CtPlayerRoll PlayerRoll => playerRoll;

        [UdonSynced, FieldChangeCallback(nameof(_Callback_LocationId))] private ushort locationId;

        public ushort _Callback_LocationId
        {
            get => locationId;
            set => locationId = value;
        }

        public ushort LocationId
        {
            get => _Callback_LocationId;
            set
            {
                _Callback_LocationId = value;
                RequestSerialization();
            }
        }

        public void Setup(Texture texture)
        {
            displayName = playerPersistenceData.DisplayName;
            icon = texture;
        }

        public void TearDown()
        {
            icon = null;
        }

        public void WeaponAttack(CtEntity target)
        {
            playerTurn.Submit(CTBattleInteractType.Attack, CtConstants.InvalidId, target.Identifier);
        }

        public void UseSkill(ushort skillId, CtEntity target)
        {
            playerTurn.Submit(CTBattleInteractType.Attack, skillId, target.Identifier);
        }

        public void UseSkill(ushort skillId)
        {
            playerTurn.Submit(CTBattleInteractType.Attack, skillId, CtConstants.InvalidId);
        }

        public void Leave()
        {
            playerTurn.Submit(CTBattleInteractType.Leave, CtConstants.InvalidId, CtConstants.InvalidId);
        }
    }
}