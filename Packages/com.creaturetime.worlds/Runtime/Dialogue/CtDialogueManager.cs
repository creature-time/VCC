
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EDialogueManagerSignal
    {
        ConversationChanged,
        ChatterChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtDialogueManager : CtSingleton
    {
        [SerializeField] private CtDialogueDatabase dialogueDatabase;
        [SerializeField] private CtConversationModel conversationModel;
        [SerializeField] private CtChatterModel[] chatterModels;

        public CtDialogueDatabase DialogueDatabase => dialogueDatabase;
        public CtConversationModel ConversationModel => conversationModel;
        public CtChatterModel[] ChatterModels => chatterModels;

        private CtChatterModel[] _activeChatterModels;

        public override void Init()
        {
            _activeChatterModels = new CtChatterModel[chatterModels.Length];

            conversationModel.Connect(EConversationModelSignal.EntryChanged, this, nameof(_OnConversationChanged));
            foreach (var model in chatterModels)
                model.Connect(EChatterModelSignal.EntryChanged, this, nameof(_OnChatterChanged));
        }

        public void _OnConversationChanged()
        {
            SetArgs.Add((CtConversationModel)Sender);
            this.Emit(EDialogueManagerSignal.ConversationChanged);
        }

        public void _OnChatterChanged()
        {
            var model = (CtChatterModel)Sender;

            SetArgs.Add(model);
            this.Emit(EDialogueManagerSignal.ChatterChanged);

            if (model.Identifier == CtConstants.InvalidId)
                _activeChatterModels[Array.IndexOf(_activeChatterModels, model)] = null;
        }

        public void StartConversation(ushort conversationId)
        {
            if (conversationModel.Identifier != CtConstants.InvalidId)
                StopConversation();

            if (!dialogueDatabase.TryGetConversation(conversationId, out var conversation))
            {
                return;
            }

            conversationModel.Identifier = conversation.StartEntryId;
        }

        public void StartConversationWithActor(CtDialogueActor actor)
        {
            foreach (var conversation in dialogueDatabase.Conversations)
            {
                if (!dialogueDatabase.TryGetDialogueEntry(conversation.StartEntryId, out var dialogueEntry)) continue;
                if (dialogueEntry.Actor != actor) continue;
                StartConversation(conversation.Identifier);
                return;
            }

#if DEBUG_LOGS
            LogCritical($"Failed to find actor's conversation (actor={actor}).");
#endif
        }

        public void StopConversation()
        {
            conversationModel.Identifier = CtConstants.InvalidId;
        }

        public void StartChatter(ushort chatterId)
        {
            int index = Array.IndexOf(_activeChatterModels, null);
            if (index == -1)
            {
#if DEBUG_LOGS
                LogWarning("Could not create another chatter.");
#endif
                return;
            }

            if (!dialogueDatabase.TryGetChatter(chatterId, out var chatter))
            {
                return;
            }

            var chatterModel = chatterModels[index];
            _activeChatterModels[index] = chatterModel;
            chatterModel.Identifier = chatter.StartEntryId;
        }

        private void _StopChatter(int index)
        {
            chatterModels[index].Identifier = CtConstants.InvalidId;
            _activeChatterModels[index] = null;
        }

        public void StopAllConversations()
        {
            StopConversation();
            for (int i = 0; i < chatterModels.Length; i++)
                _StopChatter(i);
        }

        private void Update()
        {
            conversationModel.UpdateConversation();
            foreach (var model in _activeChatterModels)
                if (model)
                    model.UpdateConversation();
        }
    }
}