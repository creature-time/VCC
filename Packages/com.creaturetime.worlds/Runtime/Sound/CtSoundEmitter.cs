
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

        public void Initialize(AudioClip clip, bool isLooping, bool playOnAwake)
        {
            audioSource.clip = clip;
            audioSource.loop = isLooping;
            audioSource.playOnAwake = playOnAwake;
        }

        public void Play()
        {
            audioSource.Play();
        }

        public void Stop()
        {
            audioSource.Stop();
        }

        private void Update()
        {
            if (!audioSource.isPlaying)
                soundManager.Release(this);
        }
    }
}