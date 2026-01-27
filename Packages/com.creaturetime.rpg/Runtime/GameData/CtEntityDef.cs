
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EEntityDefSignal
    {
        NameChanged,
        LevelChanged,
        ExpChanged,
        MainHandChanged,
        OffHandChanged,
        HeadSlotChanged,
        ChestSlotChanged,
        HandsSlotChanged,
        LegsSlotChanged,
        FeetSlotChanged,
        SkillSlotChanged,
        AttributesChanged,
        InventoryChanged,
        BarksChanged,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtEntityDef : CtAbstractSignal
    {
        public const int MaxSkillCount = 10;

        [SerializeField] protected CtGameData gameData;
        [SerializeField] protected string displayName;
        [SerializeField] protected Texture icon;

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(AttributeDataCallback))]
        private string attributeData = CtDataBlock.Serialize(CtDataBlock.InvalidData);

        public string AttributeDataCallback
        {
            get => attributeData;
            set
            {
                attributeData = value;
                this.Emit(EEntityDefSignal.AttributesChanged);
            }
        }

        public ulong AttributeData
        {
            get => CtDataBlock.Deserialize(AttributeDataCallback);
            set
            {
                AttributeDataCallback = CtDataBlock.Serialize(value);
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(MainHandWeaponCallback))]
        private string mainHandWeaponData = CtDataBlock.Serialize(CtDataBlock.InvalidData);

        public string MainHandWeaponCallback
        {
            get => mainHandWeaponData;
            set
            {
                mainHandWeaponData = value;
                this.Emit(EEntityDefSignal.MainHandChanged);
            }
        }

        public ulong MainHandWeapon
        {
            get => CtDataBlock.Deserialize(MainHandWeaponCallback);
            set
            {
                MainHandWeaponCallback = CtDataBlock.Serialize(value);
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(OffHandWeaponCallback))]
        private string offHandWeaponData = CtDataBlock.Serialize(CtDataBlock.InvalidData);

        public string OffHandWeaponCallback
        {
            get => offHandWeaponData;
            set
            {
                offHandWeaponData = value;
                this.Emit(EEntityDefSignal.OffHandChanged);
            }
        }

        public ulong OffHandWeapon
        {
            get => CtDataBlock.Deserialize(OffHandWeaponCallback);
            set
            {
                OffHandWeaponCallback = CtDataBlock.Serialize(value);
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(HeadSlotCallback))]
        private string headSlotData = CtDataBlock.Serialize(CtDataBlock.InvalidData);

        public string HeadSlotCallback
        {
            get => headSlotData;
            set
            {
                headSlotData = value;
                this.Emit(EEntityDefSignal.HeadSlotChanged);
            }
        }

        public ulong HeadSlot
        {
            get => CtDataBlock.Deserialize(HeadSlotCallback);
            set
            {
#if DEBUG_LOGS
                if (!_ValidateSLot(value, EArmorSlot.Head)) return;
#endif

                HeadSlotCallback = CtDataBlock.Serialize(value);
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(ChestSlotCallback))] 
        private string chestSlotData = CtDataBlock.Serialize(CtDataBlock.InvalidData);

        public string ChestSlotCallback
        {
            get => chestSlotData;
            set
            {
                chestSlotData = value;
                this.Emit(EEntityDefSignal.ChestSlotChanged);
            }
        }

        public ulong ChestSlot
        {
            get => CtDataBlock.Deserialize(ChestSlotCallback);
            set
            {
#if DEBUG_LOGS
                if (!_ValidateSLot(value, EArmorSlot.Chest)) return;
#endif

                ChestSlotCallback = CtDataBlock.Serialize(value);
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(HandsSlotCallback))] 
        private string handsSlotData = CtDataBlock.Serialize(CtDataBlock.InvalidData);

        public string HandsSlotCallback
        {
            get => handsSlotData;
            set
            {
                handsSlotData = value;
                this.Emit(EEntityDefSignal.HandsSlotChanged);
            }
        }

        public ulong HandsSlot
        {
            get => CtDataBlock.Deserialize(HandsSlotCallback);
            set
            {
#if DEBUG_LOGS
                if (!_ValidateSLot(value, EArmorSlot.Hands)) return;
#endif

                HandsSlotCallback = CtDataBlock.Serialize(value);
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(LegsSlotCallback))] 
        private string legsSlotData = CtDataBlock.Serialize(CtDataBlock.InvalidData);
        
        public string LegsSlotCallback
        {
            get => legsSlotData;
            set
            {
                legsSlotData = value;
                this.Emit(EEntityDefSignal.LegsSlotChanged);
            }
        }

        public ulong LegsSlot
        {
            get => CtDataBlock.Deserialize(LegsSlotCallback);
            set
            {
#if DEBUG_LOGS
                if (!_ValidateSLot(value, EArmorSlot.Legs)) return;
#endif

                LegsSlotCallback = CtDataBlock.Serialize(value);
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(FeetSlotCallback))] 
        private string feetSlotData = CtDataBlock.Serialize(CtDataBlock.InvalidData);

        public string FeetSlotCallback
        {
            get => feetSlotData;
            set
            {
                feetSlotData = value;
                this.Emit(EEntityDefSignal.FeetSlotChanged);
            }
        }

        public ulong FeetSlot
        {
            get => CtDataBlock.Deserialize(FeetSlotCallback);
            set
            {
#if DEBUG_LOGS
                if (!_ValidateSLot(value, EArmorSlot.Feet)) return;
#endif

                FeetSlotCallback = CtDataBlock.Serialize(value);
                RequestSerialization();
            }
        }

