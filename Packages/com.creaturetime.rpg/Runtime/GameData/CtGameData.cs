
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtGameData : CtSingleton
    {
        [SerializeField] private CtNpcDef[] npcDefinitions;
        [SerializeField] private CtSkillDef[] skillDefinitions;
        [SerializeField] private CtWeaponDef[] weaponDefinitions;
        [SerializeField] private CtOffHandDef[] offHandDefinitions;
        [SerializeField] private CtArmorSetDef[] armorDefinitions;
        [SerializeField] private CtProfessionDef[] professionDefinitions;
        [SerializeField] private CtAttributeDef[] attributeDefinitions;
        [SerializeField] private CtAbstractQuest[] questDefinitions;
        [SerializeField] private CtSquadDef[] squadDefinitions;

        public CtNpcDef[] NpcDefinitions => npcDefinitions;
        public CtSkillDef[] SkillDefinitions => skillDefinitions;
        public CtWeaponDef[] WeaponDefinitions => weaponDefinitions;
        public CtOffHandDef[] OffHandDefinitions => offHandDefinitions;
        public CtArmorSetDef[] ArmorDefinitions => armorDefinitions;
        public CtProfessionDef[] ProfessionDefinitions => professionDefinitions;
        public CtAttributeDef[] AttributeDefinitions => attributeDefinitions;
        public CtAbstractQuest[] QuestDefinitions => questDefinitions;
        public CtSquadDef[] SquadDefinitions => squadDefinitions;

        private DataDictionary _npcDefinitions = new DataDictionary();
        private DataDictionary _skillDefinitions = new DataDictionary();
        private DataDictionary _weaponDefinitions = new DataDictionary();
        private DataDictionary _offHandDefinitions = new DataDictionary();
        private DataDictionary _armorDefinitions = new DataDictionary();
        private DataDictionary _professionDefinitions = new DataDictionary();
        private DataDictionary _attributeDefinitions = new DataDictionary();
        private DataDictionary _questDefinitions = new DataDictionary();
        private DataDictionary _squadDefinitions = new DataDictionary();

        public void Init()
        {
            foreach (var npcDef in npcDefinitions)
            {
                _npcDefinitions[npcDef.Identifier] = npcDef;
            }

            foreach (var skillDef in skillDefinitions)
            {
                _skillDefinitions[skillDef.Identifier] = skillDef;
            }

            foreach (var weaponDef in weaponDefinitions)
            {
                _weaponDefinitions[weaponDef.Identifier] = weaponDef;
            }

            foreach (var offHandDef in offHandDefinitions)
            {
                _offHandDefinitions[offHandDef.Identifier] = offHandDef;
            }

            foreach (var armorDef in armorDefinitions)
            {
                _armorDefinitions[armorDef.Identifier] = armorDef;
            }

            foreach (var professionDef in professionDefinitions)
            {
                _professionDefinitions[professionDef.Identifier] = professionDef;

                foreach (var attributeDef in professionDef.Attributes)
                    _attributeDefinitions[attributeDef.Identifier] = attributeDef;
            }

            foreach (var quest in questDefinitions)
            {
                _questDefinitions[quest.Identifier] = quest;
            }

            foreach (var squadDef in squadDefinitions)
            {
                _squadDefinitions[squadDef.Identifier] = squadDef;
            }

#if DEBUG_LOGS
            LogDebug("Data Initialized.");
#endif
        }

        public CtNpcDef GetNpcDef(ushort identifier)
        {
            if (!_npcDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find npc by identifier (identifier={identifier}).");
#endif
                return null;
            }
            return (CtNpcDef)dataToken.Reference;
        }

        public CtSkillDef GetSkillDef(ushort identifier)
        {
            if (!_skillDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find skill by identifier (identifier={identifier}).");
#endif
                return null;
            }
            return (CtSkillDef)dataToken.Reference;
        }

        public CtWeaponDef GetWeaponDef(ushort identifier)
        {
            if (!_weaponDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find main-hand weapon by identifier (identifier={identifier}).");
#endif
                return null;
            }
            return (CtWeaponDef)dataToken.Reference;
        }

        public CtOffHandDef GetOffHandDef(ushort identifier)
        {
            if (!_offHandDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find off-hand weapon by identifier (identifier={identifier}).");
#endif
                return null;
            }
            return (CtOffHandDef)dataToken.Reference;
        }

        public CtArmorSetDef GetArmorDef(ushort identifier)
        {
            if (!_armorDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find armor by identifier (identifier={identifier}).");
#endif
                return null;
            }
            return (CtArmorSetDef)dataToken.Reference;
        }

        public CtProfessionDef GetProfessionDef(ushort identifier)
        {
            if (!_professionDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find profession by identifier (identifier={identifier}).");
#endif
                return null;
            }
            return (CtProfessionDef)dataToken.Reference;
        }

        public CtAttributeDef GetAttributeDef(ushort identifier)
        {
            if (!_attributeDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find attribute by identifier (identifier={identifier}).");
#endif
                return null;
            }
            return (CtAttributeDef)dataToken.Reference;
        }

        public CtAbstractQuest GetQuestDef(ushort identifier)
        {
            if (!_questDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find quest by identifier (identifier={identifier}).");
#endif
                return null;
            }
            return (CtAbstractQuest)dataToken.Reference;
        }

        public CtSquadDef GetSquadDef(ushort identifier)
        {
            if (!_squadDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find squad by identifier (identifier={identifier}).");
#endif
                return null;
            }
            return (CtSquadDef)dataToken.Reference;
        }
    }
}