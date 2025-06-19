
using System;
using UdonSharp;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNetConnection : CtLoggerUdonScript
    {
        [UdonSynced] private byte[] _packet = { };

        private CtNetSocket _netSocket;

        private void Start()
        {
            _netSocket = GetComponentInParent<CtNetSocket>();
        }

        public void SendNextPacket(byte[] data)
        {
#if DEBUG_LOGS
            if (!Networking.IsOwner(gameObject))
                LogCritical("Net Connection", "Packet sender was not owner of connection.");
#endif

            _packet = data;
            RequestSerialization();
            OnDeserialization();
        }

        private void _HandlePacket()
        {
            if (_packet.Length > 4)
            {
                int offset = 0;

                int header = BitConverter.ToInt32(_packet, offset);
                offset += 4;

                ESendMessageFlags flags = (ESendMessageFlags)header;
                switch (flags)
                {
                    case ESendMessageFlags.MasterOnly:
                        if (!Networking.IsMaster)
                        {
                            return;
                        }

                        break;
                }

                byte[] data = new byte[_packet.Length - offset];
                Array.Copy(_packet, offset, data, 0, data.Length);
                _netSocket.OnHandlePacket(data);
            }
        }

        public override void OnDeserialization()
        {
#if DEBUG_LOGS
            LogDebug("Net Connection",
                $"OnDeserialization (Data.Length={_packet.Length}, IsOwner={Networking.IsOwner(gameObject)})");
#endif

            _HandlePacket();
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
#if DEBUG_LOGS
            LogDebug("Net Connection",
                $"Player Restored (displayName={player.displayName}, playerId={player.playerId})");
#endif

            if (!player.isLocal || !Networking.IsOwner(gameObject))
                return;

            _netSocket.Connect(this);
        }

        private void OnDestroy()
        {
            if (_netSocket.LocalConnection == this)
                _netSocket.Disconnect();
        }
    }
}