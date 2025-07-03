
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

        private CtSoundEmitter _emitter;

        private AudioClip _clip;
        private bool _isLooping;
        private bool _playOnAwake;
        private float _pitch;

        public bool IsPlaying => _emitter && _emitter.IsPlaying;

        public CtSoundBuilder Setup(AudioClip clip, bool isLooping, bool playOnAwake, float pitch)
        {
            _clip = clip;
            _isLooping = isLooping;
            _playOnAwake = playOnAwake;
            _pitch = pitch;

            return this;
        }

        public void Play()
        {
            if (_emitter)
            {
                _emitter.Stop();
                _emitter = null;
            }

            if (!soundManager.TryGet(out _emitter)) return;

            _emitter.Initialize(_clip, _isLooping, _playOnAwake, _pitch);
            _emitter.Play();

            // Reset to defaults.
            _clip = null;
            _isLooping = false;
            _playOnAwake = false;
        }

        public void Stop()
        {
            if (_emitter)
                _emitter.Stop();
        }
    }
}