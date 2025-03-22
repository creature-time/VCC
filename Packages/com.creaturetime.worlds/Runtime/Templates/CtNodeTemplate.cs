
using UdonSharp;

namespace CreatureTime
{
    public enum ENodeStatus
    {
        Success,
        Failure,
        Running
    }

    public abstract class CtNodeTemplate<TContext> : CtLoggerUdonScript
        where TContext : UdonSharpBehaviour
    {
        public virtual bool IsComplete(TContext context) => true;
        public virtual void OnEnter(TContext context) {}
        public virtual ENodeStatus Process(TContext context) => ENodeStatus.Success;
        public virtual void OnExit(TContext context) {}
    }
}