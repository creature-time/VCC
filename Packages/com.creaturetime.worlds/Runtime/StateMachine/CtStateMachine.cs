
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EStateMachineSignal
    {
        Started,
        Finished
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CtStateMachine : CtAbstractSignal<EStateMachineSignal>
    {
        [SerializeField] private CtBlackboard[] contexts;
        [SerializeField] private CtBlackboardEntryData entryData;
        
        private CtStateBase[] _states;
        private int _processing;

        private void Start()
        {
            _states = new CtStateBase[contexts.Length];
            enabled = false;
        }

        public CtBlackboard GetContext(int identifier) => contexts[identifier];

        public int Process(CtStateBase nodeBase)
        {
            int identifier = Array.IndexOf(_states, null);
            if (identifier == -1)
            {
                LogWarning("Failed to find an available sequence.");
                return -1;
            }

            _NextNode(identifier, nodeBase);
            return identifier;
        }

        private void _NextNode(int identifier, CtStateBase nextState)
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
                Emit(EStateMachineSignal.Started);

                context.SetupBlackboard(entryData);

                if (_processing == 0)
                    enabled = true;
                _processing++;
            }

            _states[identifier] = nextState;
            if (nextState)
            {
                LogDebug($"Entering state (identifier={identifier}, prevState={prevState})");
                nextState.OnEnter(context);
            }
            else
            {
                Emit(EStateMachineSignal.Finished);
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
                var state = _states[i];
                if (!state)
                    continue;

                var context = contexts[i];
                switch (state.Process(context))
                {
                    case ENodeStatus.Success:
                        _NextNode(i, state.GetNext(context));
                        break;
                    case ENodeStatus.Failure:
                        _NextNode(i, null);
                        break;
                }
            }
        }
    }
}