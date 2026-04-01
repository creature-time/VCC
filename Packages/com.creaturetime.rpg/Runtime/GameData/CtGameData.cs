
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
        [SerializeField] private CtQuestDef[] questDefinitions;
        [SerializeField] private CtBattleDef[] battleDefinitions;
        [SerializeField] private CtSquadDef[] squadDefinitions;
        [SerializeField] private CtLocationDef[] locationDefinitions;

        public CtNpcDef[] NpcDefinitions => npcDefinitions;
        public CtSkillDef[] SkillDefinitions => skillDefinitions;
        public CtWeaponDef[] WeaponDefinitions => weaponDefinitions;
        public CtOffHandDef[] OffHandDefinitions => offHandDefinitions;
        public CtArmorSetDef[] ArmorDefinitions => armorDefinitions;
        public CtProfessionDef[] ProfessionDefinitions => professionDefinitions;
        public CtAttributeDef[] AttributeDefinitions => attributeDefinitions;
        public CtQuestDef[] QuestDefinitions => questDefinitions;
        public CtBattleDef[] BattleDefinitions => battleDefinitions;
        public CtSquadDef[] SquadDefinitions => squadDefinitions;
        public CtLocationDef[] LocationDefinitions => locationDefinitions;

        private DataDictionary _npcDefinitions = new DataDictionary();
        private DataDictionary _skillDefinitions = new DataDictionary();
        private DataDictionary _weaponDefinitions = new DataDictionary();
        private DataDictionary _offHandDefinitions = new DataDictionary();
        private DataDictionary _armorDefinitions = new DataDictionary();
        private DataDictionary _professionDefinitions = new DataDictionary();
        private DataDictionary _attributeDefinitions = new DataDictionary();
        private DataDictionary _battleDefinitions = new DataDictionary();
        private DataDictionary _squadDefinitions = new DataDictionary();
        private DataDictionary _questDefinitions = new DataDictionary();
        private DataDictionary _locationDefinitions = new DataDictionary();

        public override void Init()
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

            foreach (var battleDef in battleDefinitions)
            {
                _battleDefinitions[battleDef.Identifier] = battleDef;
            }

            foreach (var squadDef in squadDefinitions)
            {
                _squadDefinitions[squadDef.Identifier] = squadDef;
            }

            foreach (var questDef in questDefinitions)
            {
                _questDefinitions[questDef.Identifier] = questDef;
            }

            foreach (var locationDef in locationDefinitions)
            {
                _locationDefinitions[locationDef.Identifier] = locationDef;
            }

#if DEBUG_LOGS
            LogDebug("Data Initialized.");
#endif
        }

        // TODO: Convert all of these to "bool TryGet<T>(id, out T t)" methods.
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

        public CtBattleDef GetBattleDef(ushort identifier)
        {
            if (!_battleDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find battle by identifier (identifier={identifier}).");
#endif
                return null;
            }

            return (CtBattleDef)dataToken.Reference;
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

        public bool TryGetQuestDef(ushort identifier, out CtQuestDef questDef)
        {
            questDef = null;

            if (!_questDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find quest by identifier (identifier={identifier}).");
#endif
                return false;
            }

            questDef = (CtQuestDef)dataToken.Reference;
            return true;
        }

        public bool TryGetLocationDef(ushort identifier, out CtLocationDef locationDef)
        {
            locationDef = null;

            if (!_locationDefinitions.TryGetValue(identifier, out var dataToken))
            {
#if DEBUG_LOGS
                LogWarning($"Failed to find world by identifier (identifier={identifier}).");
#endif
                return false;
            }

            locationDef = (CtLocationDef)dataToken.Reference;
            return true;
        }
    }
}