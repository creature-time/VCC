
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime.RpgGame.Skills
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtPenetratingStrike : CtSkillDef
    {
        [Header("Damage Stats")]
        [SerializeField] private int damageBase = 5;
        [SerializeField] private float damagePerAttribute = 1;

        public override string GetDescription(int attributeRank)
        {
            int damage = CtRpgFormulas.CalcSkillValue(damageBase, damagePerAttribute, attributeRank);
            return $"Attack does +<color={ValueColor}>{damage}</color> melee damage with 20% armor penetration.";
        }

        public override void OnUse(CtEntity source, CtEntity target, DataList adjacentTargets)
        {
            // Skill Weapon Damage
            MeleeSkill(source, target, damageBase, damagePerAttribute, AttributeType);
        }
    }
}