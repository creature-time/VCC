
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

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

        public override string GetDescription(int attributeRank)
        {
            int healing = CtRpgFormulas.CalcSkillValue(healingBase, healingPerAttribute, attributeRank);
            return $"You gain <color={ValueColor}>{healing}</color> health. " +
                   $"You have -{armorReduction} for 1 turn after using the skill.";
        }

        public override void OnUse(CtEntity source, CtEntity target, DataList adjacentTargets)
        {
            // Apply Healing
            int attributeRank = source.TryGetAttributeLevelByAttributeType(AttributeType);
            int healing = CtRpgFormulas.CalcSkillValue(healingBase, healingPerAttribute, attributeRank);
            // target.ApplyHeal(instanceId, healing, Identifier, source);

            // Skill Weapon Damage
            target.ApplyStatus(this, source, 1);
        }

        public override void OnPersistentEffect(CtEntity target, CtEntity source)
        {
            target.ArmorRatingReduction = armorReduction;
        }
    }
}
