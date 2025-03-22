
using UdonSharp;

namespace CreatureTime
{
    public abstract class CtNodeTemplate<TContext> : UdonSharpBehaviour
        where TContext : UdonSharpBehaviour
    {
        public virtual bool IsComplete(TContext context) => true;
        public virtual void OnEnter(TContext context) {}
        public virtual void Execute(TContext context) {}
        public virtual void OnExit(TContext context) {}
    }
}