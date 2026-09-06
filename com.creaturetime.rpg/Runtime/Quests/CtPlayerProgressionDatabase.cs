
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    public enum EProgressionState
    {
        Locked,
        Available,
        Active,
        Completed,
        Failed
    }

    public enum EPlayerDatabaseSignal
    {
        ProgressionChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerProgressionDatabase : CtAbstractSignal
    {
        [SerializeField] private int maxProgressionCount = 10;

        [UdonSynced] private string[] activeProgression = { };
        [UdonSynced] private ushort[] completedProgression = { };

        public int MaxProgressiveCount => maxProgressionCount;
        public string[] ActiveProgression => activeProgression;
        public ushort[] CompletedProgression => completedProgression;

        public bool TryAddProgression(ushort progressionId, out ulong data)
        {
            data = CtDataBlock.InvalidData;
            if (activeProgression.Length >= maxProgressionCount)
            {
#if DEBUG_LOGS
                LogCritical($"Failed to add quest due to active quest list full (progressionId={progressionId}).");
#endif
                return false;
            }

            foreach (var activeQuest in activeProgression)
            {
                var activeData = CtDataBlock.Deserialize(activeQuest);
                if (!CtDataBlock.IsValid(activeData)) continue;
                if (CtDataBlock.GetProgressionIdentifier(activeData) != progressionId) continue;

#if DEBUG_LOGS
                LogCritical($"Failed to add quest due to already added to list (progressionId={progressionId}).");
#endif
                return false;
            }

            data = CtDataBlock.CreateQuestData(progressionId);
            CtArrayUtils.Insert(ref activeProgression, CtDataBlock.Serialize(data), -1);

            RequestSerialization();
            _OnProgressionChanged();

            return true;
        }

        public bool TryRemoveProgression(ushort progressionId)
        {
            if (activeProgression.Length == 0)
            {
#if DEBUG_LOGS
                LogCritical($"Failed to remove quest due to active quest list empty (progressionId={progressionId}).");
#endif
                return false;
            }

            for (var i = 0; i < activeProgression.Length; ++i)
            {
                var activeData = CtDataBlock.Deserialize(activeProgression[i]);
                if (!CtDataBlock.IsValid(activeData)) continue;
                if (CtDataBlock.GetProgressionIdentifier(activeData) != progressionId) continue;

                CtArrayUtils.Pop(ref activeProgression, i);

                RequestSerialization();
                _OnProgressionChanged();

                return true;
            }

#if DEBUG_LOGS
            LogCritical($"Failed to remove quest due to not being in list (progressionId={progressionId}).");
#endif

            return true;
        }

        public bool TryCompleteProgression(ushort progressionId)
        {
            if (!TryRemoveProgression(progressionId)) return false;

            if (IsCompleted(progressionId))
            {
#if DEBUG_LOGS
                LogCritical($"Failed to complete quest due already being completed (progressionId={progressionId}).");
#endif

                return false;
            }

            CtArrayUtils.Insert(ref completedProgression, progressionId, -1);

            RequestSerialization();
            _OnProgressionChanged();
            return true;
        }

        public bool TryGetProgressionData(ushort progressionId, out ulong data)
        {
#if DEBUG_LOGS
            LogDebug($"Getting quest data (progressionId={progressionId}).");
#endif

            data = CtDataBlock.InvalidData;
            foreach (var activeQuest in activeProgression)
            {
                var activeData = CtDataBlock.Deserialize(activeQuest);
                if (!CtDataBlock.IsValid(activeData)) continue;
                if (CtDataBlock.GetProgressionIdentifier(activeData) != progressionId) continue;

                data = activeData;
                return true;
            }

#if DEBUG_LOGS
            LogDebug($"No progression found (progressionId={progressionId}).");
#endif

            return false;
        }

        public void SetProgressionData(ushort progressionId, ulong data)
        {
#if DEBUG_LOGS
            LogDebug($"Setting quest data (progressionId={progressionId}, data={data:x16}).");
#endif

            for (var i = 0; i < activeProgression.Length; ++i)
            {
                var activeData = CtDataBlock.Deserialize(activeProgression[i]);
                if (!CtDataBlock.IsValid(activeData)) continue;
                if (CtDataBlock.GetProgressionIdentifier(activeData) != progressionId) continue;

                activeProgression[i] = CtDataBlock.Serialize(data);
                RequestSerialization();
                _OnProgressionChanged();
                return;
            }
        }

        public bool IsCompleted(ushort progressionId) => Array.IndexOf(completedProgression, progressionId) != -1;

        private void _OnProgressionChanged()
        {
            this.Emit(EPlayerDatabaseSignal.ProgressionChanged);
        }

        public override void OnDeserialization()
        {
            base.OnDeserialization();

            _OnProgressionChanged();
        }

        // public static void _OnProgressionChanged2<T>(T[] prev, T[] next, out T[] added, out T[] modified,
        //     out T[] removed)
        // {
        //     added = new T[] { };
        //     removed = new T[] { };
        //     modified = new T[] { };
        //
        //     for (var i = 0; i < next.Length; ++i)
        //     {
        //         var value = next[i];
        //         if (!prev.Contains(value))
        //         {
        //             CtPlayerProgressionDatabase.Insert(ref added, i, -1);
        //         }
        //         // TODO: Can we map these to only modify what changed in the list?
        //         else if (prev[i] != next[i])
        //         {
        //             CtPlayerProgressionDatabase.Insert(ref modified, i, -1);
        //         }
        //     }
        //
        //     for (var i = 0; i < prev.Length; ++i)
        //     {
        //         var value = prev[i];
        //         if (!next.Contains(value))
        //         {
        //             CtPlayerProgressionDatabase.Insert(ref removed, i, -1);
        //         }
        //     }
        // }
    }
}