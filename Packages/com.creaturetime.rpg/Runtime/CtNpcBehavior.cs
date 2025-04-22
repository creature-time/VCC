
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNpcBehavior : UdonSharpBehaviour
    {
        [Header("States")]
        public float selfHealingThreshold = 1.0f;

        [Header("Defensive")]
        public float defensiveWeight = 1.0f;

        public float supportWeight = 1.0f;
        public float supportCoolDownWeight = 1.0f;
        public float healingWeight = 1.0f;
        public float healingCoolDownWeight = 1.0f;

        [Header("Offensive")]
        public float offensiveWeight = 1.0f;

        public float useSkillWeight = 1.0f;
        public float useSkillCoolDownWeight = 1.0f;

        [Header("Skill Focus")]
        public float buffingWeight = 1.0f;
        public float deBuffingWeight = 1.0f;
        public float conditionsWeight = 1.0f;
        public float damageWeight = 1.0f;

        [Header("Attacking")]
        public float attackWeight = 1.0f;
        public float attackCoolDownWeight = 1.0f;

        [Header("Targeting")]
        public float focusTargetWeight = 0.0f;
    }
}