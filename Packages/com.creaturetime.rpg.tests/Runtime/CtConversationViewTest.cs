
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtConversationViewTest : CtAbstractSignal
    {
        [SerializeField] private CtConversationModel conversationModel;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private GameObject responsePrefab;

        [SerializeField] private CtChatterModel chatterModel;
        [SerializeField] private TMP_Text chatterText;

        private DataList _prefabs = new DataList();

        private void Start()
        {
            responsePrefab.SetActive(false);

            conversationModel.Connect(EConversationModelSignal.EntryChanged, this, nameof(_OnDialogueChanged));

            chatterModel.Connect(EChatterModelSignal.EntryChanged, this, nameof(_OnChatterChanged));
        }

        public void _OnDialogueChanged()
        {
            _ClearResponses();

            string text;
            if (conversationModel.Identifier != CtConstants.InvalidId)
            {
                text = $"[<b>{conversationModel.ActorName}</b>] {conversationModel.DialogueText}";

                if (conversationModel.HasResponses)
                {
                    foreach (var response in conversationModel.Responses)
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
                    conversationModel.SendCustomEventDelayedSeconds("SetComplete", 3);
                }
            }
            else
            {
                text = "[END]";
            }

            dialogueText.text = text;
        }

        public void _OnChatterChanged()
        {
            if (chatterModel.Identifier != CtConstants.InvalidId)
                chatterText.text = chatterModel.DialogueText;
            else
                chatterText.text = "[END]";
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