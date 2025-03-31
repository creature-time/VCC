
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtDialogueManager : CtAbstractSignal
    {
        [SerializeField] private CtDialogueDatabase dialogueDatabase;
        [SerializeField] private CtConversationModel conversationModel;
        [SerializeField] private CtConversationModel[] chatterModel;

        public void StartConversation(ushort conversationId)
        {
            if (conversationModel.ConversationId != CtConstants.InvalidId)
                StopConversation();
            conversationModel.ConversationId = conversationId;
        }

        public void StopConversation()
        {
            conversationModel.ConversationId = CtConstants.InvalidId;
        }

        public void StartChatter(ushort conversationId)
        {
            int index = Array.IndexOf(chatterModel, null);
            if (index == -1)
            {
                LogWarning("Could not create another chatter.");
                return;
            }

            chatterModel[index].ConversationId = conversationId;
        }

        private void _StopChatter(int index)
        {
            chatterModel[index].ConversationId = CtConstants.InvalidId;
        }

        public void StopAllConversations()
        {
            StopConversation();
            for (int i = 0; i < chatterModel.Length; i++)
                _StopChatter(i);
        }

        private void Update()
        {
            if (conversationModel)
                conversationModel.PollConversation();
        }
    }
}