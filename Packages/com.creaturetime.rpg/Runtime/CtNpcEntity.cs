
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNpcEntity : CtEntity
    {
        [SerializeField] private CtBattleNpcBrain brain;
        [SerializeField] private CtPlayerTurn npcTurn;

        private CtBattleController _controller;

        public CtBattleNpcBrain Brain => brain;

        public override Transform RootTransform => _controller.transform;
        public override Transform HeadTransform => _controller.HeadBone;
        public override Transform LeftHandTransform => _controller.HandBoneL;
        public override Transform RightHandTransform => _controller.HandBoneR;

        [UdonSynced, FieldChangeCallback(nameof(EntityIdCallback))]
        private ushort _entityId = CtConstants.InvalidId;

        public ushort EntityIdCallback
        {
            get => _entityId;
            set
            {
#if DEBUG_LOGS
                LogDebug($"Npc entity definition identifier updated (prev={_entityId}, next={value}).");
#endif

                if (EntityDef)
                {
                    EntityDef.Disconnect(EEntityDefSignal.MainHandChanged, this, nameof(_OnMainHandChanged));
                }

                _entityId = value;
                _OnEntityIdChanged();

                if (EntityDef)
                {
                    _OnMainHandChanged();

                    EntityDef.Connect(EEntityDefSignal.MainHandChanged, this, nameof(_OnMainHandChanged));
                }
            }
        }

        public void _OnMainHandChanged()
        {
            if (!_controller) return;

            CtWeaponDef weaponDef = null;
            if (CtDataBlock.IsValid(EntityDef.MainHandWeapon))
            {
                var weaponid = CtDataBlock.GetWeaponIdentifier(EntityDef.MainHandWeapon);
                weaponDef = gameData.GetWeaponDef(weaponid);
            }

            _controller.SetWeaponDef(weaponDef);
        }

        public CtBattleController Controller
        {
            get => _controller;
            set
            {
                if (_controller)
                {
                    // _controller.Brain.Context.SetUShort("EntityId", CtConstants.InvalidId);
                    // _controller.Brain.Context.SetBool("Expert/IsDoneAttackingMelee", false);
                    // _controller.Brain.Context.SetBool("Expert/IsAttackingMelee", false);
                    // _controller.Brain.Context.SetBool("Expert/IsChargingMelee", false);
                }

                LogDebug($"NpcController was updated (entityId={Identifier}, prev={_controller}, next={value}).");
                _controller = value;
                if (_controller)
                {
                    _OnMainHandChanged();
                    // _controller.Brain.Context.SetUShort("EntityId", Identifier);
                    // _controller.Brain.Context.SetBool("Expert/IsDoneAttackingMelee", false);
                    // _controller.Brain.Context.SetBool("Expert/IsAttackingMelee", false);
                    // _controller.Brain.Context.SetBool("Expert/IsChargingMelee", false);
                }
            }
        }

        public override ushort EntityId => EntityIdCallback;

        public override bool IsPlayer => false;

        public ushort NpcId
        {
            set
            {
                EntityIdCallback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(HealingCoolDownCallback))]
        private int _healingCoolDown = 0;

        public int HealingCoolDownCallback
        {
            get => _healingCoolDown;
            set => _healingCoolDown = value;
        }

        public int HealingCoolDown
        {
            get => HealingCoolDownCallback;
            set
            {
                HealingCoolDownCallback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(OffensiveSkillCoolDownCallback))]
        private int _offensiveSkillCoolDown = 0;

        public int OffensiveSkillCoolDownCallback
        {
            get => _offensiveSkillCoolDown;
            set => _offensiveSkillCoolDown = value;
        }

        public int OffensiveSkillCoolDown
        {
            get => OffensiveSkillCoolDownCallback;
            set
            {
                OffensiveSkillCoolDownCallback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(AttackCoolDownCallback))]
        private int _attackCoolDown = 0;

        public int AttackCoolDownCallback
        {
            get => _attackCoolDown;
            set => _attackCoolDown = value;
        }

        public int AttackCoolDown
        {
            get => AttackCoolDownCallback;
            set
            {
                AttackCoolDownCallback = value;
                RequestSerialization();
            }
        }

        private void _OnEntityIdChanged()
        {
            if (EntityId != CtConstants.InvalidId)
            {
                var npcDef = gameData.GetNpcDef(EntityId);
                EntityDef = npcDef;
                brain.Behavior = npcDef.Behavior;
            }
            else
            {
                EntityDef = null;
                brain.Behavior = null;
            }
        }

        public override CtBattleState BattleState
        {
            set => brain.BattleState = value;
        }

        public override void OnStartBattle()
        {
            npcTurn.ResetToWait();
            base.OnStartBattle();
        }

        public override bool HasAttackReady()
        {
            return npcTurn.InteractType == CTBattleInteractType.Attack;
        }

        public override bool TryGetAttack(out ushort skillId, out ushort targetId)
        {
            if (!HasAttackReady())
            {
                brain.Sense();
                brain.Think();
            }

            return npcTurn.TryGetAttack(out skillId, out targetId);
        }

        protected override void OnDeath() => _controller.HandleDeath();

        public override void ResetAttack()
        {
            npcTurn.ResetToWait();
        }

        public override void OnEndBattle()
        {
            npcTurn.Reset();
            base.OnEndBattle();
        }
    }
}