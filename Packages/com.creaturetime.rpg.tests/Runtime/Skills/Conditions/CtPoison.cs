using UdonSharp;

namespace CreatureTime.RpgGame.Conditions
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtPoison : CtSkillDef
    {
        public override string GetDescription(int attributeRank)
        {
            return "You have -4 Health degeneration.";
        }

        public override void OnTickEffect(CtEntity target, CtEntity source)
        {
            target.ApplyDamage(4, EDamageType.Poison, EDamageSourceType.Condition, Identifier, source, false);
        }
    }
}