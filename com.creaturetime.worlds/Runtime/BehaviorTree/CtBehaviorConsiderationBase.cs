using UdonSharp;

namespace CreatureTime
{
    public abstract class CtBehaviorConsiderationBase : UdonSharpBehaviour
    {
        public abstract float Evaluate(CtNpcContext context);
    }
}