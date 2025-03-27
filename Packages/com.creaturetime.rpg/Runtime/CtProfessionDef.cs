
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtProfessionDef : CtAbstractDefinition
    {
        [SerializeField] private string displayName;
        [SerializeField] private CtAttributeDef[] attributes;
        [SerializeField] private Color theme;

        public string DisplayName => displayName;
        public CtAttributeDef[] Attributes => attributes;
        public Color Theme => theme;
    }
}