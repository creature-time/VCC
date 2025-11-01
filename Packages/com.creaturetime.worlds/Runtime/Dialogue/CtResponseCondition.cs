
namespace CreatureTime
{
    public abstract class CtResponseCondition : CtLoggerUdonScript
    {
        public virtual bool IsValid() { return false; }
    }
}