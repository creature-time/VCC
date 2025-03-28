
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
        EquipmentChanged,
        ProfessionChanged,
        StateChanged,
        HealthChanged,
        EnergyChanged,
        SkillChanged,
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
        private const int MaxSkillCount = 10;

        [Header("Meta Data")]

        [SerializeField] protected string displayName;
        [SerializeField] private Texture icon;

        [Header("Equipment")]

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(MainHandWeaponCallback))]
        private ulong mainHandWeaponData = CtDataBlock.InvalidData;

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(OffHandWeaponCallback))]
        private ulong offHandWeaponData = CtDataBlock.InvalidData;

        [SerializeField, UdonSynced]
        private ulong[] equipmentData = new ulong[MaxEquipmentCount] {
            CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData,
            CtDataBlock.InvalidData, CtDataBlock.InvalidData
        };
        private ulong[] _cmpEquipmentData = new ulong[MaxEquipmentCount] {
            CtDataBlock.InvalidData, CtDataBlock.InvalidData, CtDataBlock.InvalidData,
            CtDataBlock.InvalidData, CtDataBlock.InvalidData
        };

        [Header("Skills")]

        [SerializeField, UdonSynced]
        private ushort[] skills = new ushort[MaxSkillCount]
        {
            CtConstants.InvalidId, CtConstants.InvalidId, CtConstants.InvalidId, CtConstants.InvalidId, 
            CtConstants.InvalidId, CtConstants.InvalidId, CtConstants.InvalidId, CtConstants.InvalidId, 
            CtConstants.InvalidId, CtConstants.InvalidId 
        };
        private ushort[] _cmpSkills = new ushort[MaxSkillCount]
        {
            CtConstants.InvalidId, CtConstants.InvalidId, CtConstants.InvalidId, CtConstants.InvalidId, 
            CtConstants.InvalidId, CtConstants.InvalidId, CtConstants.InvalidId, CtConstants.InvalidId, 
            CtConstants.InvalidId, CtConstants.InvalidId 
        };

        [Header("Stats")]

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(CharacterLevelCallback))]
        private int characterLevel = 0;

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

        [SerializeField, UdonSynced, FieldChangeCallback(nameof(AttributeDataCallback))]
        private ulong attributeData = CtDataBlock.InvalidData;

        [SerializeField] private CtUserData userData;

        public CtUserData UserData => userData;

        // TEMP
        public int armorLevel = 40;

        public string DisplayName => displayName;
        public Texture Icon => icon;

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

        public ulong[] EquipmentData => equipmentData;

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

        public int Exp
        {
            get => ExpCallback;
            set
            {
                ExpCallback = value;
                RequestSerialization();
            }
        }

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

        public void Copy(CtEntityDef other)
        {
            CharacterLevel = other.CharacterLevel;
            Exp = other.Exp;

            // Weapons
            MainHandWeapon = other.MainHandWeapon;
            OffHandWeapon = other.OffHandWeapon;

            // Equipment
            for (int i = 0; i < equipmentData.Length; ++i)
                SetEquipment(i, other.equipmentData[i]);

            // Profession and Attributes
            AttributeData = other.AttributeData;

            // Skills
            for (int i = 0; i < skills.Length; ++i)
                SetSkill(i, other.skills[i]);

            // Stats
            exp = other.exp;

            RequestSerialization();
        }

        public void SetRenderTexture(RenderTexture renderTexture)
        {
            icon = renderTexture;
        }

        public void SetEquipment(int index, ulong data)
        {
            if (equipmentData[index] == data)
                return;
            equipmentData[index] = data;
            RequestSerialization();
            _OnEquipmentChanged(index);
        }

        public void SetSkill(int skillIndex, ushort identifier)
        {
            if (skills[skillIndex] == identifier)
                return;
            skills[skillIndex] = identifier;
            RequestSerialization();
            _OnSkillChanged(skillIndex);
        }

        public ushort GetSkill(int skillIndex)
        {
            return skills[skillIndex];
        }

        private void _OnSkillChanged(int index)
        {
            CtLogger.LogDebug("Entity Stats", $"Skill changed (index={index}, value={skills[index]})");

            _cmpSkills[index] = skills[index];
            this.Emit(EEntityStatsSignal.SkillChanged);
        }

        private void _OnEquipmentChanged(int index)
        {
            CtLogger.LogDebug("Entity Stats", $"Equipment changed (index={index}, value={equipmentData[index]})");

            _cmpEquipmentData[index] = equipmentData[index];
            this.Emit(EEntityStatsSignal.EquipmentChanged);
        }

        public override void OnDeserialization()
        {
            for (int i = 0; i < skills.Length; ++i)
                if (_cmpSkills[i] != skills[i])
                    _OnSkillChanged(i);

            for (int i = 0; i < equipmentData.Length; ++i)
                if (_cmpEquipmentData[i] != equipmentData[i])
                    _OnEquipmentChanged(i);
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