
using CreatureTime.RpgGame;

namespace CreatureTime.Progression
{
    public abstract class CtAbstractPrerequisite : CtLoggerUdonScript
    {
        public abstract bool IsValid(CtPlayerProgressionDatabase playerProgressionDatabase);
    }
}