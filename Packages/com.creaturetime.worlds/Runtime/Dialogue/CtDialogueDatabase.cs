
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
        [SerializeField] private CtChatter[] chatters;

        public CtDialogueActor[] Actors => actors;
        public CtConversation[] Conversations => conversations;
        public CtChatter[] Chatters => chatters;

        private DataDictionary _actors = new DataDictionary();
        private DataDictionary _startEntryIds = new DataDictionary();
        private DataDictionary _conversations = new DataDictionary();
        private DataDictionary _chatters = new DataDictionary();

        private CtDialogueEntry[] _dialogueEntries;

        public CtDialogueEntry[] DialogueEntries => _dialogueEntries;

        private void Start()
        {
            foreach (var actor in actors)
                _actors.Add(actor.Identifier, actor);

            _dialogueEntries = GetComponentsInChildren<CtDialogueEntry>(true);

            foreach (var conversation in conversations)
            {
                _conversations.Add(conversation.Identifier, conversation);
                if (!TryGetDialogueEntry(conversation.StartEntryId, out var dialogueEntry)) continue;
                _startEntryIds.Add(dialogueEntry.Actor, conversation.StartEntryId);
            }

            foreach (var chatter in chatters)
                _chatters.Add(chatter.Identifier, chatter);
        }

        public bool TryGetStartDialogue(CtDialogueActor actor, out ushort startEntryId)
        {
            startEntryId = CtConstants.InvalidId;
            if (_startEntryIds.TryGetValue(actor, out var token))
            {
                startEntryId = token.UShort;
                return true;
            }

            return false;
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