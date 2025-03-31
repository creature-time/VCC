
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtConversationViewTest : CtAbstractSignal
    {
        [SerializeField] private CtConversationModel conversationModel;
        [SerializeField] private GameObject responsePrefab;

        private DataList _prefabs = new DataList();

        private void Start()
        {
            responsePrefab.SetActive(false);

            conversationModel.Connect(EConversationModelSignal.ConversationChanged, this, nameof(_OnConversationChanged));
            conversationModel.Connect(EConversationModelSignal.EntryChanged, this, nameof(_OnEntryChanged));
            conversationModel.Connect(EConversationModelSignal.StateChanged, this, nameof(_OnStateChanged));
        }

        public void _OnConversationChanged()
        {
            Debug.Log($"_OnConversationChanged {conversationModel.ConversationId}");
        }

        public void _OnEntryChanged()
        {
            _ClearResponses();

            if (conversationModel.Entry)
            {
                Debug.Log($"_OnEntryChanged {conversationModel.Entry.DialogueText}");

                if (conversationModel.HasResponses)
                {
                    foreach (var response in conversationModel.Entry.Responses)
                    {
                        var responseView = Instantiate(responsePrefab, responsePrefab.transform.parent)
                            .GetComponent<CtResponseViewTest>();
                        responseView.gameObject.SetActive(true);
                        responseView.Response = response;
                        _prefabs.Add(responseView);
                    }
                }
                else
                {
                    conversationModel.SetComplete();
                }
            }
        }

        public void _OnStateChanged()
        {
            Debug.Log($"_OnStateChanged {conversationModel.State}");
        }

        private void _ClearResponses()
        {
            for (int i = 0; i < _prefabs.Count; i++)
            {
                var responseView = (CtResponseViewTest)_prefabs[i].Reference;
                Destroy(responseView.gameObject);
            }
            _prefabs.Clear();
        }

        public void CommitResponse(CtDialogueResponse response)
        {
            conversationModel.SetChoice(response);
            _ClearResponses();
        }
    }
}