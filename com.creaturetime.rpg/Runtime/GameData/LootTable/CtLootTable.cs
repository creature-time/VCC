
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtLootTable : CtAbstractLootTableObject
    {
        [SerializeField] private CtAbstractLootTableObject[] entries;

        [CtItem, SerializeField] private string[] testResults;

        public void _Test_GetResult()
        {
            testResults = new string[] { };
            var results = GetResult(10);
            foreach (var result in results)
                CtArrayUtils.Insert(ref testResults, CtDataBlock.Serialize(result.CreateInstance()), -1);
            LogDebug($"Testing results... (results={CtArrayUtils.DebugToString(testResults)}");
        }

        public override bool IsNull => false;

        public override ulong CreateInstance()
        {
            var subResult = GetResult(1);
            if (subResult.Length > 0)
                return subResult[0].CreateInstance();
            return base.CreateInstance();
        }

        public CtAbstractLootTableObject[] GetResult(int count)
        {
            foreach (var entry in entries)
                LogDebug(
                    $"Result {entry.gameObject.name}" +
                    $"(probability={entry.Probability}, unique={entry.Unique}, always={entry.Always}, enabled={entry.RollEnabled})");

            // foreach (CTRdsObject o in _table)
            //     o.Handler.OnPreResultEvaluation();

            var results = new CtAbstractLootTableObject[] { };

            // Setup probabilities for faster rolling.
            var probabilities = new float[entries.Length];
            for (var i = 0; i < entries.Length; i++)
            {
                // Handle always.
                var entry = entries[i];
                if (entry.Always && entry.RollEnabled)
                {
                    CtArrayUtils.Insert(ref results, entry, -1);
                    probabilities[i] = 0;
                    continue;
                }

                probabilities[i] = entry.Probability;
            }

            if (results.Length >= count) return results;

            // Roll what we couldn't roll for yet.
            var realDropCount = count - results.Length;
            for (var i = 0; i < realDropCount; ++i)
            {
                var index = CtRandomizer.GetRandomFromArray(probabilities);
                if (index == -1) break;

                // Handle uniques.
                var entry = entries[index];
                if (entry.Unique && entry.RollEnabled)
                    probabilities[index] = 0;

                CtArrayUtils.Insert(ref results, entry, -1);
            }

            // foreach (var entry in entries)
            // {
            //     if (entry.Always && entry.RollEnabled)
            //     {
            //         CtArrayUtils.Insert(ref results, entry, -1);
            //     }
            //
            //     if (entry.Unique && entry.RollEnabled && Array.IndexOf(results, entry) == -1)
            //         continue;
            //
            //     if (entry.IsNull)
            //     {
            //         continue;
            //     }
            //
            //     // // Subtables
            //     // if (entry.GetUdonTypeID() == GetUdonTypeID<CtLootTable>())
            //     // {
            //     //     var subTable = (CtLootTable)entry;
            //     //     // Do we want to handle more than one drop from sub table?
            //     //     var subResult = subTable.GetResult(1);
            //     //     if (subResult.Length > 0)
            //     //         CtArrayUtils.Insert(ref results, subResult[0], -1);
            //     //     continue;
            //     // }
            //
            //     CtArrayUtils.Insert(ref results, entry, -1);
            // }

            return results;
        }

        public static void GetItemsFromLootTable(CtLootTable lootTable, int rolls, out ulong[] items)
        {
            items = new ulong[] { };
            var loot = lootTable.GetResult(rolls);
            foreach (var item in loot)
            {
                if (item.IsNull) continue;
                CtArrayUtils.Add(ref items, item.CreateInstance());
            }
        }
    }
}