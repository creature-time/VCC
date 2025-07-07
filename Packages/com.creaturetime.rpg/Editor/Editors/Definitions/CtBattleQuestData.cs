using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    public struct CtSquadCategoryDataBlock
    {
        [SerializeField] public CtSquadDefData squadDef0;
        [SerializeField] public CtSquadDefData squadDef1;
        [SerializeField] public CtSquadDefData squadDef2;
        [SerializeField] public CtSquadDefData squadDef3;

        public CtSquadDefData[] SquadDefs
        {
            get
            {
                var squadDefs = new List<CtSquadDefData>();
                if (squadDef0)
                    squadDefs.Add(squadDef0);
                if (squadDef1)
                    squadDefs.Add(squadDef1);
                if (squadDef2)
                    squadDefs.Add(squadDef2);
                if (squadDef3)
                    squadDefs.Add(squadDef3);

                return squadDefs.ToArray();
            }
        }
    }

    [Serializable]
    [CreateAssetMenu(fileName = "battleQuestData", menuName = "CreatureTime/Rpg/Battle Quest Definition", order = 1)]
    public class CtBattleQuestData : CtAbstractDefData
    {
        public override string GenerateName =>
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override int Identifier => identifier;

        [SerializeField] public ushort identifier;
        [SerializeField] public string displayName;
        [SerializeField] public Texture2D icon;
        [SerializeField, Range(0, 20)] public int levelReq;
        [SerializeField] public CtSquadCategoryDataBlock squadCategory0;
        [SerializeField] public CtSquadCategoryDataBlock squadCategory1;
        [SerializeField] public CtSquadCategoryDataBlock squadCategory2;
    }
}