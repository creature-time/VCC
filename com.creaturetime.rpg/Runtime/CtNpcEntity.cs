
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

        protected override void OnMainHandChangedRaw()
        {
            base.OnMainHandChangedRaw();
            if (!_controller) return;
            _controller.SetWeaponDef(MainHand);
        }

        public CtBattleController Controller
        {
            get => _controller;
            set
            {
                if (_controller)
                {
                    this.Disconnect(EEntitySignal.Death, _controller, nameof(_controller.HandleDeath));

                    // _controller.Brain.Context.SetUShort("EntityId", CtConstants.InvalidId);
                    // _controller.Brain.Context.SetBool("Expert/IsDoneAttackingMelee", false);
                    // _controller.Brain.Context.SetBool("Expert/IsAttackingMelee", false);
                    // _controller.Brain.Context.SetBool("Expert/IsChargingMelee", false);
                }

#if DEBUG_LOGS
                LogDebug($"NpcController was updated (entityId={Identifier}, prev={_controller}, next={value}).");
#endif
                _controller = value;
                if (_controller)
                {
                    this.Connect(EEntitySignal.Death, _controller, nameof(_controller.HandleDeath));

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
            get => _entityId;
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
            get => brain.BattleState;
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

        public bool TryGenerateLoot(out ulong[] items)
        {
            var npcDef = (CtNpcDef)EntityDef;

            var rolls = 1;

            // var maxCurrency = Level * 10;

            if (npcDef.IsBoss)
            {
                rolls += 2;
                // maxCurrency *= 2;
            }

            // var t = Mathf.Pow(Random.value, 2);
            // currency = Mathf.FloorToInt(Mathf.Lerp(0, maxCurrency, t));

            if (npcDef.LootTable)
                CtLootTable.GetItemsFromLootTable(npcDef.LootTable, rolls, out items);
            else
                items = new ulong[] { };

            return true;
        }
    }
}