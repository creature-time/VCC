﻿
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
    public class CtSequenceManager : CtAbstractSignal
    {
        [SerializeField] private CtBlackboard[] contexts;
        [SerializeField] private CtBlackboardEntryData entryData;

        private CtSequenceNodeBase[] _nodes;
        private int _processing;

        private void Start()
        {
            _nodes = new CtSequenceNodeBase[contexts.Length];
            enabled = false;
        }

        public CtBlackboard GetContext(int identifier) => contexts[identifier];

        public int Process(CtSequenceNodeBase nodeBase)
        {
            int identifier = Array.IndexOf(_nodes, null);
            if (identifier == -1)
            {
#if DEBUG_LOGS
                CtLogger.LogWarning("Sequence Manager", "Failed to find an available sequence.");
#endif
                return -1;
            }

            _NextNode(identifier, nodeBase);
            return identifier;
        }

        private void _NextNode(int identifier, CtSequenceNodeBase nextNode)
        {
            var context = contexts[identifier];
            var prevNode = _nodes[identifier];
            if (prevNode)
            {
#if DEBUG_LOGS
                LogDebug($"Leaving node (identifier={identifier}, prevNode={prevNode})");
#endif
                prevNode.OnExit(context);
            }

            _nodes[identifier] = nextNode;
            if (nextNode)
            {
                this.Emit(ESequencerSignal.Started);

                context.SetupBlackboard(entryData);

                if (_processing == 0)
                    enabled = true;
                _processing++;
            }
            else
            {
                this.Emit(ESequencerSignal.Finished);
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
            for (int i = 0; i < _nodes.Length; i++)
            {
                var node = _nodes[i];
                if (!node)
                    continue;

                var context = contexts[i];
                switch (node.Process(context))
                {
                    case ENodeStatus.Success:
                        _NextNode(i, node.GetNext(context));
                        break;
                    case ENodeStatus.Failure:
                        _NextNode(i, null);
                        break;
                }
            }
        }
    }
}