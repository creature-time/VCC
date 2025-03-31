
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtDialogueDatabase : UdonSharpBehaviour
    {
        [SerializeField] private CtDialogueActor[] actors;
        [SerializeField] private CtConversation[] conversations;
        [SerializeField] private CtDialogueEntry[] entries;

        private DataDictionary _actors = new DataDictionary();
        private DataDictionary _conversations = new DataDictionary();

        private void Start()
        {
            foreach (var actor in actors)
                _actors.Add(actor.Identifier, actor);

            foreach (var conversation in conversations)
                _conversations.Add(conversation.Identifier, conversation);
        }

        public bool TryGetActor(ushort actorId, out CtDialogueActor actor)
        {
            actor = null;
            if (_actors.TryGetValue(actorId, out var token))
            {
                actor = (CtDialogueActor)token.Reference;
                return true;
            }

            return false;
        }

        public bool TryGetConversation(ushort conversationId, out CtConversation conversation)
        {
            conversation = null;
            if (_conversations.TryGetValue(conversationId, out var token))
            {
                conversation = (CtConversation)token.Reference;
                return true;
            }

            return false;
        }
    }
}