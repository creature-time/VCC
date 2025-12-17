
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBattleNpcBrain : CtNpcBrain
    {
        [SerializeField] private CtGameData gameData;
        [SerializeField] private CtNpcEntity entity;
        [SerializeField] private CtBehaviorTree behaviorTree;
        [SerializeField] private CtNpcContext npcContext;
        [SerializeField] private CtPlayerTurn npcTurn;

        public override CtBlackboard Context => npcContext;

        public CtNpcBehavior Behavior
        {
            set
            {
                npcContext.Clear();

                if (!value)
                    return;

                npcContext.SetFloat("States/SelfHealingThreshold", value.selfHealingThreshold);

                npcContext.SetFloat("Defensive/DefensiveWeight", value.defensiveWeight);
                npcContext.SetFloat("Defensive/SupportWeight", value.supportWeight);
                npcContext.SetFloat("Defensive/SupportCoolDownWeight", value.supportCoolDownWeight);
                npcContext.SetFloat("Defensive/HealingWeight", value.healingWeight);
                npcContext.SetFloat("Defensive/HealingCoolDownWeight", value.healingCoolDownWeight);

                npcContext.SetFloat("Offensive/OffensiveWeight", value.offensiveWeight);
                npcContext.SetFloat("Offensive/UseSkillWeight", value.useSkillWeight);
                npcContext.SetFloat("Offensive/UseSkillCoolDownWeight", value.useSkillCoolDownWeight);

                npcContext.SetFloat("SkillFocus/BuffingWeight", value.buffingWeight);
                npcContext.SetFloat("SkillFocus/DeBuffingWeight", value.deBuffingWeight);
                npcContext.SetFloat("SkillFocus/ConditionsWeight", value.conditionsWeight);
                npcContext.SetFloat("SkillFocus/DamageWeight", value.damageWeight);

                npcContext.SetFloat("Attacking/AttackWeight", value.attackWeight);
                npcContext.SetFloat("Attacking/AttackCoolDownWeight", value.attackCoolDownWeight);

                npcContext.SetFloat("Targeting/FocusTargetWeight", value.focusTargetWeight);
            }
        }

        public CtBattleState BattleState { get; set; }

        public override void Sense()
        {
            var allyParty = BattleState.AllyParty;
            var enemyParty = BattleState.EnemyParty;

            npcContext.SetInt("Result/SkillIndex", -1);

            npcContext.SetInt("Allies/Identifiers.Count", allyParty.MaxCount);
            npcContext.SetInt("Allies/Health.Count", allyParty.MaxCount);
            npcContext.SetInt("Enemies/Identifiers.Count", enemyParty.Count);
            npcContext.SetInt("Enemies/Health.Count", enemyParty.Count);

            npcContext.SetUShort("Self/Identifier", entity.Identifier);
            npcContext.SetFloat("Self/Party", allyParty.HasMember(entity) ? 1.0f : -1.0f);
            npcContext.SetFloat("Self/Health", entity.NormalizedHealth);

            npcContext.SetFloat("Self/AttackCoolDown", entity.AttackCoolDown);
            npcContext.SetFloat("Self/HealingCoolDown", entity.HealingCoolDown);

            npcContext.SetInt("Allies.Count", allyParty.MaxCount);
            for (int i = 0; i < allyParty.MaxCount; ++i)
            {
                npcContext.SetUShort($"Allies.Values[{i}]/Identifier", CtConstants.InvalidId);
                npcContext.SetFloat($"Allies.Values[{i}]/Health", -1);

                var ally = allyParty.GetEntity(i);
                if (!ally) continue;
                if (ally.State == ECombatState.None) continue;

                npcContext.SetUShort($"Allies.Values[{i}]/Identifier", ally.Identifier);
                npcContext.SetFloat($"Allies.Values[{i}]/Health", ally.NormalizedHealth);
            }

            npcContext.SetInt("Enemies.Count", enemyParty.MaxCount);
            for (int i = 0; i < enemyParty.MaxCount; ++i)
            {
                npcContext.SetUShort($"Enemies.Values[{i}]/Identifier", CtConstants.InvalidId);
                npcContext.SetFloat($"Enemies.Values[{i}]/Health", -1);

                var enemy = enemyParty.GetEntity(i);
                if (!enemy) continue;
                if (enemy.State == ECombatState.None) continue;

                npcContext.SetUShort($"Enemies.Values[{i}]/Identifier", enemy.Identifier);
                npcContext.SetFloat($"Enemies.Values[{i}]/Health", enemy.NormalizedHealth);
            }

            npcContext.SetInt("Skills.Count", CtEntityDef.MaxSkillCount);
            for (int i = 0; i < CtEntityDef.MaxSkillCount; i++)
            {
                npcContext.SetUShort($"Skills.Values[{i}]/Identifier", CtConstants.InvalidId);

                npcContext.SetBool($"Skills.Values[{i}]/IsSelfTargetOnly", false);
                npcContext.SetBool($"Skills.Values[{i}]/IsTargetEnemy", false);

                npcContext.SetFloat($"Skills.Values[{i}]/SkillRecharging", 0);
                npcContext.SetFloat($"Skills.Values[{i}]/SupportScore", 0);
                npcContext.SetFloat($"Skills.Values[{i}]/HealingScore", 0);
                npcContext.SetFloat($"Skills.Values[{i}]/BuffingScore", 0);
                npcContext.SetFloat($"Skills.Values[{i}]/DeBuffingScore", 0);
                npcContext.SetFloat($"Skills.Values[{i}]/ConditionScore", 0);
                npcContext.SetFloat($"Skills.Values[{i}]/DamageScore", 0);

                var skillDef = entity.GetSkillDef(i);
                if (!skillDef)
                    continue;

                float recharge = entity.SkillInstances.GetRecharge(i);
                if (recharge > 0)
                    continue;

                npcContext.SetUShort($"Skills.Values[{i}]/Identifier", skillDef.Identifier);
                npcContext.SetFloat($"Skills.Values[{i}]/SkillRecharging", recharge);

                switch (skillDef.Type)
                {
                    case ESkillType.Energy:
                        if (entity.Energy < skillDef.Value)
                            continue;
                        break;
                    case ESkillType.Adrenaline:
                        if (entity.SkillInstances.GetAdrenaline(i) < skillDef.Value)
                            continue;
                        break;
                    default:
#if DEBUG_LOGS
                        LogCritical($"Skill type not supported (skillType={skillDef.Type}).");
#endif
                        continue;
                }

                float isSelfTarget = 0;
                float isEnemyTarget = 0;
                float supportScore = 0;
                float healingScore = 0;
                float buffingScore = 0;
                float deBuffingScore = 0;
                float conditionScore = 0;
                float damageScore = 0;

                switch (skillDef.TargetType)
                {
                    case ETargetType.EnemyOnly:
                    case ETargetType.AllEnemies:
                        isEnemyTarget = 1.0f;
                        break;
                    case ETargetType.AllyOnly:
                        isEnemyTarget = -1.0f;
                        break;
                    case ETargetType.SelfOnly:
                        isSelfTarget = 1.0f;
                        isEnemyTarget = -1.0f;
                        break;
                }

                if (skillDef.IsBeneficial)
                {
                    healingScore += 1.0f;
                }
                else
                {
                    damageScore += 1.0f;
                }

                if (skillDef.IsBeneficial)
                {
                    buffingScore += 1.0f;
                }
                else
                {
                    deBuffingScore += 1.0f;
                }

#if DEBUG_LOGS
                LogDebug($"Skill {i} stats " +
                         $"(isSelfTarget={isSelfTarget}, isEnemyTarget={isEnemyTarget}, supportScore={supportScore}, " +
                         $"healingScore={healingScore}, buffingScore={buffingScore}, " +
                         $"deBuffingScore={deBuffingScore}, conditionScore={conditionScore}, " +
                         $"damageScore={damageScore}).");
#endif

                npcContext.SetFloat($"Skills.Values[{i}]/IsSelfTargetOnly", isSelfTarget);
                npcContext.SetFloat($"Skills.Values[{i}]/IsTargetEnemy", isEnemyTarget);

                npcContext.SetFloat($"Skills.Values[{i}]/SupportScore", supportScore);
                npcContext.SetFloat($"Skills.Values[{i}]/HealingScore", healingScore);
                npcContext.SetFloat($"Skills.Values[{i}]/BuffingScore", buffingScore);
                npcContext.SetFloat($"Skills.Values[{i}]/DeBuffingScore", deBuffingScore);
                npcContext.SetFloat($"Skills.Values[{i}]/ConditionScore", conditionScore);
                npcContext.SetFloat($"Skills.Values[{i}]/DamageScore", damageScore);
            }
        }

        public override void Think()
        {
            npcContext.SetUShort("Result/SkillId", CtConstants.InvalidId);
            npcContext.SetUShort("Result/TargetId", CtConstants.InvalidId);

            behaviorTree.Process();

            npcContext.TryGetUShort("Result/SkillId", out var skillId);
            npcContext.TryGetUShort("Result/TargetId", out var targetId);

            npcTurn.Submit(CTBattleInteractType.Attack, skillId, targetId);
        }
    }
}