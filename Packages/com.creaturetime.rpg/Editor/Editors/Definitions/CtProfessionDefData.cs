
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    [CreateAssetMenu(fileName = "professionData", menuName = "CreatureTime/Rpg/Profession Definition", order = 1)]
    public class CtProfessionDefData : CtAbstractDefData
    {
        public override string GenerateName => 
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override ushort Identifier => identifier;

        [SerializeField] public ushort identifier = CtConstants.InvalidId;
        [SerializeField] public string displayName;
        [SerializeField] public Texture2D icon;
        [SerializeField] public Color theme = Color.black;
        [SerializeField] public CtAttributeDefData attributes1;
        [SerializeField] public CtAttributeDefData attributes2;
        [SerializeField] public CtAttributeDefData attributes3;
        [SerializeField] public CtAttributeDefData attributes4;
        [SerializeField] public CtAttributeDefData attributes5;

        public CtAttributeDefData[] Attributes
        {
            get
            {
                var attributes = new List<CtAttributeDefData>();
                if (attributes1 && attributes1.identifier != CtConstants.InvalidId)
                    attributes.Add(attributes1);
                if (attributes2 && attributes2.identifier != CtConstants.InvalidId)
                    attributes.Add(attributes2);
                if (attributes3 && attributes3.identifier != CtConstants.InvalidId)
                    attributes.Add(attributes3);
                if (attributes4 && attributes4.identifier != CtConstants.InvalidId)
                    attributes.Add(attributes4);
                if (attributes5 && attributes5.identifier != CtConstants.InvalidId)
                    attributes.Add(attributes5);
                attributes.Sort((a, b) => a.identifier.CompareTo(b.identifier));
                return attributes.ToArray();
            }
        }
    }
}