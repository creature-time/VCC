
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

        protected override void _OnEntityIdChanged()
        {
            if (EntityId != CtConstants.InvalidId)
            {
                var npcIdentifier = CtEntityManager.GetIdentifier(EntityId);
                var npcDef = gameData.GetNpcDef(npcIdentifier);
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