
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtLocationDef : CtAbstractDefinition
    {
        [SerializeField] private string displayName;
        [SerializeField] private CtSquadDef[] easySquads;
        [SerializeField] private CtSquadDef[] mediumSquads;
        [SerializeField] private CtSquadDef[] hardSquads;
        [SerializeField] private CtSquadDef[] bossSquads;

        public string DisplayName => displayName;
        public CtSquadDef[] EasySquads => easySquads;
        public CtSquadDef[] MediumSquads => mediumSquads;
        public CtSquadDef[] HardSquads => hardSquads;
        public CtSquadDef[] BossSquads => bossSquads;

        public CtSquadDef RandomEasySquad => 
            easySquads.Length > 0 ? easySquads[Random.Range(0, easySquads.Length)] : null;
        public CtSquadDef RandomMediumSquad => 
            mediumSquads.Length > 0 ? mediumSquads[Random.Range(0, mediumSquads.Length)] : null;
        public CtSquadDef RandomHardSquad => 
            hardSquads.Length > 0 ? hardSquads[Random.Range(0, hardSquads.Length)] : null;
        public CtSquadDef RandomBossSquad => 
            bossSquads.Length > 0 ? bossSquads[Random.Range(0, bossSquads.Length)] : null;
    }
}