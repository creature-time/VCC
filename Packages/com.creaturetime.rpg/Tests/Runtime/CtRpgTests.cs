
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtRpgTests : CtLoggerUdonScript
    {
        [Header("State Machine Tests")]
        [SerializeField] private CtGameData gameData;

        public void _RunGameDataTest0()
        {
            for (ushort i = 1; i <= 3; ++i)
            {
                var npcDef = gameData.GetNpcDef(i);
                Log($"{npcDef.DisplayName} (identifier={npcDef.Identifier}, level={npcDef.CharacterLevel})");
            }
        }
    }
}