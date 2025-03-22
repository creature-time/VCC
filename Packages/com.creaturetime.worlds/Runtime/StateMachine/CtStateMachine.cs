
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum EFiniteStateMachineSignal
    {
        Started,
        Finished
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtStateMachine : CtAbstractSignal<EFiniteStateMachineSignal>
    {
        [SerializeField] private CtStateBase[] states;
        [SerializeField] private CtBlackboard context;

        private CtStateBase _stateBase;

        public void Process(CtStateBase stateBase)
        {
            _NextNode(stateBase);
        }

        private void _NextNode(CtStateBase nextStateBase)
        {
            if (_stateBase)
            {
                _stateBase.OnExit(context);
            }
            else if (nextStateBase)
            {
                LogDebug("State machine started.");
                Emit(EFiniteStateMachineSignal.Started);
            }

            _stateBase = nextStateBase;
            if (_stateBase)
            {
                _stateBase.OnEnter(context);
                enabled = true;
            }
            else
            {
                enabled = false;
                Emit(EFiniteStateMachineSignal.Finished);
                LogDebug("State machine finished.");
            }
        }

        private void Update()
        {
            _stateBase.Execute(context);
            if (!_stateBase.IsComplete(context)) return;
            _NextNode(_stateBase.GetNext(context));
        }
    }
}