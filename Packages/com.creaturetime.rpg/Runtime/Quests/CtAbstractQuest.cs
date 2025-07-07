
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtAbstractQuest : CtInventoryItemDef
    {
        [SerializeField] private int levelReq;

        public virtual int LevelReq => levelReq;

        public abstract void Execute(CtParty party);
    }
}