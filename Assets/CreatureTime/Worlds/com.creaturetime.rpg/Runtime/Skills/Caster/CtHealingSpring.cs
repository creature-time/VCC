
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime.RpgGame.Skills
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtHealingSpring : CtSkillDef
    {
        [Header("Healing Stats")]
        [SerializeField] private int healingBase = 40;
        [SerializeField] private float healingPerAttribute = 2;

        public override string GetDescription(int attributeRank)
        {
            int healing = CtRpgFormulas.CalcSkillValue(healingBase, healingPerAttribute, attributeRank);
            return $"Heals for <color={ValueColor}>{healing}</color>.";
        }

        public override void OnUse(CtEntity source, CtEntity target, DataList adjacentTargets)
        {
            // Apply Healing
            int attributeRank = target.TryGetAttributeLevelByAttributeType(AttributeType);
            int healing = CtRpgFormulas.CalcSkillValue(healingBase, healingPerAttribute, attributeRank);
            // target.ApplyHeal(instanceId, healing, Identifier, source);
        }
    }
}