#if DEBUG_LOGS
        private bool _ValidateSLot(ulong data, EArmorSlot armorSlot)
        {
            if (!CtDataBlock.IsValid(data)) return true;

            var dataType = CtDataBlock.GetDataType(data);
            if (dataType != EDataType.Equipment)
            {
                LogCritical($"Invalid data was being applied to hands slot (dataType={dataType})");
                return false;
            }

            var dataArmorSlot = CtDataBlock.GetEquipmentSlot(data);
            if (dataArmorSlot != armorSlot)
            {
                LogCritical($"Invalid data was being applied to hands slot (dataArmorSlot={dataArmorSlot}, armorSlot={armorSlot})");
                return false;
            }

            return true;
        }
#endif

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SkillSlot0Callback))] 
        private ushort skillSlot0;

        public ushort SkillSlot0Callback
        {
            get => skillSlot0;
            set
            {
                skillSlot0 = value;
                SetArgs.Add(0);
                this.Emit(EEntityDefSignal.SkillSlotChanged);
            }
        }

        public ushort SkillSlot0
        {
            get => SkillSlot0Callback;
            set
            {
                SkillSlot0Callback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SkillSlot1Callback))] 
        private ushort skillSlot1;

        public ushort SkillSlot1Callback
        {
            get => skillSlot1;
            set
            {
                skillSlot1 = value;
                SetArgs.Add(1);
                this.Emit(EEntityDefSignal.SkillSlotChanged);
            }
        }

        public ushort SkillSlot1
        {
            get => SkillSlot1Callback;
            set
            {
                SkillSlot1Callback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SkillSlot2Callback))] 
        private ushort skillSlot2;

        public ushort SkillSlot2Callback
        {
            get => skillSlot2;
            set
            {
                skillSlot2 = value;
                SetArgs.Add(2);
                this.Emit(EEntityDefSignal.SkillSlotChanged);
            }
        }

        public ushort SkillSlot2
        {
            get => SkillSlot2Callback;
            set
            {
                SkillSlot2Callback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SkillSlot3Callback))] 
        private ushort skillSlot3;

        public ushort SkillSlot3Callback
        {
            get => skillSlot3;
            set
            {
                skillSlot3 = value;
                SetArgs.Add(3);
                this.Emit(EEntityDefSignal.SkillSlotChanged);
            }
        }

        public ushort SkillSlot3
        {
            get => SkillSlot3Callback;
            set
            {
                SkillSlot3Callback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SkillSlot4Callback))] 
        private ushort skillSlot4;

        public ushort SkillSlot4Callback
        {
            get => skillSlot4;
            set
            {
                skillSlot4 = value;
                SetArgs.Add(4);
                this.Emit(EEntityDefSignal.SkillSlotChanged);
            }
        }

        public ushort SkillSlot4
        {
            get => SkillSlot4Callback;
            set
            {
                SkillSlot4Callback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SkillSlot5Callback))] 
        private ushort skillSlot5;

        public ushort SkillSlot5Callback
        {
            get => skillSlot5;
            set
            {
                skillSlot5 = value;
                SetArgs.Add(5);
                this.Emit(EEntityDefSignal.SkillSlotChanged);
            }
        }

        public ushort SkillSlot5
        {
            get => SkillSlot5Callback;
            set
            {
                SkillSlot5Callback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SkillSlot6Callback))] 
        private ushort skillSlot6;

        public ushort SkillSlot6Callback
        {
            get => skillSlot6;
            set
            {
                skillSlot6 = value;
                SetArgs.Add(6);
                this.Emit(EEntityDefSignal.SkillSlotChanged);
            }
        }

        public ushort SkillSlot6
        {
            get => SkillSlot6Callback;
            set
            {
                SkillSlot6Callback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SkillSlot7Callback))] 
        private ushort skillSlot7;

        public ushort SkillSlot7Callback
        {
            get => skillSlot7;
            set
            {
                skillSlot7 = value;
                SetArgs.Add(7);
                this.Emit(EEntityDefSignal.SkillSlotChanged);
            }
        }

        public ushort SkillSlot7
        {
            get => SkillSlot7Callback;
            set
            {
                SkillSlot7Callback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SkillSlot8Callback))] 
        private ushort skillSlot8;

        public ushort SkillSlot8Callback
        {
            get => skillSlot8;
            set
            {
                skillSlot8 = value;
                SetArgs.Add(8);
                this.Emit(EEntityDefSignal.SkillSlotChanged);
            }
        }

        public ushort SkillSlot8
        {
            get => SkillSlot8Callback;
            set
            {
                SkillSlot8Callback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SkillSlot9Callback))] 
        private ushort skillSlot9;

        public ushort SkillSlot9Callback
        {
            get => skillSlot9;
            set
            {
                skillSlot9 = value;
                SetArgs.Add(9);
                this.Emit(EEntityDefSignal.SkillSlotChanged);
            }
        }

        public ushort SkillSlot9
        {
            get => SkillSlot9Callback;
            set
            {
                SkillSlot9Callback = value;
                RequestSerialization();
            }
        }

        public bool TryGetSkillIndex(ushort skillId, out int index)
        {
            var skills = new ushort[]
            {
                SkillSlot0,
                SkillSlot1,
                SkillSlot2,
                SkillSlot3,
                SkillSlot4,
                SkillSlot5,
                SkillSlot6,
                SkillSlot7,
                SkillSlot8,
                SkillSlot9
            };
            index = Array.IndexOf(skills, skillId);
            return index != -1;
        }

        public ushort GetSkill(int index)
        {
            switch (index)
            {
                case 0: return SkillSlot0;
                case 1: return SkillSlot1;
                case 2: return SkillSlot2;
                case 3: return SkillSlot3;
                case 4: return SkillSlot4;
                case 5: return SkillSlot5;
                case 6: return SkillSlot6;
                case 7: return SkillSlot7;
                case 8: return SkillSlot8;
                case 9: return SkillSlot9;
                default: return CtConstants.InvalidId;
            }
        }

        public void SetSkill(int index, ushort skillId)
        {
            switch (index)
            {
                case 0: SkillSlot0 = skillId; return;
                case 1: SkillSlot1 = skillId; return;
                case 2: SkillSlot2 = skillId; return;
                case 3: SkillSlot3 = skillId; return;
                case 4: SkillSlot4 = skillId; return;
                case 5: SkillSlot5 = skillId; return;
                case 6: SkillSlot6 = skillId; return;
                case 7: SkillSlot7 = skillId; return;
                case 8: SkillSlot8 = skillId; return;
                case 9: SkillSlot9 = skillId; return;
                default: return;
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(CharacterLevelCallback))]
        private int characterLevel = 0;

        public int CharacterLevelCallback
        {
            get => characterLevel;
            set
            {
                characterLevel = value;
                this.Emit(EEntityDefSignal.LevelChanged);
            }
        }

        public int CharacterLevel
        {
            get => CharacterLevelCallback;
            set
            {
                CharacterLevelCallback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(ExpCallback))]
        private int exp = 0;

        public int ExpCallback
        {
            get => exp;
            set
            {
                exp = value;
                this.Emit(EEntityDefSignal.ExpChanged);
            }
        }

        public int Exp
        {
            get => ExpCallback;
            set
            {
                ExpCallback = value;
                RequestSerialization();
            }
        }

        [SerializeField] private CtUserData userData;

        public string DisplayName => displayName;
        public Texture Icon => icon;

        public CtUserData UserData => userData;

        public void Copy(CtEntityDef other)
        {
            CharacterLevel = other.CharacterLevel;
            Exp = other.Exp;

            // Profession and Attributes
            AttributeData = other.AttributeData;

            // Weapons
            MainHandWeapon = other.MainHandWeapon;
            OffHandWeapon = other.OffHandWeapon;

            // Equipment
            HeadSlot = other.HeadSlot;
            ChestSlot = other.ChestSlot;
            HandsSlot = other.HandsSlot;
            LegsSlot = other.LegsSlot;
            FeetSlot = other.FeetSlot;

            // Skills
            SkillSlot0 = other.SkillSlot0;
            SkillSlot1 = other.SkillSlot1;
            SkillSlot2 = other.SkillSlot2;
            SkillSlot3 = other.SkillSlot3;
            SkillSlot4 = other.SkillSlot4;
            SkillSlot5 = other.SkillSlot5;
            SkillSlot6 = other.SkillSlot6;
            SkillSlot7 = other.SkillSlot7;
            SkillSlot8 = other.SkillSlot8;
            SkillSlot9 = other.SkillSlot9;

            // Stats
            exp = other.exp;

            RequestSerialization();
        }

        // TODO: Move these from this class.

        

        public int ExpToNextLevel => CalcExpPerLevel(CharacterLevel);
