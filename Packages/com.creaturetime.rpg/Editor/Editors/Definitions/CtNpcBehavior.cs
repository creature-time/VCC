
using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    [CreateAssetMenu(fileName = "npcBehaviorDefData", menuName = "CreatureTime/Rpg/Npc Behavior Definition", order = 1)]
    public class CtNpcBehaviorData : CtAbstractDefData
    {
        public override string GenerateName => 
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override ushort Identifier => identifier;

        [SerializeField] public ushort identifier = CtConstants.InvalidId;
        [SerializeField] public string displayName;

        [SerializeField] public float selfHealingThreshold = 1.0f;
        [SerializeField] public float defensiveWeight = 1.0f;
        [SerializeField] public float supportWeight = 1.0f;
        [SerializeField] public float supportCoolDownWeight = 1.0f;
        [SerializeField] public float healingWeight = 1.0f;
        [SerializeField] public float healingCoolDownWeight = 1.0f;
        [SerializeField] public float offensiveWeight = 1.0f;
        [SerializeField] public float useSkillWeight = 1.0f;
        [SerializeField] public float useSkillCoolDownWeight = 1.0f;
        [SerializeField] public float buffingWeight = 1.0f;
        [SerializeField] public float deBuffingWeight = 1.0f;
        [SerializeField] public float conditionsWeight = 1.0f;
        [SerializeField] public float damageWeight = 1.0f;
        [SerializeField] public float attackWeight = 1.0f;
        [SerializeField] public float attackCoolDownWeight = 1.0f;
        [SerializeField] public float focusTargetWeight = 0.0f;
    }
}