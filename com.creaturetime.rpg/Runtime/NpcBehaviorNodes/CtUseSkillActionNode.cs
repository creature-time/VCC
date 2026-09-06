
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
#if DEBUG_LOGS
            LogDebug("CtUseSkillActionNode");
#endif

            // CtNpcBehaviorUtils.AssertIfTargetIsValid(target);
            // CtNpcBehaviorUtils.AssertIfSkillIsValid(skill);

            context.TryGetFloat("SkillFocus/BuffingWeight",  out var buffingWeight);
            context.TryGetFloat("SkillFocus/DeBuffingWeight",  out var deBuffingWeight);
            context.TryGetFloat("SkillFocus/ConditionsWeight",  out var conditionsWeight);
            context.TryGetFloat("SkillFocus/DamageWeight",  out var damageWeight);

            var skillWeights = new float[10];
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

#if DEBUG_LOGS
                LogDebug($"Skill weight for {i} was {skillWeights[i]}.");
#endif
            }

            var skillIndex = CtRandomizer.GetRandomFromArray(skillWeights);
#if DEBUG_LOGS
            LogDebug($"Chosen skill index {skillIndex}.");
#endif
            context.SetInt("Result/SkillIndex", skillIndex);

            if (skillIndex != -1)
            {
                if (!context.TryGetUShort($"Skills.Values[{skillIndex}]/Identifier", out var skillId))
                {
                    return ENodeStatus.Failure;
                }

                context.SetUShort("Result/SkillId", skillId);
            }

            return chooseTargetNode.Process(context);
        }
    }
}