
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtProfessionDef : CtInventoryItemDef
    {
        [SerializeField] private Color theme = Color.black;
        [SerializeField] private CtAttributeDef[] attributes;

        public Color Theme => theme;
        public CtAttributeDef[] Attributes => attributes;
    }
}