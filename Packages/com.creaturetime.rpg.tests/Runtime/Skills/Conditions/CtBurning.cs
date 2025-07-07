
using UdonSharp;

namespace CreatureTime.RpgGame.Conditions
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBurning : CtSkillDef
    {
        public override string GetDescription(int attributeRank)
        {
            return "You have -7 Health degeneration.";
        }

        public override void OnTickEffect(CtEntity target, CtEntity source)
        {
            target.ApplyDamage(7, EDamageType.Burning, EDamageSourceType.Condition, Identifier, source, false);
        }
    }
}