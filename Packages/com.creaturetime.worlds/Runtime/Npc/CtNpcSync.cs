
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNpcSync : CtLoggerUdonScript
    {
        const float compressDataPercision = 100.0f;
        const float decompressDataPercision = 1 / compressDataPercision;
        const int compressDataSize = 16;

        [SerializeField] private CtNpcManager npcManager;
        [SerializeField] private NavMeshAgent npcAgent;
        [SerializeField] private Animator npcAnimator;
        [SerializeField] private string forwardVelocityAnimParam = "ForwardVelocity";
        [SerializeField] private string rightVelocityAnimParam = "RightVelocity";

        [SerializeField] private float packetsPerSecond = 10f;
        [SerializeField] private int maxSnapshotCount = 32;
        [SerializeField] private float oldestSnapshotTimeInSeconds = 1f;

        private int _forwardVelocity;
        private int _rightVelocity;
        private Transform _npcTransform;

        // Networked synced data
        [UdonSynced] private byte[] _data = new byte[compressDataSize];

        // Snapshot data
        private float[] _snapshotTimestamp;
        private Vector3[] _localData;
        private Quaternion[] _localRotations;
        private Vector3[] _localVelocity;

        // Rate limiter
        private float _frameSize;
        private float _delta;

        // Queue
        private int _head;
        private int _tail;
        private int _diff;

        private void Start()
        {
            _forwardVelocity = Animator.StringToHash(forwardVelocityAnimParam);
            _rightVelocity = Animator.StringToHash(rightVelocityAnimParam);
            _npcTransform = npcAgent.transform;

            _snapshotTimestamp = new float[maxSnapshotCount];
            _localData = new Vector3[maxSnapshotCount];
            _localRotations = new Quaternion[maxSnapshotCount];
            _localVelocity = new Vector3[maxSnapshotCount];

            _frameSize = 1 / packetsPerSecond;

            npcManager.AddNpc(this);
        }

        private void Update()
        {
            // if (Networking.IsOwner(gameObject))
            // {
            //     if (_delta >= _frameSize)
            //     {
            //         _delta %= _frameSize;
            //
            //         RequestSerialization();
            //         _TakeSnapshot(Time.time);
            //         _tail = _head;
            //         _diff = 0;
            //
            //         _delta = 0;
            //     }
            //     else
            //     {
            //         _delta += Time.deltaTime;
            //     }
            // }
            // else
            // {
            //     if (_diff == 0)
            //     {
            //         _delta = 0;
            //     }
            //     else if (_diff < 2)
            //     {
            //         // float max = _diff * _frameSize;
            //         // float floor = (_diff - 1) * _frameSize;
            //         _delta += Time.deltaTime * .5f;// * (1f - Mathf.Clamp01((floor + _delta) / max)) * .5f;
            //     }
            //     else if (_diff > 2)
            //     {
            //         _delta += Time.deltaTime * 1.5f;
            //     }
            //     else
            //     {
            //         _delta += Time.deltaTime;
            //     }
            //
            //     if (_delta >= _frameSize)
            //     {
            //         _tail = (_tail + 1) % maxSnapshotCount;
            //         _delta -= _frameSize;
            //         _diff--;
            //     }
            //
            //     _UpdateSyncedVariables();
            // }

            var direction = _npcTransform.TransformDirection(npcAgent.velocity / npcAgent.speed);
            npcAnimator.SetFloat(_forwardVelocity, direction.z);
            npcAnimator.SetFloat(_rightVelocity, direction.x);
        }

        public static void CompressData(byte[] data, int offset, Vector3 position, float yRotationComponent, Vector3 velocity)
        {
            ushort x = (ushort)(position.x * compressDataPercision + 32767.0f);
            byte[] xBytes = BitConverter.GetBytes(x);

            ushort y = (ushort)(position.y * compressDataPercision + 32767.0f);
            byte[] yBytes = BitConverter.GetBytes(y);

            ushort z = (ushort)(position.z * compressDataPercision + 32767.0f);
            byte[] zBytes = BitConverter.GetBytes(z);

            ushort yRot = (ushort)((yRotationComponent - 180.0f) * compressDataPercision + 32767.0f);
            byte[] yRotBytes = BitConverter.GetBytes(yRot);

            ushort xVel = (ushort)(position.x * compressDataPercision + 32767.0f);
            byte[] xVelBytes = BitConverter.GetBytes(xVel);

            ushort yVel = (ushort)(position.y * compressDataPercision + 32767.0f);
            byte[] yVelBytes = BitConverter.GetBytes(yVel);

            ushort zVel = (ushort)(position.z * compressDataPercision + 32767.0f);
            byte[] zVelBytes = BitConverter.GetBytes(zVel);

            Buffer.BlockCopy(xBytes, 0, data, offset, xBytes.Length);
            offset += xBytes.Length;

            Buffer.BlockCopy(yBytes, 0, data, offset, yBytes.Length);
            offset += yBytes.Length;

            Buffer.BlockCopy(zBytes, 0, data, offset, zBytes.Length);
            offset += zBytes.Length;

            Buffer.BlockCopy(yRotBytes, 0, data, offset, yRotBytes.Length);
            offset += yRotBytes.Length;

            Buffer.BlockCopy(xVelBytes, 0, data, offset, xVelBytes.Length);
            offset += xVelBytes.Length;

            Buffer.BlockCopy(yVelBytes, 0, data, offset, yVelBytes.Length);
            offset += yVelBytes.Length;

            Buffer.BlockCopy(zVelBytes, 0, data, offset, zVelBytes.Length);
            offset += zVelBytes.Length;
        }

        public static void DecompressData(byte[] data, int offset, out Vector3 position, out float yRotationComponent, out Vector3 velocity)
        {
            ushort x = BitConverter.ToUInt16(data, offset + 0);
            position.x = (x - 32767.0f) * decompressDataPercision;

            ushort y = BitConverter.ToUInt16(data, offset + 2);
            position.y = (y - 32767.0f) * decompressDataPercision;

            ushort z = BitConverter.ToUInt16(data, offset + 4);
            position.z = (z - 32767.0f) * decompressDataPercision;

            ushort yRot = BitConverter.ToUInt16(data, offset + 6);
            yRotationComponent = (yRot - 32767.0f) * decompressDataPercision + 180.0f;

            x = BitConverter.ToUInt16(data, offset + 0);
            velocity.x = (x - 32767.0f) * decompressDataPercision;

            y = BitConverter.ToUInt16(data, offset + 2);
            velocity.y = (y - 32767.0f) * decompressDataPercision;

            z = BitConverter.ToUInt16(data, offset + 4);
            velocity.z = (z - 32767.0f) * decompressDataPercision;
        }

        public override void OnPreSerialization()
        {
            CompressData(_data, 0, npcAgent.transform.position, npcAgent.transform.rotation.eulerAngles.y, npcAgent.velocity);
        }

        public override void OnDeserialization(DeserializationResult result)
        {
            _TakeSnapshot(result.sendTime);
        }

        private void _TakeSnapshot(float timestamp)
        {
            int head = (_head + 1) % maxSnapshotCount;
            if (head == _tail)
            {
                LogWarning(
                    "Could not add snapshot since we hit limit for the number of snapshots " +
                    $"(maxSnapshotCount={maxSnapshotCount}).");
                return;
            }

            Vector3 rotationVector = Vector3.zero;
            _head = head;
            _snapshotTimestamp[_head] = timestamp;
            DecompressData(_data, 0, out _localData[_head], out rotationVector.y, out _localVelocity[_head]);
            _localRotations[_head] = Quaternion.Euler(rotationVector);

            _diff++;
        }

        private void _UpdateSyncedVariables()
        {
            float t = _delta / _frameSize;
            // Debug.Log($"_UpdateSyncedVariables {_head} {_tail} {_diff} {t}");

            Vector3 localData;
            Vector3 prevData = _localData[_tail];

            Quaternion localRotation;
            Quaternion prevRotation = _localRotations[_tail];
            
            Vector3 localVelocity;
            Vector3 prevVelocity = _localVelocity[_tail];

            if (_tail == _head)
            {
                localData = prevData;
                localRotation = prevRotation;
                localVelocity = prevVelocity;
            }
            else
            {
                int offset = (_tail + 1) % maxSnapshotCount;
                localData = _localData[offset];
                localRotation = _localRotations[offset];
                localVelocity = _localVelocity[offset];
            }

            npcAgent.transform.position = Vector3.Slerp(prevData, localData, t);
            npcAgent.transform.rotation = Quaternion.Lerp(prevRotation, localRotation, t);
            npcAgent.velocity = Vector3.Slerp(prevVelocity, localVelocity, t);
        }
    }
}