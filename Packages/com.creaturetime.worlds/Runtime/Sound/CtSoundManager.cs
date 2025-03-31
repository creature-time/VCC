
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSoundManager : UdonSharpBehaviour
    {
        [SerializeField] private GameObject sourceEmitterPrefab;
        [SerializeField] int defaultCapacity = 10;
        [SerializeField] int maxPoolSize = 20;
        [SerializeField] int maxSoundInstances = 20;

        private CtSoundEmitter[] _sourceEmitterPool;
        private int _audioSourcePoolCount;
        private DataList _activeSoundEmitters = new DataList();

        private void Start()
        {
            _sourceEmitterPool = new CtSoundEmitter[maxPoolSize];
            for (int i = 0; i < defaultCapacity; i++)
                OnCreate();

            sourceEmitterPrefab.SetActive(false);
        }

        private CtSoundEmitter OnCreate()
        {
            var prefab = Instantiate(sourceEmitterPrefab, transform);
            prefab.SetActive(false);

            var soundEmitter = prefab.GetComponent<CtSoundEmitter>();
            _sourceEmitterPool[_audioSourcePoolCount++] = soundEmitter;
            return soundEmitter;
        }

        public bool TryGet(out CtSoundEmitter soundEmitter)
        {
            soundEmitter = null;
            if (_activeSoundEmitters.Count == _audioSourcePoolCount)
            {
                if (_audioSourcePoolCount == maxPoolSize) return false;
                soundEmitter = OnCreate();
            }
            else
            {
                foreach (var poolObject in _sourceEmitterPool)
                {
                    if (!_activeSoundEmitters.Contains(poolObject))
                    {
                        soundEmitter = poolObject;
                        break;
                    }
                }
            }

            soundEmitter.gameObject.SetActive(true);
            _activeSoundEmitters.Add(soundEmitter);

            return true;
        }

        public void Release(CtSoundEmitter sourceEmitter)
        {
            sourceEmitter.gameObject.SetActive(false);
            _activeSoundEmitters.Remove(sourceEmitter);
        }
    }
}