
using TMPro;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtResponseViewTest : UdonSharpBehaviour
    {
        [SerializeField] private CtConversationViewTest view;
        [SerializeField] private TMP_Text text;

        private CtDialogueResponse _response;

        public CtDialogueResponse Response
        {
            set
            {
                _response = value;
                text.text = _response.DisplayText;
            }
        }

        public void _CommitResponse()
        {
            view.CommitResponse(_response);
        }
    }
}