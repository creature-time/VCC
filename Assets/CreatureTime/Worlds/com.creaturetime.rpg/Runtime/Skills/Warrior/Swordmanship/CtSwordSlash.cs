using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime.RpgGame.Skills
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSwordSlash : CtSkillDef
    {
        [SerializeField, HideInInspector] private CtConditions conditions;

        [Header("Damage Stats")]
        [SerializeField] private int damageBase = 1;
        [SerializeField] private float damagePerAttribute = 1.0f + PreCalcOneThirds;

        [Header("Bleeding Turns")]
        [SerializeField] private int bleedingBase = 1;
        [SerializeField] private float bleedingPerAttribute = PreCalcOneThirds;

        public override string GetDescription(int attributeRank)
        {
            int damage = CtRpgFormulas.CalcSkillValue(damageBase, damagePerAttribute, attributeRank);
            int bleeding = CtRpgFormulas.CalcSkillValue(bleedingBase, bleedingPerAttribute, attributeRank);
            return $"Attack does +<color={ValueColor}>{damage}</color> melee damage and " +
                   $"daze for <color={ValueColor}>{bleeding}</color> turns.";
        }

        public override void OnUse(CtEntity source, CtEntity target, DataList adjacentTargets)
        {
            // Skill Weapon Damage
            MeleeSkill(source, target, damageBase, damagePerAttribute, AttributeType);

            // Apply Bleeding
            int attributeRank = source.TryGetAttributeLevelByAttributeType(AttributeType);
            int turns = CtRpgFormulas.CalcSkillValue(bleedingBase, bleedingPerAttribute, attributeRank);
            target.ApplyStatus(conditions.Bleeding, source, turns);
        }
    }
}