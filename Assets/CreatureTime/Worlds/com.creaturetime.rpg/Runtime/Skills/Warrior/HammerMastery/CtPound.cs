
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime.RpgGame.Skills
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtPound : CtSkillDef
    {
        [SerializeField, HideInInspector] private CtConditions conditions;

        [Header("Damage Stats")]
        [SerializeField] private int damageBase = 1;
        [SerializeField] private float damagePerAttribute = 1.0f + PreCalcOneThirds;

        [Header("Dazed Turns")]
        [SerializeField] private int dazedBase = 1;
        [SerializeField] private float dazedPerAttribute = 0.125f;

        public override string GetDescription(int attributeRank)
        {
            int damage = CtRpgFormulas.CalcSkillValue(damageBase, damagePerAttribute, attributeRank);
            int dazed = CtRpgFormulas.CalcSkillValue(dazedBase, dazedPerAttribute, attributeRank);
            return $"Attack does +<color={ValueColor}>{damage}</color> melee damage and " +
                   $"daze for <color={ValueColor}>{dazed}</color> turns.";
        }

        public override void OnUse(CtEntity source, CtEntity target, DataList adjacentTargets)
        {
            // Skill Weapon Damage
            MeleeSkill(source, target, damageBase, damagePerAttribute, AttributeType);

            // Apply Dazed
            int attributeRank = source.TryGetAttributeLevelByAttributeType(AttributeType);
            int turns = CtRpgFormulas.CalcSkillValue(dazedBase, dazedPerAttribute, attributeRank);
            target.ApplyStatus(conditions.Dazed, source, turns);
        }
    }
}