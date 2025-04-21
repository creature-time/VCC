
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleNpcController : CtAbstractSignal
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator animator;

        [Header("Characteristics")]
        [SerializeField] private CtNpcFeature[] features = {};

        [Header("Skeleton References")]
        [SerializeField] private Transform headBone;
        [SerializeField] private Transform eyeBoneL;
        [SerializeField] private Transform eyeBoneR;

        public Transform HeadBone => headBone;
        public Transform EyeBoneL => eyeBoneL;
        public Transform EyeBoneR => eyeBoneR;

        [SerializeField] private CtNpcEntity npcEntity;

        public void Sense()
        {
            npcGameState.AllyIdentifiers = new int[bsm.AllyTeam.Length];
            npcGameState.AllyTeamHealth = new float[bsm.AllyTeam.Length];
            npcGameState.EnemyIdentifiers = new int[bsm.EnemyTeam.Length];
            npcGameState.EnemyTeamHealth = new float[bsm.EnemyTeam.Length];

            npcGameState.NpcIndex = npcEntity.Identifier;
            npcGameState.NpcTeam = (source.Team == bsm.EnemyTeam) ? -1.0f : 1.0f;
            npcGameState.NpcHealth = NormalizedHealth;
            
            for (int i = 0; i < bsm.AllyTeam.Length; ++i)
            {
                npcGameState.AllyTeamHealth[i] = -1;
                CtEntity ally = bsm.AllyTeam[i];
                if (ally.State == ECombatState.None)
                    continue;
            
                npcGameState.AllyIdentifiers[i] = ally.Identifier;
                npcGameState.AllyTeamHealth[i] =
                    ally.Health / (float)ally.EntityDef.MaxHealth;
            }
            
            for (int i = 0; i < bsm.EnemyTeam.Length; ++i)
            {
                npcGameState.EnemyTeamHealth[i] = -1;
                CtEntity enemy = bsm.EnemyTeam[i];
                if (enemy.State == ECombatState.None)
                    continue;
            
                npcGameState.EnemyIdentifiers[i] = enemy.Identifier;
                npcGameState.EnemyTeamHealth[i] = enemy.Health / (float)enemy.EntityDef.MaxHealth;
            }
            
            for (int i = 0; i < 10; i++)
            {
                npcGameState.IsSelfTargetOnly[i] = 0;
                npcGameState.IsTargetEnemey[i] = 0;
                npcGameState.SkillRecharging[i] = 0;
            
                npcGameState.SupportSkillScores[i] = 0;
                npcGameState.HealingSkillScores[i] = 0;
                npcGameState.BuffingSkillScores[i] = 0;
                npcGameState.DeBuffingSkillScores[i] = 0;
                npcGameState.ConditionSkillScores[i] = 0;
                npcGameState.DamageSkillScores[i] = 0;
            
                ushort skillId = source.EntityStats.GetSkill(i);
                if (skillId == CtRpgRules.InvalidId)
                    continue;
            
                CtSkillDef skillDef = gameData.GetSkillDef(skillId);
                if (!skillDef)
                    continue;
            
                int recharge = source.GetRecharge(i);
                if (recharge > 0)
                    continue;
            
                npcGameState.SkillRecharging[i] = recharge;
            
                switch (skillDef.Type)
                {
                    case ESkillType.Energy:
                        if (source.Energy < skillDef.Value)
                            continue;
                        break;
                    case ESkillType.Adrenaline:
                        if (source.GetAdrenaline(i) < skillDef.Value)
                            continue;
                        break;
                    default:
                        CtLogger.LogCritical("Battle Sequencer",
                            $"Skill type not supported (skillType={skillDef.Type}).");
                        continue;
                }
            
                CtNpcBehaviorGameState.DistributeSkillScores(skillDef,
                    ref npcGameState.IsSelfTargetOnly[i],
                    ref npcGameState.IsTargetEnemey[i],
                    ref npcGameState.SupportSkillScores[i],
                    ref npcGameState.HealingSkillScores[i],
                    ref npcGameState.BuffingSkillScores[i],
                    ref npcGameState.DeBuffingSkillScores[i],
                    ref npcGameState.ConditionSkillScores[i],
                    ref npcGameState.DamageSkillScores[i]);
            }
        }

        public void Think()
        {
            brain.
            // npcBehaviorTree.Process(
            //     source.AsNpcStats().Behavior, source.NpcBehaviorData,
            //     ref targetId, ref skillIndex);
            //
            // ++source.NpcBehaviorData.HealingCoolDown;
            // ++source.NpcBehaviorData.OffensiveSkillCoolDown;

            if (targetId == -1)
            {
                CtLogger.LogError("Battle Sequencer",
                    "Failed to find best target " +
                    $"(entity={DisplayName} [{Identifier}], sourceId={Identifier}, targetId={targetId}).");
                return false;
            }

            LogDebug("Battle Sequencer",
                "Npc plays turn " +
                $"(entity={npcEntity.DisplayName} [{npcEntity.Identifier}], skillIndex={skillIndex}, targetId={targetId}).");

            if (skillIndex != -1)
            {
                ushort skillId = EntityStats.GetSkill(skillIndex);
                CtSkillDef skillDef = gameData.GetSkillDef(skillId);
                switch (skillDef.TargetType)
                {
                    case ETargetType.EnemyOnly:
                        npcController.OffensiveSkillCoolDown = 0;
                        break;
                    case ETargetType.AllEnemies:
                        npcController.OffensiveSkillCoolDown = 0;
                        break;
                    case ETargetType.AllyOnly:
                        npcController.HealingCoolDown = 0;
                        break;
                    case ETargetType.SelfOnly:
                        npcController.HealingCoolDown = 0;
                        break;
                    default:
                        CtLogger.LogCritical("Battle Sequencer",
                            $"Invalid target mask (targetMask={skillDef.TargetType}");
                        break;
                }
            }
        }

        public bool TryGetAttack(out int skillIndex, out int targetId)
        {
            skillIndex = brain;
            targetId = -1;

            return true;
        }

        private void Update()
        {
            // for (int i = 0; i < features.Length; i++)
            //     features[i].ExecuteUpdate(this);
        }

        private void LateUpdate()
        {
            // for (int i = 0; i < features.Length; i++)
            //     features[i].ExecuteLateUpdate(this);
        }
    }
}