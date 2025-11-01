
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EActorType
    {
        Npc,
        Player,
        LocalPlayer
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtDialogueActor : UdonSharpBehaviour
    {
        [SerializeField] private ushort identifier = CtConstants.InvalidId;

        [SerializeField] private string actorName;
        [SerializeField] private EActorType actorType;
        [SerializeField] private Texture texture;
        [SerializeField] private CtUserData userData;

        public ushort Identifier => identifier;
        public string ActorName => actorName;
        public EActorType ActorType => actorType;
        public Texture Texture => texture;
        public CtUserData UserData => userData;
    }
}