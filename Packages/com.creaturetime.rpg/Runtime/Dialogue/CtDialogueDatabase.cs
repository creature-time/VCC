
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtDialogueDatabase : UdonSharpBehaviour
    {
        [SerializeField] private CtDialogueActor[] actors;
        [SerializeField] private CtConversation[] conversations;
        [FormerlySerializedAs("chatterConversations")] [SerializeField] private CtChatter[] chatters;

        private DataDictionary _actors = new DataDictionary();
        private DataDictionary _conversations = new DataDictionary();
        private DataDictionary _chatters = new DataDictionary();

        private void Start()
        {
            foreach (var actor in actors)
                _actors.Add(actor.Identifier, actor);

            foreach (var conversation in conversations)
                _conversations.Add(conversation.Identifier, conversation);

            foreach (var chatter in chatters)
                _chatters.Add(chatter.Identifier, chatter);
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

        public bool TryGetDialogueEntry(ushort entryId, out CtDialogueEntry entry)
        {
            entry = null;
            var keys = _conversations.GetKeys();
            for (var i = 0; i < keys.Count; i++)
            {
                var conversation = (CtConversation)_conversations[keys[i]].Reference;
                if (conversation.TryGetEntry(entryId, out var subEntry))
                {
                    entry = subEntry;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetChatter(ushort conversationId, out CtChatter chatter)
        {
            chatter = null;
            if (_chatters.TryGetValue(conversationId, out var token))
            {
                chatter = (CtChatter)token.Reference;
                return true;
            }

            return false;
        }

        public bool TryGetChatterEntry(ushort entryId, out CtChatterEntry entry)
        {
            entry = null;
            var keys = _chatters.GetKeys();
            for (var i = 0; i < keys.Count; i++)
            {
                var chatter = (CtChatter)_chatters[keys[i]].Reference;
                if (chatter.TryGetEntry(entryId, out var subEntry))
                {
                    entry = subEntry;
                    return true;
                }
            }

            return false;
        }
    }
}