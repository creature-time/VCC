
namespace CreatureTime
{
    public abstract class CtNpcBrain : CtLoggerUdonScript
    {
        public abstract CtBlackboard Context { get; }

        // Translate game state to world state.
        public abstract void Sense();
        public abstract void Think();
    }
}