//         public int EnergyRegeneration
//         {
//             get
//             {
//                 int result = 2;
//                 ulong[] equipment =
//                 {
//                     HeadSlot,
//                     ChestSlot,
//                     HandsSlot,
//                     LegsSlot,
//                     FeetSlot
//                 };
//                 for (int i = 0; i < equipment.Length; i++)
//                 {
//                     var slotData = equipment[i];
//                     if (!CtDataBlock.IsValid(slotData)) continue;
//
//                     var slot = (EArmorSlot)i;
//                     var identifier = CtDataBlock.GetEquipmentIdentifier(slotData);
//                     var armorDef = gameData.GetArmorDef(identifier);
//                     if (!armorDef)
//                     {
// #if DEBUG_LOGS
//                         LogWarning($"Failed to find armor definition for equipment in armor slot (slot={slot}).");
// #endif
//                         continue;
//                     }
//
//                     var armorSlotDef = armorDef.GetArmorSlot(slot);
//                     if (!armorSlotDef)
//                     {
// #if DEBUG_LOGS
//                         LogWarning($"Failed to find slot for equipment in armor slot (armorDef={armorDef}, slot={slot}).");
// #endif
//                         continue;
//                     }
//
//                     result += armorSlotDef.EnergyRegenerationBonus;
//                 }
//
//                 return result;
//             }
//         }

        public int MaxEnergy => 20;
        public int MaxHealth => 100 + 20 * (CharacterLevel - 1);

        private static int CalcExpPerLevel(int level)
        {
            return 1400 + 600 * level;
        }

        public static int CalculateDamage(int baseDamage, int strikeLevel, int targetArmorLevel)
        {
            return (int)(baseDamage * Mathf.Pow(2, (strikeLevel - targetArmorLevel) / 40.0f));
        }
    }
}