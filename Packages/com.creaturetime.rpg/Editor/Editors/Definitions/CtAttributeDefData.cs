
using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    [CreateAssetMenu(fileName = "attributeData", menuName = "CreatureTime/Rpg/Attribute Definition", order = 1)]
    public class CtAttributeDefData : ScriptableObject
    {
        [SerializeField] public ushort identifier;
        [SerializeField] public string displayName;
    }
}