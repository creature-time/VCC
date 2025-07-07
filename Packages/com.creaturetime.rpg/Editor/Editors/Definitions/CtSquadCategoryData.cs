
using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    [CreateAssetMenu(fileName = "squadCategoryData", menuName = "CreatureTime/Rpg/Squad Category Definition", order = 1)]
    public class CtSquadCategoryData : CtAbstractDefData
    {
        public override string GenerateName =>
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override int Identifier => identifier;

        [SerializeField] public ushort identifier;
        [SerializeField] public string displayName;
    }
}
