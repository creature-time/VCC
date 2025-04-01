
using UdonSharp;

namespace CreatureTime
{
    public abstract class CtResponseCondition : UdonSharpBehaviour
    {
        public virtual bool IsValid() { return false; }
    }
}