
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtInventoryItemDef : CtAbstractDefinition
    {
        [SerializeField] private string displayName = "";
        [SerializeField] private Texture2D icon;

        public string DisplayName => displayName;
        public Texture2D Icon => icon;
    }
}
