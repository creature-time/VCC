
namespace CreatureTime
{
    public abstract class CtNpcFeature : CtLoggerUdonScript
    {
        public virtual void Init(CtNpcController controller) {}
        public virtual void ExecuteUpdate(CtNpcController controller) {}
        public virtual void ExecuteLateUpdate(CtNpcController controller) {}
    }
}