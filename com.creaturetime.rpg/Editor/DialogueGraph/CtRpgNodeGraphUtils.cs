
using System;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    public static class CtRpgNodeGraphUtils
    {
        public static CtQuestDef FindQuest(ushort actorId)
        {
            var questDefs = GameObject.FindObjectsOfType<CtQuestDef>(true);
            foreach (var questDef in questDefs)
            {
                if (questDef.Identifier == actorId)
                    return questDef;
            }
        
            return null;
        }

        public static CtNpcDef FindNpc(ushort actorId)
        {
            var npcDefs = GameObject.FindObjectsOfType<CtNpcDef>(true);
            foreach (var npcDef in npcDefs)
            {
                if (npcDef.Identifier == actorId)
                    return npcDef;
            }
        
            return null;
        }
    }
}
