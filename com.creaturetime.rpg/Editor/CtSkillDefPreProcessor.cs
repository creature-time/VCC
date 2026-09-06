
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace CreatureTime.RpgGame
{
    public class CtSkillDefPreProcessor : IVRCSDKBuildRequestedCallback
    {
        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType != VRCSDKRequestedBuildType.Scene) return true;

            Debug.Log("Assigning skill hints.");
            CtSkillDefFuncs.AssignSkillFlags();
            return true;
        }

        public int callbackOrder => 0;
    }
}