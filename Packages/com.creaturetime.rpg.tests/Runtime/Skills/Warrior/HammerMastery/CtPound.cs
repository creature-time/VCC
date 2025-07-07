
using UdonSharp;
using UnityEngine;

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

        public override ESkillType Type => ESkillType.Adrenaline;
        public override int Cost => 5;
        public override int RechargeTime => 5;
        public override ETargetType TargetType => ETargetType.EnemyOnly;

        public override string GetDescription(int attributeRank)
        {
            int damage = CalcSkillValue(damageBase, damagePerAttribute, attributeRank);
            int dazed = CalcSkillValue(dazedBase, dazedPerAttribute, attributeRank);
            return $"Attack does +<color={ValueColor}>{damage}</color> melee damage and " +
                   $"daze for <color={ValueColor}>{dazed}</color> turns.";
        }

        public override void OnUse(CtGameData gameData, CtEntity target, CtEntity source)
        {
            // Skill Weapon Damage
            MeleeSkill(gameData, target, source, Identifier, damageBase, damagePerAttribute);

            // Apply Dazed
            int attributeRank = TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, AttributeType);
            int turns = CalcSkillValue(dazedBase, dazedPerAttribute, attributeRank);
            ApplyStatus(target, source, conditions.Dazed.Identifier, turns);
        }
    }
}