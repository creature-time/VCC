
using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    [CreateAssetMenu(fileName = "attributeData", menuName = "CreatureTime/Rpg/Attribute Definition", order = 1)]
    public class CtAttributeDefData : CtAbstractDefData
    {
        public override string GenerateName => 
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override ushort Identifier => identifier;

        [SerializeField] public ushort identifier = CtConstants.InvalidId;
        [SerializeField] public string displayName;
        [SerializeField] public Texture2D icon;
    }
}