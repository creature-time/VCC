
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame.Skills
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtHealingSpring : CtSkillDef
    {
        [Header("Healing Stats")]
        [SerializeField] private int healingBase = 40;
        [SerializeField] private float healingPerAttribute = 2;

        public override bool IsBeneficial => true;
        public override ESkillType Type => ESkillType.Energy;
        public override int Cost => 5;
        public override int RechargeTime => 5;
        public override ETargetType TargetType => ETargetType.SelfOnly;

        public override string GetDescription(int attributeRank)
        {
            int healing = CalcHeal(healingBase, healingPerAttribute, attributeRank);
            return $"Heals for <color={ValueColor}>{healing}</color>.";
        }

        public override void OnUse(CtGameData gameData, CtEntity target, CtEntity source)
        {
            // Apply Healing
            int attributeRank = TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, AttributeType);
            int healing = CalcHeal(healingBase, healingPerAttribute, attributeRank);
            // target.ApplyHeal(instanceId, healing, Identifier, source);
        }
    }
}