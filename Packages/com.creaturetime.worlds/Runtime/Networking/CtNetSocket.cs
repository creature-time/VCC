
using System;
using UdonSharp;
using VRC.SDKBase;

namespace CreatureTime
{
    [Flags]
    public enum ESendMessageFlags
    {
        None = 0,
        MasterOnly = 1 << 0,
        Client = 1 << 1
    }

    public enum ENetSocketSignal
    {
        PacketChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtNetSocket : CtAbstractSignal
    {
        private const int MaxQueue = 256;

        public CtNetConnection LocalConnection { get; private set; }

        public byte[] Packet { get; private set; } = { };

        private byte[][] _dataQueue = new byte[MaxQueue][];
        private int _dataQueueTail = 0;
        private int _dataQueueHead = 0;

        private void FixedUpdate()
        {
            byte[] data = _GetNextPacket();
            if (data != null)
            {
                LocalConnection.SendNextPacket(data);
            }
        }

        public void Connect(CtNetConnection localConnection)
        {
            CtLogger.LogDebug("Net Socket", "Connecting local connection.");

            if (!Networking.IsOwner(localConnection.gameObject))
            {
                CtLogger.LogCritical("Net Socket", "Local player must be the owner of the local connection.");
                return;
            }

            if (LocalConnection)
            {
                CtLogger.LogCritical("Net Socket", "Local connection already connected.");
                return;
            }

            LocalConnection = localConnection;
            enabled = true;
        }

        public void Disconnect()
        {
            CtLogger.LogDebug("Net Socket", "Disconnecting local connection.");

            enabled = false;
            LocalConnection = null;
        }

        public void Reset()
        {
            _dataQueueTail = 0;
            _dataQueueHead = 0;
            for(var i = 0; i < MaxQueue; i++)
            {
                _dataQueue[i] = null;
            }
        }

        private void _SendMessage(byte[] data, ESendMessageFlags flags)
        {
            int identifier = _dataQueueHead;
            CtLogger.LogDebug("Net Socket",
                $"Adding packet (Data.Length={data.Length}, Identifier={identifier}, flags={flags}, " +
                $"owner={Networking.IsOwner(LocalConnection.gameObject)}, object={gameObject})");

            byte[] message = new byte[data.Length + 4];
            int offset = 0;
            CtBinaryUtils.ToBytes((int)flags, ref message, ref offset);
            Array.Copy(data, 0, message, offset, data.Length);

            _dataQueue[identifier] = message;
            _dataQueueHead = (identifier + 1) % MaxQueue;
        }

        public void SendAll(byte[] data)
        {
            _SendMessage(data, ESendMessageFlags.None);
        }

        public void SendToMasterOnly(byte[] data)
        {
            _SendMessage(data, ESendMessageFlags.MasterOnly);
        }

        private byte[] _GetNextPacket()
        {
            if (_dataQueueHead != _dataQueueTail)
            {
                int identifier = _dataQueueTail;
                byte[] data = _dataQueue[identifier];
                CtLogger.LogDebug("Net Socket",
                    $"Sending next packet (Data.Length={data.Length}, Identifier={identifier})");
                _dataQueue[identifier] = null;
                _dataQueueTail = (identifier + 1) % MaxQueue;
                return data;
            }
            return null;
        }

        public void OnHandlePacket(byte[] data)
        {
            Packet = data;
            this.Emit(ENetSocketSignal.PacketChanged);
        }
    }
}