
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime.RpgGame.Skills
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtFireHeal : CtSkillDef
    {
        [SerializeField, HideInInspector] private CtConditions conditions;

        [Header("Healing Stats")]
        [SerializeField] private int healingBase = 200;
        [SerializeField] private float healingPerAttribute = 20;

        public override string GetDescription(int attributeRank)
        {
            int healingPercent = CtRpgFormulas.CalcSkillValue(healingBase, healingPerAttribute, attributeRank);
            return 
                $"Heal for <color={ValueColor}>{healingPercent}</color>% " +
                "of the Energy cost each time you case a spell.";
        }

        public override void OnUse(CtEntity source, CtEntity target, DataList adjacentTargets)
        {
            // Apply Burning
            target.ApplyStatus(conditions.Burning, source, 10);
        }

        // public override void OnSkillUsed(CtGameData gameData, CtEntity target, CtEntity source,
        //     CtSkillDef usedSkill)
        // {
        //     if (usedSkill.SkillType == ESkillType.Adrenaline)
        //         return;
        //
        //     int attributeRank = TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, AttributeType);
        //     int skillValue = CalcSkillValue(healingBase, healingPerAttribute, attributeRank);
        //     int value = (int)(skillValue * (float)usedSkill.Value * 0.01f);
        //     // target.ApplyHeal(instanceId, value, Identifier, source);
        // }
    }
}