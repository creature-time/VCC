
namespace CreatureTime
{
    public class CtSequenceNodeBase : CtNodeTemplate<CtBlackboard>
    {
        public virtual CtSequenceNodeBase GetNext(CtBlackboard context) => null;
    }
}