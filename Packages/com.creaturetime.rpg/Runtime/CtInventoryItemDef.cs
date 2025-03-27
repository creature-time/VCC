
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtInventoryItemDef : CtAbstractDefinition
    {
        [SerializeField] private string displayName = "";
        [SerializeField] private Texture icon;

        public string DisplayName => displayName;
        public Texture Icon => icon;
    }
}
