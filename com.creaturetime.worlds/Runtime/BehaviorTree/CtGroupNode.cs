
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtGroupNode : CtBehaviorTreeNode
    {
        [SerializeField] protected CtBehaviorTreeNodeBase[] children;

        protected int _index;
    }
}