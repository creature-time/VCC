
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [DefaultExecutionOrder(-1)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtGameData : UdonSharpBehaviour
    {
        private DataDictionary _npcDefinitions = new DataDictionary();
        private DataDictionary _skillDefinitions = new DataDictionary();
        private DataDictionary _weaponDefinitions = new DataDictionary();
        private DataDictionary _offHandDefinitions = new DataDictionary();
        private DataDictionary _armorDefinitions = new DataDictionary();
        private DataDictionary _professionDefinitions = new DataDictionary();

        private void OnEnable()
        {
            CtNpcDef[] npcDefs = GetComponentsInChildren<CtNpcDef>(true);
            CtSkillDef[] skillDefs = GetComponentsInChildren<CtSkillDef>(true);
            CtWeaponDef[] weaponDefs = GetComponentsInChildren<CtWeaponDef>(true);
            CtOffHandDef[] offHandDefs = GetComponentsInChildren<CtOffHandDef>(true);
            CtArmorDef[] armorDefs = GetComponentsInChildren<CtArmorDef>(true);
            CtProfessionDef[] professionDefs = GetComponentsInChildren<CtProfessionDef>(true);

            for (int i = 0; i < npcDefs.Length; i++)
            {
                var npcDef = npcDefs[i];
                _npcDefinitions[npcDef.Identifier] = npcDef;
            }

            for (int i = 0; i < skillDefs.Length; i++)
            {
                var skillDef = skillDefs[i];
                _skillDefinitions[skillDef.Identifier] = skillDef;
            }

            for (int i = 0; i < weaponDefs.Length; i++)
            {
                var weaponDef = weaponDefs[i];
                _weaponDefinitions[weaponDef.Identifier] = weaponDef;
            }

            for (int i = 0; i < offHandDefs.Length; i++)
            {
                var offHandDef = offHandDefs[i];
                _offHandDefinitions[offHandDef.Identifier] = offHandDef;
            }

            for (int i = 0; i < armorDefs.Length; i++)
            {
                var armorDef = armorDefs[i];
                _armorDefinitions[armorDef.Identifier] = armorDef;
            }

            for (int i = 0; i < professionDefs.Length; i++)
            {
                var professionDef = professionDefs[i];
                _professionDefinitions[professionDef.Identifier] = professionDef;
            }

            CtLogger.LogDebug("Rpg Data", "Data Initialized.");
        }

        public CtNpcDef GetNpcDef(ushort identifier)
        {
            if (!_npcDefinitions.TryGetValue(identifier, out var dataToken))
            {
                CtLogger.LogWarning("Rpg Data", $"Failed to find npc by identifier (identifier={identifier}).");
                return null;
            }
            return (CtNpcDef)dataToken.Reference;
        }

        public CtSkillDef GetSkillDef(ushort identifier)
        {
            if (!_skillDefinitions.TryGetValue(identifier, out var dataToken))
            {
                CtLogger.LogWarning("Rpg Data", $"Failed to find skill by identifier (identifier={identifier}).");
                return null;
            }
            return (CtSkillDef)dataToken.Reference;
        }

        public CtWeaponDef GetWeaponDef(ushort identifier)
        {
            if (!_weaponDefinitions.TryGetValue(identifier, out var dataToken))
            {
                CtLogger.LogWarning("Rpg Data", $"Failed to find main-hand weapon by identifier (identifier={identifier}).");
                return null;
            }
            return (CtWeaponDef)dataToken.Reference;
        }

        public CtOffHandDef GetOffHandDef(ushort identifier)
        {
            if (!_offHandDefinitions.TryGetValue(identifier, out var dataToken))
            {
                CtLogger.LogWarning("Rpg Data", $"Failed to find off-hand weapon by identifier (identifier={identifier}).");
                return null;
            }
            return (CtOffHandDef)dataToken.Reference;
        }

        public CtArmorDef GetArmorDef(ushort identifier)
        {
            if (!_armorDefinitions.TryGetValue(identifier, out var dataToken))
            {
                CtLogger.LogWarning("Rpg Data", $"Failed to find armor by identifier (identifier={identifier}).");
                return null;
            }
            return (CtArmorDef)dataToken.Reference;
        }

        public CtProfessionDef GetProfessionDef(ushort identifier)
        {
            if (!_professionDefinitions.TryGetValue(identifier, out var dataToken))
            {
                CtLogger.LogWarning("Rpg Data", $"Failed to find profession by identifier (identifier={identifier}).");
                return null;
            }
            return (CtProfessionDef)dataToken.Reference;
        }
    }
}