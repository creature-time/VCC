
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime.RpgGame.Skills
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtFlare : CtSkillDef
    {
        [SerializeField, HideInInspector] private CtConditions conditions;

        [Header("Damage Stats")]
        [SerializeField] private int damageBase = 20;
        [SerializeField] private float damagePerAttribute = 3;

        [Header("Burning Turns")]
        [SerializeField] private int burningBase = 1;
        [SerializeField] private float burningPerAttribute = 0.3333333f;

        public override string GetDescription(int attributeRank)
        {
            int damage = CtRpgFormulas.CalcSkillValue(damageBase, damagePerAttribute, attributeRank);
            int burning = CtRpgFormulas.CalcSkillValue(burningBase, burningPerAttribute, attributeRank);
            return
                $"Damage your target for <color={ValueColor}>{damage}</color> fire damage. " +
                $"Apply burning for <color={ValueColor}>{burning}</color> turns.";
        }

        public override void OnUse(CtEntity source, CtEntity target, DataList adjacentTargets)
        {
            // Skill Damage
            SpellSkill(source, target, EDamageType.Fire, damageBase, damagePerAttribute, AttributeType);

            // Apply Burning
            int attributeRank = source.TryGetAttributeLevelByAttributeType(AttributeType);
            int turns = CtRpgFormulas.CalcSkillValue(burningBase, burningPerAttribute, attributeRank);
            target.ApplyStatus(conditions.Burning, source, turns);
        }
    }
}