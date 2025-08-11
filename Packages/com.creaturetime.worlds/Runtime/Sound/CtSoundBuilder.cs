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
        private float _volume;
        private bool _is2d;
        private Vector3 _position;

        public bool IsPlaying => _emitter && _emitter.IsPlaying;

        public CtSoundBuilder Setup(AudioClip clip, bool isLooping, bool playOnAwake, float pitch)
        {
            _clip = clip;
            _isLooping = isLooping;
            _playOnAwake = playOnAwake;
            _pitch = pitch;
            _volume = 1.0f;
            _is2d = false;
            _position = Vector3.zero;

            return this;
        }

        public CtSoundBuilder SetVolume(float volume)
        {
            _volume = volume;
            return this;
        }

        public CtSoundBuilder Set2D()
        {
            _is2d = true;
            return this;
        }

        public CtSoundBuilder SetPosition(Vector3 position)
        {
            _position = position;
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

            _emitter.Initialize(_position, _clip, _isLooping, _playOnAwake, _pitch, _volume, _is2d);
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