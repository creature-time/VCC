
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EEntityStatsSignal
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
        ProfessionChanged,
        StateChanged,
        HealthChanged,
        EnergyChanged,
        SkillSlot0Changed,
        SkillSlot1Changed,
        SkillSlot2Changed,
        SkillSlot3Changed,
        SkillSlot4Changed,
        SkillSlot5Changed,
        SkillSlot6Changed,
        SkillSlot7Changed,
        SkillSlot8Changed,
        SkillSlot9Changed,
        AttributesChanged,
        SkillRechargeChanged,
        SkillAdrenalineChanged,
        CombatEffectChanged,
        InventoryChanged,
        BarksChanged,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtEntityDef : CtAbstractSignal
    {
        private const int MaxEquipmentCount = 5;
        public const int MaxSkillCount = 10;

        [SerializeField] protected string displayName;
        [SerializeField] private Texture icon;

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(AttributeDataCallback))]
        private ulong attributeData = CtDataBlock.InvalidData;

        public ulong AttributeDataCallback
        {
            get => attributeData;
            set
            {
                attributeData = value;
                this.Emit(EEntityStatsSignal.AttributesChanged);
            }
        }

        public ulong AttributeData
        {
            get => AttributeDataCallback;
            set
            {
                AttributeDataCallback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(MainHandWeaponCallback))]
        private ulong mainHandWeaponData = CtDataBlock.InvalidData;

        public ulong MainHandWeaponCallback
        {
            get => mainHandWeaponData;
            set
            {
                mainHandWeaponData = value;
                this.Emit(EEntityStatsSignal.MainHandChanged);
            }
        }

        public ulong MainHandWeapon
        {
            get => MainHandWeaponCallback;
            set
            {
                MainHandWeaponCallback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(OffHandWeaponCallback))]
        private ulong offHandWeaponData = CtDataBlock.InvalidData;

        public ulong OffHandWeaponCallback
        {
            get => offHandWeaponData;
            set
            {
                offHandWeaponData = value;
                this.Emit(EEntityStatsSignal.OffHandChanged);
            }
        }

        public ulong OffHandWeapon
        {
            get => OffHandWeaponCallback;
            set
            {
                OffHandWeaponCallback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(HeadSlotCallback))]
        private ulong headSlotData = CtDataBlock.InvalidData;

        public ulong HeadSlotCallback
        {
            get => headSlotData;
            set
            {
                headSlotData = value;
                this.Emit(EEntityStatsSignal.HeadSlotChanged);
            }
        }

        public ulong HeadSlot
        {
            get => HeadSlotCallback;
            set
            {
                HeadSlotCallback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(ChestSlotCallback))] 
        private ulong chestSlotData = CtDataBlock.InvalidData;

        public ulong ChestSlotCallback
        {
            get => chestSlotData;
            set
            {
                chestSlotData = value;
                this.Emit(EEntityStatsSignal.ChestSlotChanged);
            }
        }

        public ulong ChestSlot
        {
            get => ChestSlotCallback;
            set
            {
                ChestSlotCallback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(HandsSlotCallback))] 
        private ulong handsSlotData = CtDataBlock.InvalidData;

        public ulong HandsSlotCallback
        {
            get => handsSlotData;
            set
            {
                handsSlotData = value;
                this.Emit(EEntityStatsSignal.HandsSlotChanged);
            }
        }

        public ulong HandsSlot
        {
            get => HandsSlotCallback;
            set
            {
                HandsSlotCallback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(LegsSlotCallback))] 
        private ulong legsSlotData = CtDataBlock.InvalidData;
        
        public ulong LegsSlotCallback
        {
            get => legsSlotData;
            set
            {
                headSlotData = value;
                this.Emit(EEntityStatsSignal.LegsSlotChanged);
            }
        }

        public ulong LegsSlot
        {
            get => LegsSlotCallback;
            set
            {
                LegsSlotCallback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(FeetSlotCallback))] 
        private ulong feetSlotData = CtDataBlock.InvalidData;

        public ulong FeetSlotCallback
        {
            get => feetSlotData;
            set
            {
                feetSlotData = value;
                this.Emit(EEntityStatsSignal.FeetSlotChanged);
            }
        }

        public ulong FeetSlot
        {
            get => FeetSlotCallback;
            set
            {
                FeetSlotCallback = value;
                RequestSerialization();
            }
        }

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SkillSlot0Callback))] 
        private ushort skillSlot0;

        public ushort SkillSlot0Callback
        {
            get => skillSlot0;
            set
            {
                skillSlot0 = value;
                this.Emit(EEntityStatsSignal.SkillSlot0Changed);
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
                this.Emit(EEntityStatsSignal.SkillSlot1Changed);
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
                this.Emit(EEntityStatsSignal.SkillSlot2Changed);
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
                this.Emit(EEntityStatsSignal.SkillSlot3Changed);
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
                this.Emit(EEntityStatsSignal.SkillSlot4Changed);
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
                this.Emit(EEntityStatsSignal.SkillSlot5Changed);
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
                this.Emit(EEntityStatsSignal.SkillSlot6Changed);
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
                this.Emit(EEntityStatsSignal.SkillSlot7Changed);
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
                this.Emit(EEntityStatsSignal.SkillSlot8Changed);
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
                this.Emit(EEntityStatsSignal.SkillSlot9Changed);
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

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(CharacterLevelCallback))]
        private int characterLevel = 0;

        public int CharacterLevelCallback
        {
            get => characterLevel;
            set
            {
                characterLevel = value;
                this.Emit(EEntityStatsSignal.LevelChanged);
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
                this.Emit(EEntityStatsSignal.ExpChanged);
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

        public void SetRenderTexture(RenderTexture renderTexture)
        {
            icon = renderTexture;
        }

        // TODO: Move these from this class.

        public int ExpToNextLevel => CalcExpPerLevel(CharacterLevel);
        public int EnergyRegeneration => 2;
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