
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace CreatureTime
{
    public class CtSingletonPreProcessor : IVRCSDKBuildRequestedCallback
    {
        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType != VRCSDKRequestedBuildType.Scene) return true;

            Debug.Log("Assigning singletons.");
            CtSingletonEditor.AssignSingletons(CtSingletonEditor.GetCurrentSingletonTypes());
            return true;
        }

        public int callbackOrder => 1000;
    }
}