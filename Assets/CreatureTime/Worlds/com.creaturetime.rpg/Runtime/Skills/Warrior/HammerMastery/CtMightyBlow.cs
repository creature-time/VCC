
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime.RpgGame.Skills
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtMightyBlow : CtSkillDef
    {
        [Header("Damage Stats")]
        [SerializeField] private int damageBase = 10;
        [SerializeField] private float damagePerAttribute = 2;

        public override string GetDescription(int attributeRank)
        {
            int damage = CtRpgFormulas.CalcSkillValue(damageBase, damagePerAttribute, attributeRank);
            return $"Attack does +<color={ValueColor}>{damage}</color> melee damage.";
        }

        public override void OnUse(CtEntity source, CtEntity target, DataList adjacentTargets)
        {
            // Skill Weapon Damage
            MeleeSkill(target, source, damageBase, damagePerAttribute, AttributeType);
        }
    }
}