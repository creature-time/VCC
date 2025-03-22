
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace CreatureTime
{
    public enum ERpcType
    {
        None,
        Client,
        Server,
        NetMultiCast
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtRpc : UdonSharpBehaviour
    {
        [SerializeField] private CtNetSocket socket;

        [SerializeField] private int messageId;
        [SerializeField] private UdonSharpBehaviour targetScript;
        [SerializeField] private string methodName;
        [SerializeField] private ERpcType type;

        public DataList Args { get; private set; } = new DataList();

        private static byte[] _ArgsDataWithMsg(int messageId, byte[] data)
        {
            byte[] result = new byte[data.Length + 4];
            int offset = 0;
            CtBinaryUtils.ToBytes(messageId, ref data, ref offset);
            Array.Copy(data, 0, result, 4, data.Length);
            return result;
        }

        public void OnRecv(DataList args)
        {
            switch (type)
            {
                case ERpcType.Server:
                    if (!Networking.IsMaster)
                        return;

                    break;
                case ERpcType.Client:
                    if (!Networking.IsOwner(gameObject))
                        return;

                    break;
            }

            Args.Clear();
            Args.AddRange(args);
            targetScript.SendCustomEvent(methodName);
        }

        public void Send(DataList args)
        {
            // "Server"
            if (Networking.IsMaster)
            {
                switch (type)
                {
                    case ERpcType.None:
                        OnRecv(args);

                        break;
                    case ERpcType.NetMultiCast:
                        socket.SendAll(_ArgsDataWithMsg(messageId, CtBinaryUtils.ToData(args)));

                        break;
                    case ERpcType.Server:
                        OnRecv(args);

                        break;
                    case ERpcType.Client:
                        if (Networking.IsOwner(gameObject))
                            OnRecv(args);
                        else
                            // Runs on the "server".
                            socket.SendAll(_ArgsDataWithMsg(messageId, CtBinaryUtils.ToData(args)));

                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case ERpcType.None:
                        OnRecv(args);

                        break;
                    case ERpcType.NetMultiCast:
                        OnRecv(args);

                        break;
                    case ERpcType.Server:
                        if (Networking.IsOwner(gameObject))
                            // Runs on the "server".
                            socket.SendAll(_ArgsDataWithMsg(messageId, CtBinaryUtils.ToData(args)));
                        else if (Networking.Master.IsOwner(gameObject))
                            ; // Dropped!
                        else
                            ; // Dropped!

                        break;
                    case ERpcType.Client:
                        OnRecv(args);

                        break;
                }
            }
        }
    }
}