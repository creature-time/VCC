
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtBlinkFeature : CtNpcFeature
    {
        [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
        [SerializeField] private Vector2 minMaxBlinkDelta = new Vector2(4.0f, 6.0f);
        [SerializeField] private float blinkHoldTimer = 0.1f;
        [SerializeField] private float blinkSpeed = 30.0f;
        [SerializeField] private int blinkEyeLeft = -1;
        [SerializeField] private int blinkEyeRight = -1;

        private float _blinkTimer;
        private float _blinkTargetValue;

        public override void ExecuteUpdate(CtNpcController context)
        {
            _blinkTimer -= Time.deltaTime;
            if (_blinkTimer <= 0)
                _blinkTimer = Random.Range(minMaxBlinkDelta.x, minMaxBlinkDelta.y);

            float t = 1.0f - Mathf.Exp(-blinkSpeed * Time.deltaTime);
            _blinkTargetValue = Mathf.Lerp(_blinkTargetValue, _blinkTimer < blinkHoldTimer ? 100.0f : 0.0f, t);

            if (blinkEyeLeft != -1)
                skinnedMeshRenderer.SetBlendShapeWeight(blinkEyeLeft, _blinkTargetValue);
            if (blinkEyeRight != -1)
                skinnedMeshRenderer.SetBlendShapeWeight(blinkEyeRight, _blinkTargetValue);
        }
    }
}