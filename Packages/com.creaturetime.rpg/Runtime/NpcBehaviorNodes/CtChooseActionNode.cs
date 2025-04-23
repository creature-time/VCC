
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtChooseActionNode : CtBehaviorTreeNodeBase
    {
        [SerializeField] private CtDefensiveActionNode defensiveChoiceNode;
        [SerializeField] private CtOffensiveActionNode offensiveChoiceNode;

        private float[] _weights = { 0f, 0f };

        public override ENodeStatus Process(CtNpcContext context)
        {
            // CtNpcBehaviorUtils.AssertIfTargetIsValid(target);
            // CtNpcBehaviorUtils.AssertIfSkillIsValid(skill);

            context.TryGetFloat("States/SelfHealingThreshold", out var selfHealingThreshold);
            context.TryGetFloat("Self/Health", out var npcHealth);
            context.TryGetFloat("Defensive/HealingWeight", out var healingWeight);

            context.TryGetFloat("Self/HealingCoolDown", out var selfHealingCoolDown);
            context.TryGetFloat("Defensive/HealingCoolDownWeight", out var healingCoolDownWeight);

            context.TryGetFloat("Offensive/OffensiveWeight", out var offensiveWeight);

            // Determine if we should heal based on npc health and last time they healed.
            float weightHp = (selfHealingThreshold - npcHealth) * healingWeight;
            float healingCoolDown =
                Mathf.Min(1.0f, selfHealingCoolDown * healingCoolDownWeight);
            _weights[0] = Mathf.Max(0.0f, weightHp + healingCoolDown);

            _weights[1] = offensiveWeight;

            var index = CtRandomizer.GetRandomFromArray(_weights);
            if (index == 0)
                return defensiveChoiceNode.Process(context);
            return offensiveChoiceNode.Process(context);
        }
    }
}