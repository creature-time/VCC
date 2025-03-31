
using UdonSharp;

namespace CreatureTime
{
    public abstract class CtResponseConsequence : UdonSharpBehaviour
    {
        public virtual void Execute(CtBlackboard blackboard) { }
    }
}