
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtConversation : UdonSharpBehaviour
    {
        [SerializeField] private ushort identifier = CtConstants.InvalidId;
        [SerializeField] private ushort startEntryId = CtConstants.InvalidId;

        [SerializeField] private CtDialogueEntry[] entries;

        public ushort Identifier => identifier;
        public ushort StartEntryId => startEntryId;

        private DataDictionary _entries = new DataDictionary();
        private DataList _conversants = new DataList();

        public DataList Conversants => _conversants;

        private void Start()
        {
            foreach (var entry in entries)
            {
                _entries.Add(entry.Identifier, entry);
                if (entry.Actor && !_conversants.Contains(entry.Actor))
                    _conversants.Add(entry.Actor);
                if (entry.Conversant && !_conversants.Contains(entry.Conversant))
                    _conversants.Add(entry.Conversant);
            }
        }

        public bool TryGetEntry(ushort entryId, out CtDialogueEntry entry)
        {
            entry = null;
            if (_entries.TryGetValue(entryId, out var token))
            {
                entry = (CtDialogueEntry)token.Reference;
                return true;
            }

            return false;
        }
    }
}