
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtConversation : UdonSharpBehaviour
    {
        [SerializeField] private ushort identifier = CtConstants.InvalidId;
        [SerializeField] private CtDialogueEntry[] entries;
        [SerializeField] private ushort startEntryId = CtConstants.InvalidId;

        public ushort Identifier => identifier;
        public ushort StartEntryId => startEntryId;

        private DataDictionary _entries = new DataDictionary();

        private void Start()
        {
            foreach (var entry in entries)
                _entries.Add(entry.Identifier, entry);
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

        public CtDialogueEntry GetFirstDialogueEntry()
        {
            if (!TryGetEntry(startEntryId, out var entry))
            {
                return null;
            }

            return entry;
        }
    }
}