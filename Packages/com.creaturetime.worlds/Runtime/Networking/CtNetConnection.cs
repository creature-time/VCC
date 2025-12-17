
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNetConnection : CtLoggerUdonScript
    {
        [SerializeField] private CtNetSocket netSocket;

        [UdonSynced] private byte[] _packet = { };

        public void SendNextPacket(byte[] data)
        {
#if DEBUG_LOGS
            if (!Networking.IsOwner(gameObject))
                LogCritical("Packet sender was not owner of connection.");
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
                netSocket.OnHandlePacket(data);
            }
        }

        public override void OnDeserialization()
        {
#if DEBUG_LOGS
            LogDebug($"OnDeserialization (Data.Length={_packet.Length}, IsOwner={Networking.IsOwner(gameObject)})");
#endif

            _HandlePacket();
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
#if DEBUG_LOGS
            LogDebug($"Player Restored (displayName={player.displayName}, playerId={player.playerId})");
#endif

            if (!player.isLocal || !Networking.IsOwner(gameObject))
                return;

            netSocket.Connect(this);
        }

        private void OnDestroy()
        {
            if (netSocket.LocalConnection == this)
                netSocket.Disconnect();
        }
    }
}