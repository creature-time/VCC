
using CreatureTime.Progression;
using UdonSharp;
using UnityEngine;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtLevelPrerequisite : CtAbstractPrerequisite
    {
        [SerializeField] private CtRpgGame rpgGame;

        [SerializeField] private int reqLevel;

        public override bool IsValid(CtPlayerProgressionDatabase playerProgressionDatabase)
        {
            return rpgGame.LocalEntity.Level >= reqLevel;
        }
    }
}