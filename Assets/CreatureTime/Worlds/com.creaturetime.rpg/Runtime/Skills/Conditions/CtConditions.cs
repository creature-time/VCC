
using UdonSharp;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtConditions : CtSingleton
    {
        public CtSkillDef bleeding;
        public CtSkillDef poison;
        public CtSkillDef disease;
        public CtSkillDef burning;
        public CtSkillDef dazed;
        public CtSkillDef blind;
        public CtSkillDef exhausted;

        public CtSkillDef Bleeding => bleeding;
        public CtSkillDef Poison => poison;
        public CtSkillDef Disease => disease;
        public CtSkillDef Burning => burning;
        public CtSkillDef Dazed => dazed;
        public CtSkillDef Blind => blind;
        public CtSkillDef Exhausted => exhausted;
    }
}