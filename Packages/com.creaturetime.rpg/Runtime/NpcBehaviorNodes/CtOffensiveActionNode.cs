
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtOffensiveActionNode : CtBehaviorTreeNodeBase
    {
        [SerializeField] private CtUseSkillActionNode useSkillActionNode;
        [SerializeField] private CtChooseTargetNode chooseTargetNode;

        private float[] _weights = { 0, 0 };

        public override ENodeStatus Process(CtNpcContext context)
        {
            context.TryGetFloat("Offensive/UseSkillWeight",  out var useSkillWeight);
            context.TryGetFloat("Self/AttackCoolDown",  out var attackCoolDown);
            context.TryGetFloat("Attacking/AttackWeight",  out var attackWeight);
            context.TryGetFloat("Attacking/AttackCoolDownWeight",  out var attackCoolDownWeight);

            // float lastUsedSkillWeight = 
            //     Mathf.Min(1.0f, behaviorData.OffensiveSkillCoolDown * behavior.useSkillCoolDownWeight);
            // useSkillWeight += lastUsedSkillWeight;
            useSkillWeight = Mathf.Max(0.0f, useSkillWeight);

            float lastAttackWeight = 
                Mathf.Min(1.0f, attackCoolDown * attackCoolDownWeight);
            attackWeight += lastAttackWeight;
            attackWeight = Mathf.Max(0.0f, attackWeight);

            _weights[0] = useSkillWeight;
            _weights[1] = attackWeight;
            var index = CtRandomizer.GetRandomFromArray(_weights);
            if (index != -1)
            {
                return useSkillActionNode.Process(context);
            }

            return chooseTargetNode.Process(context);
        }
    }
}