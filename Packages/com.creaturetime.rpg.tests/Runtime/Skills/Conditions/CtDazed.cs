using UdonSharp;

namespace CreatureTime.RpgGame.Conditions
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtDazed : CtSkillDef
    {
        public override string GetDescription(int attributeRank)
        {
            return "You have 90% change of being stunned due to being dazed.";
        }

        public override void OnPersistentEffect(CtEntity target, CtEntity source)
        {
            target.IsDazed = true;
            target.ApplyDamage(-1, EDamageType.Dazed, EDamageSourceType.Condition, Identifier, source, false);
        }
    }
}