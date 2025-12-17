
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EPartySlotSignal
    {
        IdentifierChanged,
        Disconnected,
        Reconnected
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPartySlot : CtAbstractSignal
    {
        [UdonSynced, FieldChangeCallback(nameof(IdentifierCallback))]
        private ushort _identifier = CtConstants.InvalidId;

        [UdonSynced] private string _disconnectedUuid;
        public string DisconnectedUuid => _disconnectedUuid;

        public ushort IdentifierCallback
        {
            get => _identifier;
            set
            {
                _identifier = value;
                _disconnectedUuid = null;

                SetArgs.Add(_identifier);
                this.Emit(EPartySlotSignal.IdentifierChanged);
            }
        }

        public ushort Identifier
        {
            get => IdentifierCallback;
            set
            {
                IdentifierCallback = value;
                RequestSerialization();
            }
        }

        public CtEntity EntityCache { get; set; }

        public bool HasDisconnectedAlias => _disconnectedUuid != null;

        public void Reconnected(string uuid)
        {
            if (string.IsNullOrEmpty(_disconnectedUuid))
            {
                LogCritical("Cannot reconnect to party slot without an alias.");
                return;
            }

            if (_disconnectedUuid != uuid)
            {
                LogCritical("Cannot reconnect to party slot without matching aliases.");
                return;
            }

            _disconnectedUuid = null;
        }

        public void Disconnected(string uuid)
        {
            if (_disconnectedUuid != null)
            {
                LogCritical("Cannot disconnect from a slot that already been disconnected.");
                return;
            }

            _disconnectedUuid = uuid;
        }
    }
}