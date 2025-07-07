
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame.Skills
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtFireHeal : CtSkillDef
    {
        [Header("Healing Stats")]
        [SerializeField] private int healingBase = 200;
        [SerializeField] private float healingPerAttribute = 20;

        public override bool IsBeneficial => true;
        public override ESkillType Type => ESkillType.Energy;
        public override int Cost => 5;
        public override int RechargeTime => 4;
        public override ETargetType TargetType => ETargetType.SelfOnly;

        public override string GetDescription(int attributeRank)
        {
            int healingPercent = CalcSkillValue(healingBase, healingPerAttribute, attributeRank);
            return 
                $"Heal for <color={ValueColor}>{healingPercent}</color>% " +
                "of the Energy cost each time you case a spell.";
        }

        public override void OnUse(CtGameData gameData, CtEntity target, CtEntity source)
        {
            // Apply Burning
            ApplyStatus(target, source, Identifier, 10);
        }

        public override void OnSkillUsed(CtGameData gameData, CtEntity target, CtEntity source,
            CtSkillDef usedSkill)
        {
            if (usedSkill.Type == ESkillType.Adrenaline)
                return;

            int attributeRank = TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, AttributeType);
            int skillValue = CalcSkillValue(healingBase, healingPerAttribute, attributeRank);
            int value = (int)(skillValue * (float)usedSkill.Value * 0.01f);
            // target.ApplyHeal(instanceId, value, Identifier, source);
        }
    }
}