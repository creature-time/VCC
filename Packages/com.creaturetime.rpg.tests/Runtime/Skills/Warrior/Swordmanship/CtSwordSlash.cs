using UdonSharp;
using UnityEngine;

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

        public override ESkillType Type => ESkillType.Adrenaline;
        public override int Cost => 5;
        public override int RechargeTime => 5;
        public override ETargetType TargetType => ETargetType.EnemyOnly;

        public override string GetDescription(int attributeRank)
        {
            int damage = CalcSkillValue(damageBase, damagePerAttribute, attributeRank);
            int bleeding = CalcSkillValue(bleedingBase, bleedingPerAttribute, attributeRank);
            return $"Attack does +<color={ValueColor}>{damage}</color> melee damage and " +
                   $"daze for <color={ValueColor}>{bleeding}</color> turns.";
        }

        public override void OnUse(CtGameData gameData, CtEntity target, CtEntity source)
        {
            // Skill Weapon Damage
            MeleeSkill(gameData, target, source, Identifier, damageBase, damagePerAttribute);

            // Apply Bleeding
            int attributeRank = TryGetAttributeLevelByAttributeType(gameData, source.EntityDef, AttributeType);
            int turns = CalcSkillValue(bleedingBase, bleedingPerAttribute, attributeRank);
            ApplyStatus(target, source, conditions.Bleeding.Identifier, turns);
        }
    }
}