
using UdonSharp;

namespace CreatureTime
{
    public abstract class CtNpcBrain : UdonSharpBehaviour
    {
        // Translate game state to world state.
        public abstract void Sense();
        public abstract void Think();
    }
}