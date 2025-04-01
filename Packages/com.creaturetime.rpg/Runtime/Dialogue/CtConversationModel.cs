
using UdonSharp;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace CreatureTime
{
    public enum EConversationModelSignal
    {
        EntryChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtConversationModel : CtAbstractConversationModel
    {
        private CtDialogueEntry _entry;

        // [UdonSynced, FieldChangeCallback(nameof(ConversationIdCallback))] 
        private ushort _entryId = CtConstants.InvalidId;

        public ushort ConversationIdCallback
        {
            get => _entryId;
            set
            {
                _entryId = value;
                dialogueDatabase.TryGetDialogueEntry(_entryId, out _entry);
                if (_entry)
                    State = EConversationState.Processing;

                this.Emit(EConversationModelSignal.EntryChanged);
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

        public string ActorName
        {
            get
            {
                var speaker = _entry.Actor;
                switch (speaker.ActorType)
                {
                    case EActorType.Player:
                        var player = VRCPlayerApi.GetPlayerById(speaker.Identifier);
                        if (player == null)
                            return $"<Unknown Player (id={speaker.Identifier})>";
                        return player.displayName;
                    case EActorType.LocalPlayer:
                        var localPlayer = Networking.LocalPlayer;
                        if (localPlayer == null)
                            return "<Unknown Local Player>";
                        return localPlayer.displayName;
                    default:
                        return speaker.ActorName;
                }
            }
        }

        public string DialogueText
        {
            get
            {
                string result = _entry.DialogueText;

                // TODO: Bake these down in an actual editor. Sigh...

                // TODO: Make this a setting within the database?
                string playerColor = "#00FF00";

                string localPlayer = Networking.LocalPlayer.displayName;
                result = result.Replace("[LocalPlayer]", $"<color=#{playerColor}>{localPlayer}</color>");

                // TODO: Handle [Actor={actorName/actorId}] to <color=#{actorColor}>{actorDisplayName}</color>.
                // TODO: Handle [PlayerId={playerId}] to something like <color=#{playerColor}>{playerName}</color>.

                return result;
            }
        }

        public CtDialogueActor Actor => _entry.Actor;

        public CtDialogueActor Conversant => _entry.Conversant;

        public bool HasResponses => _entry && _entry.Responses.Length > 0;

        public CtDialogueResponse[] Responses
        {
            get
            {
                DataList results = new DataList();
                foreach (var response in _entry.Responses)
                    if (response.IsValid())
                        results.Add(response);

                CtDialogueResponse[] responses = new CtDialogueResponse[results.Count];
                for (int i = 0; i < responses.Length; i++)
                    responses[i] = (CtDialogueResponse)results[i].Reference;
                return responses;
            }
        }

        public void SetChoice(CtDialogueResponse response)
        {
            response.Execute();
            Identifier = response.NextId;
        }

        public override void UpdateConversation()
        {
            if (!IsComplete || Identifier == CtConstants.InvalidId)
                return;

            Identifier = _entry.NextId;
        }
    }
}