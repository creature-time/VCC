
using UdonSharp;

namespace CreatureTime.RpgGame.Conditions
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtExhausted : CtSkillDef
    {
        public override string GetDescription(int attributeRank)
        {
            return "Exhausted and shows signs of death.";
        }
    }
}