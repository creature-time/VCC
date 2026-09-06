
namespace CreatureTime
{
    public class CtStateBase : CtNodeTemplate<CtBlackboard>
    {
        public virtual CtStateBase GetNext(CtBlackboard context) => null;
    }
}