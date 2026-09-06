
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame.Dialogue
{
    public enum EMathExpression
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtCheckQuestObjective : CtResponseCondition
    {
        [SerializeField] private CtRpgGame rpgGame;
        [SerializeField] private CtQuestSystem questSystem;

        [SerializeField] private CtQuestDef quest;
        [SerializeField] private string flag;
        [SerializeField] private EMathExpression expression;
        [SerializeField] private int value;

        public override bool IsValid()
        {
            var localEntity = rpgGame.LocalEntity;
            var playerDatabase = quest.IsPrimaryQuest ? localEntity.PrimaryQuestProgression : localEntity.SecondaryQuestProgression;
            if (!questSystem.TryGetObjectiveValue(playerDatabase, flag, out var objectiveValue))
                return false;

            switch (expression)
            {
                case EMathExpression.Equal:
                    return objectiveValue == value;
                case EMathExpression.NotEqual:
                    return objectiveValue != value;
                case EMathExpression.GreaterThan:
                    return objectiveValue > value;
                case EMathExpression.LessThan:
                    return objectiveValue < value;
            }

            return false;
        }
    }
}