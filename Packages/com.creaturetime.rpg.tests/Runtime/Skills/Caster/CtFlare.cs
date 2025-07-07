
using UdonSharp;
using UnityEngine;

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

        public override ESkillType Type => ESkillType.Energy;
        public override int Cost => 5;
        public override int RechargeTime => 5;
        public override ETargetType TargetType => ETargetType.EnemyOnly;

        public override string GetDescription(int attributeRank)
        {
            int damage = CalcSkillValue(damageBase, damagePerAttribute, attributeRank);
            int burning = CalcSkillValue(burningBase, burningPerAttribute, attributeRank);
            return
                $"Damage your target for <color={ValueColor}>{damage}</color> fire damage. " +
                $"Apply burning for <color={ValueColor}>{burning}</color> turns.";
        }

        public override void OnUse(CtGameData gameData, CtEntity target, CtEntity source)
        {
            // Skill Damage
            SpellSkill(gameData, target, source, AttributeType, Identifier, EDamageType.Fire, damageBase, damagePerAttribute);

            // Apply Burning
            int attributeRank = TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, AttributeType);
            int turns = CalcSkillValue(burningBase, burningPerAttribute, attributeRank);
            ApplyStatus(target, source, conditions.Burning.Identifier, turns);
        }
    }
}