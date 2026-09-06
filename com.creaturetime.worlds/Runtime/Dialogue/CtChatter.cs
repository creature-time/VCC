
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtChatter : UdonSharpBehaviour
    {
        [SerializeField] private ushort identifier = CtConstants.InvalidId;
        [SerializeField] private ushort startEntryId = CtConstants.InvalidId;

        [SerializeField] private CtChatterEntry[] entries;

        public ushort Identifier => identifier;
        public ushort StartEntryId => startEntryId;

        private DataDictionary _entries = new DataDictionary();

        private void Start()
        {
            foreach (var entry in entries)
                _entries.Add(entry.Identifier, entry);
        }

        public bool TryGetEntry(ushort entryId, out CtChatterEntry entry)
        {
            entry = null;
            if (_entries.TryGetValue(entryId, out var token))
            {
                entry = (CtChatterEntry)token.Reference;
                return true;
            }

            return false;
        }
    }
}