﻿
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
    public class CtStateMachine : CtAbstractSignal
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
#if DEBUG_LOGS
                LogWarning("Failed to find an available sequence.");
#endif
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
#if DEBUG_LOGS
                LogDebug($"Leaving state (identifier={identifier}, prevState={prevState})");
#endif
                prevState.OnExit(context);
            }
            else if (nextState)
            {
                this.Emit(EStateMachineSignal.Started);

                context.SetupBlackboard(entryData);

                if (_processing == 0)
                    enabled = true;
                _processing++;
            }

            _states[identifier] = nextState;
            if (nextState)
            {
#if DEBUG_LOGS
                LogDebug($"Entering state (identifier={identifier}, prevState={prevState})");
#endif
                nextState.OnEnter(context);
            }
            else
            {
                this.Emit(EStateMachineSignal.Finished);
                context.Clear();

#if DEBUG_LOGS
                LogDebug($"Stopping (identifier={identifier})");
#endif

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