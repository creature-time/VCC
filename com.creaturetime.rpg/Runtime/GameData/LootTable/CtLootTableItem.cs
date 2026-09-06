
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtLootTableItem : CtAbstractLootTableObject
    {
        [CtItem, SerializeField] private string data = CtDataBlock.Serialize(CtDataBlock.InvalidData);

        public override bool IsNull => false;

        public override ulong CreateInstance() => CtDataBlock.Deserialize(data);
    }
}