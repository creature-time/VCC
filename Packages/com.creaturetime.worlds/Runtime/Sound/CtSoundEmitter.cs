
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;

namespace CreatureTime
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(VRCSpatialAudioSource))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSoundEmitter : UdonSharpBehaviour
    {
        [SerializeField] private CtSoundManager soundManager;
        [SerializeField] private AudioSource audioSource;

        public bool IsPlaying => audioSource.isPlaying;

        public void Initialize(Vector3 position, AudioClip clip, bool isLooping, bool playOnAwake, float pitch, float volume, bool is2d)
        {
            transform.position = position;

            audioSource.clip = clip;
            audioSource.loop = isLooping;
            audioSource.playOnAwake = playOnAwake;
            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.spatialBlend = is2d ? 0f : 1f;
            audioSource.spatialize = !is2d;
        }

        public void Play()
        {
            audioSource.Play();
            enabled = true;
        }

        public void Stop()
        {
            audioSource.Stop();
        }

        private void Update()
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = null;
                soundManager.Release(this);
                enabled = false;
            }
        }
    }
}