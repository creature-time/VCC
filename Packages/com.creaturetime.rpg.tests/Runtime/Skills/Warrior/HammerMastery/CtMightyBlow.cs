
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame.Skills
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtMightyBlow : CtSkillDef
    {
        [Header("Damage Stats")]
        [SerializeField] private int damageBase = 10;
        [SerializeField] private float damagePerAttribute = 2;

        public override ESkillType Type => ESkillType.Adrenaline;
        public override int Cost => 5;
        public override int RechargeTime => 5;
        public override ETargetType TargetType => ETargetType.EnemyOnly;

        public override string GetDescription(int attributeRank)
        {
            int damage = CalcSkillValue(damageBase, damagePerAttribute, attributeRank);
            return $"Attack does +<color={ValueColor}>{damage}</color> melee damage.";
        }

        public override void OnUse(CtGameData gameData, CtEntity target, CtEntity source)
        {
            // Skill Weapon Damage
            MeleeSkill(gameData, target, source, Identifier, damageBase, damagePerAttribute);
        }
    }
}