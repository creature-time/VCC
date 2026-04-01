using UdonSharp;

namespace CreatureTime.RpgGame.Conditions
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBlind : CtSkillDef
    {
        public override string GetDescription(int attributeRank)
        {
            return "You have a 50% chance to miss your melee attacks.";
        }

        public override void OnPersistentEffect(CtEntity target, CtEntity source)
        {
            target.IsBlind = true;
        }
    }
}