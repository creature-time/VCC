
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtArmorSetDef : CtAbstractDefinition
    {
        [SerializeField] private string displayName;
        [SerializeField] private EItemRarity rarity;
        [SerializeField] private CtArmorSlotDef headSlot;
        [SerializeField] private CtArmorSlotDef chestSlot;
        [SerializeField] private CtArmorSlotDef handsSlot;
        [SerializeField] private CtArmorSlotDef legsSlot;
        [SerializeField] private CtArmorSlotDef feetSlot;

        public string DisplayName => displayName;
        public EItemRarity Rarity => rarity;
        public CtArmorSlotDef HeadSlot => headSlot;
        public CtArmorSlotDef ChestSlot => chestSlot;
        public CtArmorSlotDef HandsSlot => handsSlot;
        public CtArmorSlotDef LegsSlot => legsSlot;
        public CtArmorSlotDef FeetSlot => feetSlot;

        public CtArmorSlotDef GetArmorSlot(EArmorSlot armorSlot)
        {
            switch (armorSlot)
            {
                case EArmorSlot.Head:
                    return headSlot;
                case EArmorSlot.Chest:
                    return chestSlot;
                case EArmorSlot.Hands:
                    return handsSlot;
                case EArmorSlot.Legs:
                    return legsSlot;
                case EArmorSlot.Feet:
                    return feetSlot;
                default:
#if DEBUG_LOGS
                    LogCritical($"Unknown armor slot (armorSlot={armorSlot}).");
#endif
                    return null;
            }
        }

        public static int GetArmorIndex(EArmorSlot armorSlot)
        {
            switch (armorSlot)
            {
                case EArmorSlot.Head:
                    return 0;
                case EArmorSlot.Chest:
                    return 1;
                case EArmorSlot.Hands:
                    return 2;
                case EArmorSlot.Legs:
                    return 3;
                case EArmorSlot.Feet:
                    return 4;
                default:
#if DEBUG_LOGS
                    Debug.LogError($"Unknown armor slot (armorSlot={armorSlot}).");
#endif
                    return -1;
            }
        }

        public static EArmorSlot RollArmorHit()
        {
            double roll = CtRandomizer.GetDoubleValue(100.0);
            if (roll < 12.5)
                return EArmorSlot.Head;
            if (roll < 25.0)
                return EArmorSlot.Hands;
            if (roll < 37.5)
                return EArmorSlot.Feet;
            if (roll < 75.0)
                return EArmorSlot.Legs;
            return EArmorSlot.Chest;
        }
    }
}