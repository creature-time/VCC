
using UdonSharp;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtLocationDef : CtAbstractDefinition
    {
        [SerializeField] private string displayName;
        [SerializeField] private int pathCount = 4;
        [SerializeField] private int maxNodeCount = -1;
        [SerializeField] private CtSquadDef[] easySquads;
        [SerializeField] private CtSquadDef[] mediumSquads;
        [SerializeField] private CtSquadDef[] hardSquads;
        [SerializeField] private CtSquadDef[] eliteSquads;
        [SerializeField] private CtSquadDef[] endBossSquads;

        [SerializeField] private CtPoiTableDef poiTableDef;

        public string DisplayName => displayName;

        public int PathCount => pathCount;
        public int MaxNodeCount => maxNodeCount;

        public CtSquadDef[] EasySquads => easySquads;
        public CtSquadDef[] MediumSquads => mediumSquads;
        public CtSquadDef[] HardSquads => hardSquads;
        public CtSquadDef[] EliteSquads => eliteSquads;
        public CtSquadDef[] EndBossSquads => endBossSquads;

        public CtPoiTableDef PoiTableDef => poiTableDef;

        public bool HasEndBoss => endBossSquads.Length > 0;

        public CtSquadDef RandomEasySquad => 
            easySquads.Length > 0 ? easySquads[Random.Range(0, easySquads.Length)] : null;
        public CtSquadDef RandomMediumSquad => 
            mediumSquads.Length > 0 ? mediumSquads[Random.Range(0, mediumSquads.Length)] : null;
        public CtSquadDef RandomHardSquad => 
            hardSquads.Length > 0 ? hardSquads[Random.Range(0, hardSquads.Length)] : null;
        public CtSquadDef RandomEliteSquad => 
            eliteSquads.Length > 0 ? eliteSquads[Random.Range(0, eliteSquads.Length)] : null;
        public CtSquadDef RandomBossSquad => 
            endBossSquads.Length > 0 ? endBossSquads[Random.Range(0, endBossSquads.Length)] : null;
    }
}