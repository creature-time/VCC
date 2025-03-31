
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSoundBuilder : UdonSharpBehaviour
    {
        [SerializeField] private CtSoundManager soundManager;
        [SerializeField] private Vector3 position;
        [SerializeField] private bool randomPitch;

        private AudioClip _clip;
        private bool _isLooping;
        private bool _playOnAwake;

        public void Setup(AudioClip clip, bool isLooping, bool playOnAwake)
        {
            _clip = clip;
            _isLooping = isLooping;
            _playOnAwake = playOnAwake;
        }

        public void Play()
        {
            if (!soundManager.TryGet(out var soundEmitter)) return;

            soundEmitter.Initialize(_clip, _isLooping, _playOnAwake);
            soundEmitter.Play();

            // Reset to defaults.
            _clip = null;
            _isLooping = false;
            _playOnAwake = false;
        }
    }
}