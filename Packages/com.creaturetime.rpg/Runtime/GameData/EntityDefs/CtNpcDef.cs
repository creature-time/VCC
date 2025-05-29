
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtNpcDef : CtEntityDef
    {
        [SerializeField] private ushort identifier = CtConstants.InvalidId;
        [SerializeField] private CtNpcBehavior behavior;
        [SerializeField] private bool isRecruitable;

        public ushort Identifier => identifier;
        public CtNpcBehavior Behavior => behavior;
        public bool IsRecruitable => isRecruitable;
    }
}