
namespace CreatureTime
{
    public abstract class CtSingleton : CtAbstractSignal
    {
        public virtual int Order => 0;

        public virtual void Init() { }
    }
}
