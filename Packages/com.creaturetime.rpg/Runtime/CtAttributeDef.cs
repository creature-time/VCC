
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtAttributeDef : UdonSharpBehaviour
    {
        [SerializeField] private string displayName;
        [SerializeField] private EAttributeType attributeType = EAttributeType.None;

        public string DisplayName => displayName;
        public EAttributeType AttributeType => attributeType;
    }
}