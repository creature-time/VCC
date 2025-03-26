
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace CreatureTime
{
    public abstract class CtObjectSync : CtLoggerUdonScript
    {
        [Header("Snapshots")]
        [SerializeField] protected int maxSnapshotCount = 32;

        [Header("Snapshot Rate")]
        [SerializeField] private float packetsPerSecond = 10f;
        [SerializeField] private float oldestSnapshotTimeInSeconds = 0.5f;

        // Snapshot queue
        private int _head;
        private int _tail;
        private int _diff;

        // Timers
        private float _delta;
        private float _frameSize;

        protected void Init()
        {
            _frameSize = 1 / packetsPerSecond;
        }

        private void Update()
        {
            if (Networking.IsOwner(gameObject))
            {
                if (_delta >= _frameSize)
                {
                    RequestSerialization();
                    SendCustomEventDelayedFrames(nameof(_DelayedEvent_TakeSnapshot), 0);
                    _delta %= _frameSize;
                }
                else
                {
                    _delta += Time.deltaTime;
                }
            }
            else
            {
                if (_diff == 0)
                {
                    _delta = 0;
                }
                else if (_diff < 2)
                {
                    _delta += Time.deltaTime * .5f;
                }
                else if (_diff < 3)
                {
                    _delta += Time.deltaTime * .75f;
                }
                else if (_diff > 3)
                {
                    _delta += Time.deltaTime * 1.5f;
                }
                else
                {
                    _delta += Time.deltaTime;
                }

                if (_delta >= _frameSize)
                {
                    _tail = (_tail + 1) % maxSnapshotCount;
                    _delta -= _frameSize;
                    _diff--;
                }

                _OnUpdateSync();
            }
        }

        public override void OnPreSerialization()
        {
            OnPreSerializeSyncData();
        }

        public override void OnDeserialization(DeserializationResult result)
        {
            _OnTakeSnapshot(result.sendTime);
        }

        public void _DelayedEvent_TakeSnapshot()
        {
            // Object owner will always have latest.
            _tail = _head;
            _diff = 0;
            OnTakeSnapshot(Time.time, _tail);
        }

        private void _OnTakeSnapshot(float timestamp)
        {
            int head = (_head + 1) % maxSnapshotCount;
            if (head == _tail)
            {
                LogWarning(
                    "Could not add snapshot since we hit limit for the number of snapshots " +
                    $"(maxSnapshotCount={maxSnapshotCount}).");
                return;
            }

            OnTakeSnapshot(timestamp, head);

            _head = head;
            _diff++;
        }

        private void _OnUpdateSync()
        {
            if (_tail == _head)
                OnUpdateSync(_head, _head, 0);
            else
                OnUpdateSync(_tail, (_tail + 1) % maxSnapshotCount, _delta / _frameSize);
        }

        public abstract void OnPreSerializeSyncData();
        public abstract void OnTakeSnapshot(float timestamp, int index);
        public abstract void OnUpdateSync(int a, int b, float t);
    }
}