
using UdonSharp;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtGameData : CtSingleton
    {
        // TODO: Profile getting data from objects. Udon is known to be slow but unsure if it will affect this at all.
        //       Can we collapse all these objects into "structs" within this object for quicker lookup?

        private DataDictionary _npcDefinitions = new DataDictionary();
        private DataDictionary _skillDefinitions = new DataDictionary();
        private DataDictionary _weaponDefinitions = new DataDictionary();
        private DataDictionary _offHandDefinitions = new DataDictionary();
        private DataDictionary _armorDefinitions = new DataDictionary();
        private DataDictionary _professionDefinitions = new DataDictionary();
        private DataDictionary _questDefinitions = new DataDictionary();
        private DataDictionary _squadDefinitions = new DataDictionary();

        public CtSkillDef[] Skills { get; private set; }
        public CtProfessionDef[] Professions { get; private set; }
        public CtAbstractQuest[] Quests { get; private set; }

        public void Init()
        {
            CtNpcDef[] npcDefs = GetComponentsInChildren<CtNpcDef>(true);
            for (int i = 0; i < npcDefs.Length; i++)
            {
                var npcDef = npcDefs[i];
                _npcDefinitions[npcDef.Identifier] = npcDef;
            }

            Skills = GetComponentsInChildren<CtSkillDef>(true);
            for (int i = 0; i < Skills.Length; i++)
            {
                var skillDef = Skills[i];
                _skillDefinitions[skillDef.Identifier] = skillDef;
            }

            CtWeaponDef[] weaponDefs = GetComponentsInChildren<CtWeaponDef>(true);
            for (int i = 0; i < weaponDefs.Length; i++)
            {
                var weaponDef = weaponDefs[i];
                _weaponDefinitions[weaponDef.Identifier] = weaponDef;
            }

            CtOffHandDef[] offHandDefs = GetComponentsInChildren<CtOffHandDef>(true);
            for (int i = 0; i < offHandDefs.Length; i++)
            {
                var offHandDef = offHandDefs[i];
                _offHandDefinitions[offHandDef.Identifier] = offHandDef;
            }

            CtArmorSetDef[] armorDefs = GetComponentsInChildren<CtArmorSetDef>(true);
            for (int i = 0; i < armorDefs.Length; i++)
            {
                var armorDef = armorDefs[i];
                _armorDefinitions[armorDef.Identifier] = armorDef;
            }

            Professions = GetComponentsInChildren<CtProfessionDef>(true);
            for (int i = 0; i < Professions.Length; i++)
            {
                var professionDef = Professions[i];
                _professionDefinitions[professionDef.Identifier] = professionDef;
            }

            Quests = GetComponentsInChildren<CtAbstractQuest>(true);
            for (int i = 0; i < Quests.Length; i++)
            {
                var quest = Quests[i];
                _questDefinitions[quest.Identifier] = quest;
            }

            CtSquadDef[] squadDefs = GetComponentsInChildren<CtSquadDef>(true);
            for (int i = 0; i < squadDefs.Length; i++)
            {
                var squadDef = squadDefs[i];
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