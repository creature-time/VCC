
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNpcEntity : CtEntity
    {
        [SerializeField] private CtBattleNpcBrain brain;
        [SerializeField] private CtPlayerTurn npcTurn;

        private CtNpcController _npcController;

        public CtBattleNpcBrain Brain => brain;

        [UdonSynced, FieldChangeCallback(nameof(EntityIdCallback))]
        private ushort _entityId = CtConstants.InvalidId;

        public ushort EntityIdCallback
        {
            get => _entityId;
            set
            {
                var previousId = _entityId;
                _entityId = value;

                _OnEntityIdChanged();

                SetArgs.Add(previousId);
                SetArgs.Add(_entityId);
                this.Emit(EEntitySignal.IdentifierChanged);
            }
        }

        public CtNpcController NpcController
        {
            get => _npcController;
            set
            {
                if (_npcController)
                {
                    _npcController.Brain.Context.SetUShort("EntityId", CtConstants.InvalidId);
                    _npcController.Brain.Context.SetBool("Expert/IsDoneAttackingMelee", false);
                    _npcController.Brain.Context.SetBool("Expert/IsAttackingMelee", false);
                    _npcController.Brain.Context.SetBool("Expert/IsChargingMelee", false);
                }

                LogDebug($"NpcController was updated (entityId={Identifier}, prev={_npcController}, next={value}).");
                _npcController = value;
                if (_npcController)
                {
                    _npcController.Brain.Context.SetUShort("EntityId", Identifier);
                    _npcController.Brain.Context.SetBool("Expert/IsDoneAttackingMelee", false);
                    _npcController.Brain.Context.SetBool("Expert/IsAttackingMelee", false);
                    _npcController.Brain.Context.SetBool("Expert/IsChargingMelee", false);
                }
            }
        }

        public override ushort EntityId => EntityIdCallback;

        public override Vector3 Position => _npcController.transform.position;
        public override Quaternion Rotation => _npcController.transform.rotation;

        public override bool IsPlayer => false;

        public ushort NpcId
        {
            set => EntityIdCallback = value;
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