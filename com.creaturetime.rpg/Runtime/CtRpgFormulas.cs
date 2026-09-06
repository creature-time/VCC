
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public class CtRpgFormulas : UdonSharpBehaviour
    {
        #region Experience & Level

        // Formula: expGained = level 100 +/- (4 + levelDiff * 16) with (-6 <= levelDiff <= 11)
        private static int[] GetExperienceTable() => 
            new int[18] { 0, 16, 32, 48, 64, 80, 100, 120, 136, 152, 168, 184, 200, 216, 232, 248, 264, 280 };
        private const int SameLevelIndex = 6;

        public static int CalcExperience(int playerLevel, int enemyLevel)
        {
            var experienceTable = GetExperienceTable();
            var levelDiff = enemyLevel - playerLevel;
            var expIndex = SameLevelIndex - levelDiff;
            expIndex = Mathf.Clamp(expIndex, 0, experienceTable.Length - 1);
            return experienceTable[expIndex];
        }

        // Formula: expToNextLevel = 1400 + 600 * level
        private static int[] _GetLevelTable() =>
            new int[22] {
                0,
                2000, 2600, 3200, 3800, 4400, 5000, 5600, 6200, 6800, 7400,
                8000, 8600, 9200, 9800, 10400, 11000, 11600, 12200, 12800, 13400,
                15000
            };

        public static void ConvertExpToLevel(int exp, out int level, out int expStart, out int expEnd, out int baseHealth)
        {
            var levelTable = _GetLevelTable();

            level = 1;
            expStart = levelTable[0];
            expEnd = levelTable[level];

            while (expEnd <= exp)
            {
                level++;
                expStart = expEnd;
                expEnd += levelTable[Mathf.Min(level, levelTable.Length - 1)];
            }

            baseHealth = 100 + 20 * level;
        }

        public static int ConvertLevelToMinExp(int level)
        {
            return CalcExpToLevel(level - 1);
        }

        private static int _CalcExpToNextLevel(int level, int[] levelTable)
        {
            if (level == 0) return 0;
            return levelTable[Mathf.Min(level, levelTable.Length - 1)] + _CalcExpToNextLevel(level - 1, levelTable);
        }

        public static int CalcExpToLevel(int level)
        {
            if (level == 0) return 0;
            var levelTable = _GetLevelTable();
            return _CalcExpToNextLevel(level, levelTable);
        }

        #endregion

        #region Damage Calculations

        private static float _CritChance(int sourceLevel, int targetLevel, int weaponAttributeLevel)
        {
            var a = 8 * sourceLevel;
            var b = 4 * weaponAttributeLevel;
            var c = 6 * Mathf.Min(weaponAttributeLevel, (sourceLevel + 4) / 2);
            var d = 15 * targetLevel;
            var pow = (a + b + c - d - 100) / 40;
            var baseCriticalChance =
                0.05f * Mathf.Pow(2, pow) * (1.0f - weaponAttributeLevel * 0.01f) +
                weaponAttributeLevel * 0.01f;
            return baseCriticalChance;
        }

        public static bool IsCritical(int sourceLevel, int targetLevel, int weaponAttributeLevel, float criticalChanceMod)
        {
            var criticalChance = _CritChance(sourceLevel, targetLevel, weaponAttributeLevel);
            criticalChance += criticalChanceMod;
            return Random.Range(0.0f, 1.0f) < criticalChance;
        }

        private static int _CalcDamageWithStrikeLevel(int baseDamage, int strikeLevel, int targetArmorLevel)
        {
            return (int)(baseDamage * Mathf.Pow(2, (strikeLevel - targetArmorLevel) / 40.0f));
        }

        public static int CalcSkillValue(float baseValue, float valuePerAttribute, int attributeRank)
        {
            return Mathf.RoundToInt(baseValue + valuePerAttribute * attributeRank);
        }

        public static int CalcValue(int skillValue, int sourceLevel, int targetArmorLevel)
        {
            var strikeLevel = 3 * sourceLevel;
            var damageTotal = _CalcDamageWithStrikeLevel(skillValue, strikeLevel, targetArmorLevel);
            return damageTotal;
        }

        public static int CalcDamage(float baseValue, float valuePerAttribute, int attributeRank,
            int characterLevel, int targetArmorLevel)
        {
            int skillValue = CalcSkillValue(baseValue, valuePerAttribute, attributeRank);
            return CalcValue(skillValue, characterLevel, targetArmorLevel);
        }

        public static int CalcHeal(float baseValue, float valuePerAttribute, int attributeRank)
        {
            return CalcDamage(baseValue, valuePerAttribute, attributeRank, 0, 0);
        }

        public static int CalcWeaponDamage(bool isCritical, int damageMin, int damageMax)
        {
            return isCritical
                ? (int)(damageMax * 1.2f)
                : Random.Range(damageMin, damageMax);
        }

        public static int CalcWeaponDamageModWithAttributeLevel(int weaponDamage, bool isCritical, int sourceLevel, 
            int reqAttributeLevel, int attributeLevel, int targetArmorRating)
        {
            var attributeThreshold = (sourceLevel + 4) / 2;
            var strikeLevel = isCritical
                ? reqAttributeLevel + 20
                : 5 * Mathf.Min(reqAttributeLevel, attributeThreshold) +
                  2 * Mathf.Max(0, reqAttributeLevel - attributeThreshold);
            weaponDamage = _CalcDamageWithStrikeLevel(weaponDamage, strikeLevel, targetArmorRating);
            weaponDamage = Mathf.Max(0, weaponDamage);

            // If source does not meet requirements to use the weapon.
            if (attributeLevel < reqAttributeLevel)
                weaponDamage = (int)(weaponDamage * CtConstants.OneThirds);

            return weaponDamage;
        }

        #endregion

        #region Attributes

        private static int[] GetAttributePointTable() => new int[20]
        {
            0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 55, 65, 75, 85, 95, 110, 125, 140, 155, 170
        };

        public static int CalcAttributePoints(int level)
        {
            // TODO: How did we want to access these 30 extra points? Maybe unlocked by doing something special?
            var additionalPoints = 30;

            var attributeTable = GetAttributePointTable();
            return attributeTable[Mathf.Min(level - 1, attributeTable.Length - 1)] + additionalPoints;
        }

        #endregion
    }
}