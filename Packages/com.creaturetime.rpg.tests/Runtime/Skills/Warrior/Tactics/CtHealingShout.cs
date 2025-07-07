
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame.Skills
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtHealingShout : CtSkillDef
    {
        [Header("Healing Stats")]
        [SerializeField] private int healingBase = 82;
        [SerializeField] private float healingPerAttribute = 6;

        [Header("Persistent Stats")]
        [SerializeField] private int armorReduction = 40;

        public override bool IsBeneficial => true;
        public override ESkillType Type => ESkillType.Adrenaline;
        public override int Cost => 5;
        public override int RechargeTime => 5;
        public override ETargetType TargetType => ETargetType.SelfOnly;

        public override string GetDescription(int attributeRank)
        {
            int healing = CalcHeal(healingBase, healingPerAttribute, attributeRank);
            return $"You gain <color={ValueColor}>{healing}</color> health. " +
                   $"You have -{armorReduction} for 1 turn after using the skill.";
        }

        public override void OnUse(CtGameData gameData, CtEntity target, CtEntity source)
        {
            // Apply Healing
            int attributeRank = TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, AttributeType);
            int healing = CalcHeal(healingBase, healingPerAttribute, attributeRank);
            // target.ApplyHeal(instanceId, healing, Identifier, source);

            // Skill Weapon Damage
            ApplyStatus(target, source, Identifier, 1);
        }

        public override void OnPersistentEffect(CtEntity target, CtEntity source)
        {
            if (target.ArmorRatingReduction < armorReduction)
                target.ArmorRatingReduction = armorReduction;
        }
    }
}
