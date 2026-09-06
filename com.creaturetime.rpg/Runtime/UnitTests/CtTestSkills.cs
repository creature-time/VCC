
using CreatureTime.UnitTest;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime.RpgGame.UnitTest
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtTestSkills : CtUnitTest
    {
        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtBattleState battleState;
        [SerializeField] private CtBattleController controller;
        [SerializeField] private CtNpcEntity source;
        [SerializeField] private CtNpcEntity target;
        [SerializeField] private CtNpcEntity[] adjacentTargets;

        public override void Run()
        {
            source.NpcId = 1;
            source.BattleState = battleState;
            source.Controller = controller;

            target.NpcId = 1;
            target.BattleState = battleState;
            target.Controller = controller;

            var adjacentTargetsList = new DataList();
            for (int i = 0; i < adjacentTargets.Length; i++)
            {
                var adjacentTarget = adjacentTargets[i];
                adjacentTarget.NpcId = 1;
                adjacentTarget.BattleState = battleState;
                adjacentTarget.Controller = controller;
                adjacentTargetsList.Add(adjacentTarget);
            }

            foreach (var skillDef in gameData.SkillDefinitions)
            {
                Log($"Testing skill (skillDef={skillDef})...");

                source.OnStartBattle();
                target.OnStartBattle();
                for (int i = 0; i < adjacentTargets.Length; i++)
                    adjacentTargets[i].OnStartBattle();

                if (skillDef.HasUse)
                    skillDef.OnUse(source, target, adjacentTargetsList);
                if (skillDef.HasPersistentEffect)
                    skillDef.OnPersistentEffect(source, target);
                if (skillDef.HasSkillUsedEffect)
                    skillDef.OnSkillUsed(source, target, skillDef);
                if (skillDef.HasTickEffect)
                    skillDef.OnTickEffect(source, target);
                if (skillDef.HasBlockEffect)
                    skillDef.TryBlock(source, target, 42);

                source.OnEndBattle();
                target.OnEndBattle();
                for (int i = 0; i < adjacentTargets.Length; i++)
                    adjacentTargets[i].OnEndBattle();
            }

            source.NpcId = CtConstants.InvalidId;
            source.BattleState = null;
            source.Controller = null;

            target.NpcId = CtConstants.InvalidId;
            target.BattleState = null;
            target.Controller = null;

            for (int i = 0; i < adjacentTargets.Length; i++)
            {
                var adjacentTarget = adjacentTargets[i];
                adjacentTarget.NpcId = CtConstants.InvalidId;
                adjacentTarget.BattleState = null;
                adjacentTarget.Controller = null;
            }
        }
    }
}