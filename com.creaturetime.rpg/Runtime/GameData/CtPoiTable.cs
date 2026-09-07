
using UdonSharp;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtPoiTable : CtLoggerUdonScript
    {
        private float easyWeight;
        private float mediumWeight;
        private float hardWeight;
        private float eliteWeight;
        private float campSiteWeight;
        private float merchantWeight;

        public void ResetTable(float startEasyWeight, float startMediumWeight, float startHardWeight,
            float startEliteWeight, float startCampSiteWeight, float startMerchantWeight)
        {
            easyWeight = startEasyWeight;
            mediumWeight = startMediumWeight;
            hardWeight = startHardWeight;
            eliteWeight = startEliteWeight;
            campSiteWeight = startCampSiteWeight;
            merchantWeight = startMerchantWeight;
        }

        public void UpdateTable(float modEasyWeight, float modMediumWeight, float modHardWeight, float modEliteWeight,
            float modCampSiteWeight, float modMerchantWeight, EMapPoiType resetPoiType)
        {
            easyWeight += modEasyWeight;
            mediumWeight += modMediumWeight;
            hardWeight += modHardWeight;
            eliteWeight += modEliteWeight;
            campSiteWeight += modCampSiteWeight;
            merchantWeight += modMerchantWeight;

            switch (resetPoiType)
            {
                case EMapPoiType.Easy:
                    easyWeight = 0;
                    break;
                case EMapPoiType.Medium:
                    mediumWeight = 0;
                    break;
                case EMapPoiType.Hard:
                    hardWeight = 0;
                    break;
                case EMapPoiType.Elite:
                    eliteWeight = 0;
                    break;
                case EMapPoiType.CampSite:
                    campSiteWeight = 0;
                    break;
                case EMapPoiType.Merchant:
                    merchantWeight = 0;
                    break;
                default:
                    LogCritical($"Invalid reset poi type (resetPoiTyp={resetPoiType}.");
                    return;
            }
        }

        public float[] ToArray => new float[]
        {
            easyWeight,
            mediumWeight,
            hardWeight,
            eliteWeight,
            campSiteWeight,
            merchantWeight
        };
    }
}