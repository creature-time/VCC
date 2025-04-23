
using System.Runtime.Remoting.Contexts;
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

            npcContext.SetInt("Allies/Identifiers.Count", allyParty.MaxCount);
            npcContext.SetInt("Allies/Health.Count", allyParty.MaxCount);
            npcContext.SetInt("Enemies/Identifiers.Count", allyParty.MaxCount);
            npcContext.SetInt("Enemies/Health.Count", allyParty.MaxCount);

            npcContext.SetUShort("Self/Identifier", entity.Identifier);
            npcContext.SetFloat("Self/Party", allyParty.HasMember(entity) ? 1.0f : -1.0f);
            npcContext.SetFloat("Self/Health", entity.NormalizedHealth);

            npcContext.SetFloat("Self/AttackCoolDown", entity.AttackCoolDown);
            npcContext.SetFloat("Self/HealingCoolDown", entity.HealingCoolDown);

            npcContext.SetInt("Allies.Count", allyParty.MaxCount);
            for (int i = 0; i < allyParty.MaxCount; ++i)
            {
                ushort identifier = allyParty.GetMemberId(i);
                if (identifier == CtConstants.InvalidId)
                    continue;
                if (!BattleState.TryGetEntity(identifier, out var entity))
                    continue;
                if (entity.State == ECombatState.None)
                    continue;

                npcContext.SetUShort($"Allies.Values[{i}]/Identifier", entity.Identifier);
                npcContext.SetFloat($"Allies.Values[{i}]/Health", entity.NormalizedHealth);
            }

            npcContext.SetInt("Enemies.Count", enemyParty.MaxCount);
            for (int i = 0; i < enemyParty.MaxCount; ++i)
            {
                ushort identifier = enemyParty.GetMemberId(i);
                if (identifier == CtConstants.InvalidId)
                    continue;
                if (!BattleState.TryGetEntity(identifier, out var entity))
                    continue;
                if (entity.State == ECombatState.None)
                    continue;

                npcContext.SetUShort($"Enemies.Values[{i}]/Identifier", entity.Identifier);
                npcContext.SetFloat($"Enemies.Values[{i}]/Health", entity.NormalizedHealth);
            }

            npcContext.SetInt("Skills.Count", CtEntityDef.MaxSkillCount);
            for (int i = 0; i < CtEntityDef.MaxSkillCount; i++)
            {
                npcContext.SetBool($"Skills.Values[{i}]/IsSelfTargetOnly", false);
                npcContext.SetBool($"Skills.Values[{i}]/IsTargetEnemy", false);

                npcContext.SetFloat($"Skills.Values[{i}]/SupportScore", 0);
                npcContext.SetFloat($"Skills.Values[{i}]/HealingScore", 0);
                npcContext.SetFloat($"Skills.Values[{i}]/BuffingScore", 0);
                npcContext.SetFloat($"Skills.Values[{i}]/DeBuffingScore", 0);
                npcContext.SetFloat($"Skills.Values[{i}]/ConditionScore", 0);
                npcContext.SetFloat($"Skills.Values[{i}]/DamageScore", 0);

                ushort skillId = entity.EntityDef.GetSkill(i);
                if (skillId == CtConstants.InvalidId)
                    continue;

                float recharge = entity.GetRecharge(i);
                if (recharge > 0)
                    continue;

                CtSkillDef skillDef = gameData.GetSkillDef(skillId);
                if (!skillDef)
                    continue;

                npcContext.SetFloat($"Skills.Values[{i}]/SkillRecharging", recharge);

                switch (skillDef.Type)
                {
                    case ESkillType.Energy:
                        if (entity.Energy < skillDef.Value)
                            continue;
                        break;
                    case ESkillType.Adrenaline:
                        if (entity.GetAdrenaline(i) < skillDef.Value)
                            continue;
                        break;
                    default:
                        CtLogger.LogCritical("Battle Sequencer",
                            $"Skill type not supported (skillType={skillDef.Type}).");
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

                if (skillDef)
                    return;

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
            npcContext.SetInt("Result/SkillIndex", -1);
            npcContext.SetUShort("Result/TargetId", CtConstants.InvalidId);

            behaviorTree.Process();

            npcContext.TryGetInt("Result/SkillIndex", out var skillIndex);
            npcContext.TryGetUShort("Result/TargetId", out var targetId);
            npcTurn.Submit(CTBattleInteractType.Attack, skillIndex, targetId);
        }
    }
}