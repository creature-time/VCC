
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtBehaviorTreeNode : CtBehaviorTreeNodeBase
    {
        [SerializeField] private CtBehaviorConsiderationBase consideration;

        public float CalculateScore(CtNpcContext context) => consideration.Evaluate(context);
    }
}