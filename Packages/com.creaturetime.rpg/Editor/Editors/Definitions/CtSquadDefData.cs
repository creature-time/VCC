using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    public struct CtNpcDataBlock
    {
        [SerializeField] public CtNpcDefData slot0;
        [SerializeField] public CtNpcDefData slot1;
        [SerializeField] public CtNpcDefData slot2;
        [SerializeField] public CtNpcDefData slot3;
        [SerializeField] public CtNpcDefData slot4;

        public CtNpcDefData[] NpcDefs
        {
            get
            {
                var npcDefs = new List<CtNpcDefData>();
                if (slot0)
                    npcDefs.Add(slot0);
                if (slot1)
                    npcDefs.Add(slot1);
                if (slot2)
                    npcDefs.Add(slot2);
                if (slot3)
                    npcDefs.Add(slot3);
                if (slot4)
                    npcDefs.Add(slot4);

                return npcDefs.ToArray();
            }
        }
    }

    [Serializable]
    public struct CtSquadUserData
    {
        public GameObject userData;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "squadDefData", menuName = "CreatureTime/Rpg/Squad Definition", order = 1)]
    public class CtSquadDefData : CtAbstractDefData
    {
        public override string GenerateName =>
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override int Identifier => identifier;

        [SerializeField] public ushort identifier = CtConstants.InvalidId;
        [SerializeField] public string displayName;
        [SerializeField] public CtNpcDataBlock npcDataBlock;
        [SerializeField] public CtSquadUserData userData;
    }
}
