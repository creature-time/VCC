
using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    [CreateAssetMenu(fileName = "NpcDefTypeData", menuName = "CreatureTime/Rpg/Npc Type Definition", order = 1)]
    public class CtNpcTypeDefData : CtAbstractDefData
    {
        public override string GenerateName => 
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override int Identifier => identifier;

        [SerializeField] public ushort identifier = CtConstants.InvalidId;
        [SerializeField] public string displayName;
    }
}