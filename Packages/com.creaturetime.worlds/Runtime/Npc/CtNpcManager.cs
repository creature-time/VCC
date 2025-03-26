
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;

namespace CreatureTime
{
    [DefaultExecutionOrder(-1)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNpcManager : CtObjectSync
    {
        private const float compressDataPercision = 100.0f;
        private const float decompressDataPercision = 1 / compressDataPercision;
        private const int compressDataSize = 16;

        [SerializeField] private NavMeshAgent[] agents;

        [UdonSynced] private byte[] _data;

        private Vector3[][] _positions;
        private Quaternion[][] _rotations;
        private Vector3[][] _velocities;

        private void Start()
        {
            _positions = new Vector3[maxSnapshotCount][];

            _data = new byte[agents.Length * compressDataSize];

            _positions = new Vector3[maxSnapshotCount][];
            _rotations = new Quaternion[maxSnapshotCount][];
            _velocities = new Vector3[maxSnapshotCount][];
            for (int i = 0; i < maxSnapshotCount; i++)
            {
                _positions[i] = new Vector3[agents.Length];
                _rotations[i] = new Quaternion[agents.Length];
                _velocities[i] = new Vector3[agents.Length];
            }

            for (int i = 0; i < maxSnapshotCount - 1; i++)
            {
                for (int j = 0; j < agents.Length; j++)
                {
                    _positions[i][j] = Vector3.zero;
                    _rotations[i][j] = Quaternion.identity;
                    _velocities[i][j] = Vector3.zero;
                }
            }
        }

        public override void OnPreSerializeSyncData()
        {
            for (int i = 0; i < agents.Length; i++)
            {
                var agent = agents[i];
                _CompressData(_data, i * compressDataSize, 
                    agent.transform.position, agent.transform.rotation.eulerAngles.y, agent.velocity);
            }
        }

        public override void OnTakeSnapshot(float timestamp, int index)
        {
            Vector3 rotationVector = Vector3.zero;
            var localData = _positions[index];
            var localRotation = _rotations[index];
            var localVelocity = _velocities[index];
            for (int i = 0; i < agents.Length; i++)
            {
                _DecompressData(_data, i * compressDataSize, 
                    out localData[i], out rotationVector.y, out localVelocity[i]);
                localRotation[i] = Quaternion.Euler(rotationVector);
            }
        }

        public override void OnUpdateSync(int a, int b, float t)
        {
            Vector3[] prevPosition = _positions[a];
            Vector3[] nextPosition = _positions[b];
            Quaternion[] prevRotation = _rotations[a];
            Quaternion[] nextRotation = _rotations[b];
            Vector3[] prevVelocity = _velocities[a];
            Vector3[] nextVelocity = _velocities[b];

            for (int i = 0; i < agents.Length; i++)
            {
                var agent = agents[i];
                if (agent)
                {
                    agent.transform.position = Vector3.Slerp(prevPosition[i], nextPosition[i], t);
                    agent.transform.rotation = Quaternion.Lerp(prevRotation[i], nextRotation[i], t);
                    agent.velocity = Vector3.Slerp(prevVelocity[i], nextVelocity[i], t);
                }
            }
        }

        private static void _CompressData(byte[] data, int offset, 
            Vector3 position, float yRotationComponent, Vector3 velocity)
        {
            ushort x = (ushort)(position.x * compressDataPercision + 32767.0f);
            byte[] xBytes = BitConverter.GetBytes(x);

            ushort y = (ushort)(position.y * compressDataPercision + 32767.0f);
            byte[] yBytes = BitConverter.GetBytes(y);

            ushort z = (ushort)(position.z * compressDataPercision + 32767.0f);
            byte[] zBytes = BitConverter.GetBytes(z);

            ushort yRot = (ushort)((yRotationComponent - 180.0f) * compressDataPercision + 32767.0f);
            byte[] yRotBytes = BitConverter.GetBytes(yRot);

            ushort xVel = (ushort)(velocity.x * compressDataPercision + 32767.0f);
            byte[] xVelBytes = BitConverter.GetBytes(xVel);

            ushort yVel = (ushort)(velocity.y * compressDataPercision + 32767.0f);
            byte[] yVelBytes = BitConverter.GetBytes(yVel);

            ushort zVel = (ushort)(velocity.z * compressDataPercision + 32767.0f);
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

        private static void _DecompressData(byte[] data, int offset, 
            out Vector3 position, out float yRotationComponent, out Vector3 velocity)
        {
            ushort x = BitConverter.ToUInt16(data, offset + 0);
            position.x = (x - 32767.0f) * decompressDataPercision;

            ushort y = BitConverter.ToUInt16(data, offset + 2);
            position.y = (y - 32767.0f) * decompressDataPercision;

            ushort z = BitConverter.ToUInt16(data, offset + 4);
            position.z = (z - 32767.0f) * decompressDataPercision;

            ushort yRot = BitConverter.ToUInt16(data, offset + 6);
            yRotationComponent = (yRot - 32767.0f) * decompressDataPercision + 180.0f;

            x = BitConverter.ToUInt16(data, offset + 8);
            velocity.x = (x - 32767.0f) * decompressDataPercision;

            y = BitConverter.ToUInt16(data, offset + 10);
            velocity.y = (y - 32767.0f) * decompressDataPercision;

            z = BitConverter.ToUInt16(data, offset + 12);
            velocity.z = (z - 32767.0f) * decompressDataPercision;
        }
    }
}