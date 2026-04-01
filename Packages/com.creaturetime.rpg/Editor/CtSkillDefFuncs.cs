
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreatureTime.RpgGame
{
    public static class CtSkillDefFuncs
    {
        public static void AssignSkillFlags(GameObject root = null)
        {
            Dictionary<ECombatEffectFlags, string> methodFlags = new Dictionary<ECombatEffectFlags, string>
            {
                { ECombatEffectFlags.Use, "OnUse" },
                { ECombatEffectFlags.PersistentEffect, "OnPersistentEffect" },
                { ECombatEffectFlags.SkillUsedEffect, "OnSkillUsed" },
                { ECombatEffectFlags.TickEffect, "OnTickEffect" },
                { ECombatEffectFlags.BlockEffect, "TryBlock"}
            };

            CtSkillDef[] skillDefinitions;
            skillDefinitions = root ? 
                root.GetComponentsInChildren<CtSkillDef>(true) : 
                Object.FindObjectsOfType<CtSkillDef>(true);

            foreach (CtSkillDef skillDefinition in skillDefinitions)
            {
                var serializedObject = new SerializedObject(skillDefinition);
                var flagsProp = serializedObject.FindProperty("flags");

                var flags = ECombatEffectFlags.None;

                foreach (KeyValuePair<ECombatEffectFlags, string> entry in methodFlags)
                {
                    var methodInfo = skillDefinition.GetType().GetMethod(entry.Value);
                    if (methodInfo == null)
                    {
                        throw new Exception($"Failed to find method (method={entry.Value}).");
                    }

                    if (methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType)
                    {
                        flags |= entry.Key;
                    }
                }

                flagsProp.enumValueFlag = Convert.ToInt32(flags);
                serializedObject.ApplyModifiedProperties();

                Debug.Log($"[EnterPlaymode] {skillDefinition.DisplayName} set flags to {skillDefinition.Flags}");
            }
        }
    }
}