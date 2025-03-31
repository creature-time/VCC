
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public enum EConversationModelSignal
    {
        ConversationChanged,
        EntryChanged,
        StateChanged,
    }

    public enum EConversationState
    {
        Processing,
        Interrupted,
        Completed
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtConversationModel : CtAbstractSignal
    {
        [SerializeField] private CtDialogueDatabase dialogueDatabase;
        [SerializeField] private CtBlackboard blackboard;

        [UdonSynced, FieldChangeCallback(nameof(ConversationIdCallback))] private ushort _conversationId = CtConstants.InvalidId;

        public ushort ConversationIdCallback
        {
            get => _conversationId;
            set
            {
                if (_conversation)
                    EntryId = CtConstants.InvalidId;

                _conversation = dialogueDatabase.TryGetConversation(value, out var conversation) ? conversation : null;
                this.Emit(EConversationModelSignal.ConversationChanged);

                if (_conversation)
                    EntryId = _conversation.GetFirstDialogueEntry().Identifier;
            }
        }

        public ushort ConversationId
        {
            get => ConversationIdCallback;
            set
            {
                ConversationIdCallback = value;
                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(EntryIdCallback))] private ushort _entryId = CtConstants.InvalidId;

        public ushort EntryIdCallback
        {
            get => _conversationId;
            set
            {
                _entryId = value;
                _conversation.TryGetEntry(_entryId, out _entry);
                this.Emit(EConversationModelSignal.EntryChanged);

                if (_entry)
                    State = EConversationState.Processing;
            }
        }

        public ushort EntryId
        {
            get => EntryIdCallback;
            set
            {
                EntryIdCallback = value;
                RequestSerialization();
            }
        }

        private CtConversation _conversation;
        private CtDialogueEntry _entry;
        private EConversationState _state = EConversationState.Completed;

        public CtBlackboard Blackboard => blackboard;

        public CtDialogueEntry Entry => _entry;

        public EConversationState State
        {
            get => _state;
            set
            {
                _state = value;
                this.Emit(EConversationModelSignal.StateChanged);
            }
        }

        public bool HasResponses => _entry && _entry.Responses.Length > 0;

        public CtDialogueResponse[] Responses
        {
            get
            {
                DataList results = new DataList();
                foreach (var response in _entry.Responses)
                    if (response.IsValid(blackboard))
                        results.Add(response);

                CtDialogueResponse[] responses = new CtDialogueResponse[results.Count];
                for (int i = 0; i < responses.Length; i++)
                    responses[i] = (CtDialogueResponse)results[i].Reference;
                return responses;
            }
        }

        public bool IsComplete => State != EConversationState.Processing;

        public void SetChoice(CtDialogueResponse response)
        {
            response.Execute(blackboard);
            EntryId = response.NextId;
        }

        public void Interrupt()
        {
            State = EConversationState.Interrupted;
        }

        public void SetComplete() => State = EConversationState.Completed;

        public void PollConversation()
        {
            if (_conversation && IsComplete)
            {
                EntryId = _entry.NextId;
            }
        }
    }
}
