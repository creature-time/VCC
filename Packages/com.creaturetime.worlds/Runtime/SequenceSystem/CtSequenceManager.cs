
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum ESequencerSignal
    {
        Started,
        Finished
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSequenceManager : CtAbstractSignal<ESequencerSignal>
    {
        [SerializeField] private CtSequenceNodeBase[] nodes;
        [SerializeField] private CtBlackboard[] contexts;
        [SerializeField] private CtBlackboardEntryData entryData;

        [SerializeField] private CtSequenceNodeBase[] _states;

        private int _processing;

        private void Start()
        {
            _states = new CtSequenceNodeBase[contexts.Length];
            foreach (var context in contexts)
                context.SetupBlackboard(entryData);

            enabled = false;
        }

        public CtBlackboard GetContext(int identifier) => contexts[identifier];

        public int Process(CtSequenceNodeBase nodeBase)
        {
            int index = Array.IndexOf(_states, null);
            if (index == -1)
            {
                CtLogger.LogWarning("Sequence Manager", "Failed to find an available sequence.");
                return -1;
            }

            _NextNode(index, nodeBase);
            return index;
        }

        private void _NextNode(int identifier, CtSequenceNodeBase nextState)
        {
            var context = contexts[identifier];
            var prevState = _states[identifier];
            if (prevState)
            {
                LogDebug($"Leaving state (identifier={identifier}, prevState={prevState})");
                prevState.OnExit(context);
            }
            else if (nextState)
            {
                if (_processing == 0)
                    enabled = true;
                _processing++;
            }

            _states[identifier] = nextState;
            if (nextState)
            {
                LogDebug($"Entering state (identifier={identifier}, prevState={prevState})");
                nextState.OnEnter(context);

                enabled = true;
            }
            else
            {
                context.Clear();

                LogDebug($"Stopping (identifier={identifier})");

                _processing--;
                if (_processing == 0)
                    enabled = false;
            }
        }

        private void Update()
        {
            for (int i = 0; i < _states.Length; i++)
            {
                var context = contexts[i];
                var state = _states[i];
                if (!state)
                    continue;
                state.Execute(context);
                if (!state.IsComplete(context))
                    continue;
                _NextNode(i, state.GetNext(context));
            }
        }
    }
}