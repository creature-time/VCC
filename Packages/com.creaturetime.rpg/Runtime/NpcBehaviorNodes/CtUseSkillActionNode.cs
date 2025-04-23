
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtUseSkillActionNode : CtBehaviorTreeNodeBase
    {
        [SerializeField] private CtChooseTargetNode chooseTargetNode;

        public override ENodeStatus Process(CtNpcContext context)
        {
            // CtNpcBehaviorUtils.AssertIfTargetIsValid(target);
            // CtNpcBehaviorUtils.AssertIfSkillIsValid(skill);

            context.TryGetFloat("SkillFocus/BuffingWeight",  out var buffingWeight);
            context.TryGetFloat("SkillFocus/DeBuffingWeight",  out var deBuffingWeight);
            context.TryGetFloat("SkillFocus/ConditionsWeight",  out var conditionsWeight);
            context.TryGetFloat("SkillFocus/DamageWeight",  out var damageWeight);

            float[] skillWeights = new float[10];
            for (int i = 0; i < 10; ++i)
            {
                skillWeights[i] = 0;

                context.TryGetFloat($"Skills.Values[{i}]/BuffingScore",  out var buffingScore);
                context.TryGetFloat($"Skills.Values[{i}]/DeBuffingScore",  out var deBuffingScore);
                context.TryGetFloat($"Skills.Values[{i}]/ConditionScore",  out var conditionsScore);
                context.TryGetFloat($"Skills.Values[{i}]/DamageScore",  out var damageScore);
                skillWeights[i] += buffingScore * buffingWeight;
                skillWeights[i] += deBuffingScore * deBuffingWeight;
                skillWeights[i] += conditionsScore * conditionsWeight;
                skillWeights[i] += damageScore * damageWeight;

                context.TryGetFloat($"Skills.Values[{i}]/SkillRecharging",  out var recharge);
                skillWeights[i] += Mathf.Clamp(recharge, 0.0f, 1.0f);
            }

            var skillIndex = CtRandomizer.GetRandomFromArray(skillWeights);
            context.SetInt("Results/SkillIndex", skillIndex);

            return chooseTargetNode.Process(context);
        }
    }
}