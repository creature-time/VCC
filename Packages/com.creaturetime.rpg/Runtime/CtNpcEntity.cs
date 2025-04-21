
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNpcEntity : CtEntity
    {
        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtBattleNpcController npcController;

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
            var npcIdentifier = CtEntityManager.GetIdentifier(EntityId);
            EntityDef = gameData.GetNpcDef(npcIdentifier);
        }

        public override bool TryGetAttack()
        {
            return npcController.TryGetAttack(out _skillIndex, out _targetId);
        }
    }
}