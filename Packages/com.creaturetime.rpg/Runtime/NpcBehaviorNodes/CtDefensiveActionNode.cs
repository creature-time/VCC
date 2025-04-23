
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtDefensiveActionNode : CtBehaviorTreeNodeBase
    {
        [SerializeField] private CtChooseTargetNode chooseTargetNode;
        [SerializeField] private CtOffensiveActionNode offensiveChoiceNode;

        public override ENodeStatus Process(CtNpcContext context)
        {
            // CtNpcBehaviorUtils.AssertIfTargetIsValid(target);
            // CtNpcBehaviorUtils.AssertIfSkillIsInvalid(skill);

            float[] skillWeights = new float[10];
            for (int i = 0; i < 10; ++i)
            {
                skillWeights[i] = 0;

                context.TryGetFloat("Defensive/SupportWeight",  out var supportScore);
                context.TryGetFloat($"Skills.Values[{i}]/SupportWeight",  out var supportWeight);
                context.TryGetFloat("Defensive/HealingScore",  out var healingScore);
                context.TryGetFloat($"Skills.Values[{i}]/HealingWeight",  out var healingWeight);

                skillWeights[i] += supportScore * supportWeight;
                skillWeights[i] += healingScore * healingWeight;

                context.TryGetFloat($"Skills.Values[{i}]/SkillRecharging", out var recharge);
                skillWeights[i] += Mathf.Clamp(recharge, 0.0f, 1.0f);
            }

            var skillIndex = CtRandomizer.GetRandomFromArray(skillWeights);
            if (skillIndex != -1)
            {
                context.SetInt("Result/SkillIndex", skillIndex);
                return chooseTargetNode.Process(context);
            }

            return offensiveChoiceNode.Process(context);
         }
    }
}