
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNpcEntity : CtEntity
    {
        [SerializeField] private CtBattleNpcBrain brain;
        [SerializeField] private CtPlayerTurn npcTurn;

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

        public override ushort EntityId => EntityIdCallback;
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

        public override bool TryGetAttack(out int skillIndex, out ushort targetId)
        {
            brain.Sense();
            brain.Think();

            return npcTurn.TryGetAttack(out skillIndex, out targetId);
        }
    }
}