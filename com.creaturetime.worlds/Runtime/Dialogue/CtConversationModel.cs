
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
        private ushort _entryId = CtConstants.InvalidId;

        public override ushort Identifier
        {
            get => _entryId;
            set
            {
                if (_entry)
                    _entry.OnExitTriggers();

                _entryId = value;
                if (dialogueDatabase.TryGetDialogueEntry(_entryId, out _entry))
                    State = EConversationState.Processing;

                if (_entry)
                    _entry.OnEnterTriggers();

                this.Emit(EConversationModelSignal.EntryChanged);
            }
        }

        public CtDialogueEntry Entry => _entry;

        public ushort ConversationId => _entry ? _entry.ConversationId : CtConstants.InvalidId;

        public string ActorName
        {
            get
            {
                var speaker = _entry.Actor;
                if (!speaker)
                {
#if DEBUG_LOGS
                    LogWarning($"Actor doesn't exist: {_entry.Identifier}");
#endif
                    return null;
                }

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

        public CtDialogueActor Actor => _entry ? _entry.Actor : null;

        public CtDialogueActor Conversant => _entry ? _entry.Conversant : null;

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