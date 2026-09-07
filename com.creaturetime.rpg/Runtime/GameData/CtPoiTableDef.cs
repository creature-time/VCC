
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtPoiTableDef : UdonSharpBehaviour
    {
        [SerializeField] private float startEasyWeight = 3.0f;
        [SerializeField] private float modEasyWeight = 0.2f;

        [SerializeField] private float startMediumWeight = 1.5f;
        [SerializeField] private float modMediumWeight = 0.2f;

        [SerializeField] private float startHardWeight;
        [SerializeField] private float modHardWeight = 0.2f;

        [SerializeField] private float startEliteWeight;
        [SerializeField] private float modEliteWeight = 0.2f;

        [SerializeField] private float startCampSiteWeight;
        [SerializeField] private float modCampSiteWeight = 0.2f;

        [SerializeField] private float startMerchantWeight;
        [SerializeField] private float modMerchantWeight = 0.2f;

        public void ResetTable(CtPoiTable poiTable)
        {
            poiTable.ResetTable(startEasyWeight, startMediumWeight, startHardWeight, startEliteWeight,
                startCampSiteWeight, startMerchantWeight);
        }

        public void UpdateTable(CtPoiTable poiTable, EMapPoiType resetPoiType)
        {
            poiTable.UpdateTable(modEasyWeight, modMediumWeight, modHardWeight, modEliteWeight, modCampSiteWeight,
                modMerchantWeight, resetPoiType);
        }
    }
}