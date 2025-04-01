
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EChatterModelSignal
    {
        EntryChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtChatterModel : CtAbstractConversationModel
    {
        private CtChatterEntry _entry;
        private float _timer;

        // [UdonSynced, FieldChangeCallback(nameof(ConversationIdCallback))] 
        private ushort _entryId = CtConstants.InvalidId;

        public ushort ConversationIdCallback
        {
            get => _entryId;
            set
            {
                _entryId = value;
                dialogueDatabase.TryGetChatterEntry(_entryId, out _entry);

                if (_entry)
                {
                    _timer = _entry.Duration;
                    if (_timer <= 0)
                        _timer = Mathf.Ceil(_entry.DialogueText.Length / 4f);
                    State = EConversationState.Processing;
                }
                else
                {
                    _timer = 0;
                }

                this.Emit(EChatterModelSignal.EntryChanged);
            }
        }

        public override ushort Identifier
        {
            get => ConversationIdCallback;
            set
            {
                ConversationIdCallback = value;
                // TODO: Can we make this toggleable or externally controlled?
                // RequestSerialization();
            }
        }

        public string DialogueText => _entry.DialogueText;

        public override void UpdateConversation()
        {
            if (IsComplete || Identifier == CtConstants.InvalidId)
                return;

            _timer -= Time.deltaTime;
            if (_timer > 0)
                return;

            State = EConversationState.Completed;
            Identifier = _entry.NextId;
        }
    }
}