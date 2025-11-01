
using System;
using System.Collections.Generic;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [CreateAssetMenu(fileName = "DialogueGraphAsset", menuName = "CreatureTime/DialogueGraph/Asset")]
    public class CtDialogueGraphAsset : ScriptableObject
    {
        [SerializeField] private CtGraph graph;

        public List<CtGraphNode> Nodes => graph.Nodes;
        public List<CtGraphEdge> Edges => graph.Edges;

        private Dictionary<string, CtDialogueNodeBase> _nodeLookup = new Dictionary<string, CtDialogueNodeBase>();
        private Dictionary<string, Dictionary<string, CtDialogueNodeBase>> _inputLookup = new Dictionary<string, Dictionary<string, CtDialogueNodeBase>>();
        private Dictionary<string, Dictionary<string, CtDialogueNodeBase>> _outputLookup = new Dictionary<string, Dictionary<string, CtDialogueNodeBase>>();

        private ushort _identifierGen;
        private ushort _conversationId;

        private CtDialogueDatabase _dialogueDatabase;
        private GameObject _conversationGameObject;
        private Transform _conversationsGroup;
        private CtConversation _conversation;
        private GameObject _gameObject;
        private Stack<CtDialogueEntry> _dialogueEntry = new Stack<CtDialogueEntry>();
        private Dictionary<string, CtDialogueEntry> _visitedNodes = new Dictionary<string, CtDialogueEntry>();
        private CtDialogueResponse _dialogueResponse;

        public void Init()
        {
            _identifierGen = 0;
            _conversationId = 0;

            _dialogueDatabase = FindObjectOfType<CtDialogueDatabase>(true);
            var so = new SerializedObject(_dialogueDatabase);

            var conversationsProperty = so.FindProperty("conversations");
            conversationsProperty.arraySize = 0;

            so.ApplyModifiedProperties();

            _conversationsGroup = _dialogueDatabase.transform.Find("Conversations");
            for (int i = _conversationsGroup.childCount - 1; i >= 0; i--)
                DestroyImmediate(_conversationsGroup.GetChild(i).gameObject);
        }

        public bool TryGetNodeFromInput(string nodeId, string portId, out CtDialogueNodeBase node)
        {
            node = null;
            if (!_inputLookup.TryGetValue(nodeId, out var lookup)) return false;
            return lookup.TryGetValue(portId, out node);
        }

        public bool TryGetNodeFromOutput(string nodeId, string portId, out CtDialogueNodeBase node)
        {
            node = null;
            if (!_outputLookup.TryGetValue(nodeId, out var lookup)) return false;
            return lookup.TryGetValue(portId, out node);
        }

        public void Process()
        {
            _nodeLookup.Clear();
            _inputLookup.Clear();
            _outputLookup.Clear();

            var startNodes = new List<CtStartNode>();

            foreach (var node in graph.Nodes)
            {
                _nodeLookup.Add(node.Guid, (CtDialogueNodeBase)node);
                if (node is CtStartNode startNode)
                    startNodes.Add(startNode);
            }

            foreach (var edge in graph.Edges)
            {
                if (TryGetNode(edge.OutputId, out var node))
                {
                    if (!_inputLookup.TryGetValue(edge.InputId, out var lookup))
                    {
                        _inputLookup.Add(edge.InputId, lookup = new Dictionary<string, CtDialogueNodeBase>());
                    }

                    lookup.TryAdd(edge.InputPortId, node);
                }

                if (TryGetNode(edge.InputId, out node))
                {
                    if (!_outputLookup.TryGetValue(edge.OutputId, out var lookup))
                    {
                        _outputLookup.Add(edge.OutputId, lookup = new Dictionary<string, CtDialogueNodeBase>());
                    }

                    lookup.TryAdd(edge.OutputPortId, node);
                }
            }

            foreach (var startNode in startNodes)
            {
                startNode.Process(this);
                CtSingletonEditor.AssignSingletons(CtSingletonEditor.GetCurrentSingletonTypes(), _conversationGameObject);
            }
        }

        public bool TryGetNode(string guid, out CtDialogueNodeBase node)
        {
            return _nodeLookup.TryGetValue(guid, out node);
        }

        public CreatureTime.CtDialogueActor FindActor(ushort actorId)
        {
            var dialogueActors = FindObjectsOfType<CreatureTime.CtDialogueActor>(true);
            foreach (var dialogueActor in dialogueActors)
            {
                if (dialogueActor.Identifier == actorId)
                    return dialogueActor;
            }

            return null;
        }

        private T AddUdonSharpComponentWithUdonBehavior<T>(GameObject gameObject)
            where T : UdonSharpBehaviour
        {
            return (T)AddUdonSharpComponentWithUdonBehavior(gameObject, typeof(T));
        }

        private UdonSharpBehaviour AddUdonSharpComponentWithUdonBehavior(GameObject gameObject, Type type)
        {
            return gameObject.AddUdonSharpComponent(type);
        }

        public void CreateConversation()
        {
            _conversationId++;
            _conversationGameObject = new GameObject($"Conversation {_conversationId}");

            _conversationGameObject.transform.parent = _conversationsGroup;

            _conversation = AddUdonSharpComponentWithUdonBehavior<CtConversation>(_conversationGameObject);
            var so = new SerializedObject(_conversation);

            so.FindProperty("identifier").uintValue = _conversationId;
            so.FindProperty("startEntryId").uintValue = _identifierGen;

            so.ApplyModifiedProperties();

            so = new SerializedObject(_dialogueDatabase);

            var conversationsProperty = so.FindProperty("conversations");
            conversationsProperty.InsertArrayElementAtIndex(conversationsProperty.arraySize);
            var conversationProperty = conversationsProperty.GetArrayElementAtIndex(conversationsProperty.arraySize - 1);;
            conversationProperty.objectReferenceValue = _conversation;

            so.ApplyModifiedProperties();
        }

        public bool CreateDialogue(string guid, string dialogue, ushort actorId, ushort conversantId)
        {
            bool result = _visitedNodes.TryGetValue(guid, out var entry);
            if (!result)
            {
                var identifier = _identifierGen++;
                if (_dialogueResponse)
                {
                    var dialogueResponseSo = new SerializedObject(_dialogueResponse);
                    dialogueResponseSo.FindProperty("nextId").uintValue = identifier;
                    dialogueResponseSo.ApplyModifiedProperties();
                    _dialogueResponse = null;
                }
                else
                {
                    if (_dialogueEntry.Count > 0)
                    {
                        var so2 = new SerializedObject(_dialogueEntry.Peek());
                        so2.FindProperty("nextId").uintValue = identifier;
                        so2.ApplyModifiedProperties();
                    }
                }

                _gameObject = new GameObject(dialogue);
                _gameObject.transform.parent = _conversationGameObject.transform;

                entry = AddUdonSharpComponentWithUdonBehavior<CtDialogueEntry>(_gameObject);
                var so = new SerializedObject(entry);

                so.FindProperty("identifier").uintValue = identifier;
                so.FindProperty("conversationId").uintValue = _conversationId;
                so.FindProperty("dialogueText").stringValue = dialogue;

                so.FindProperty("actor").objectReferenceValue = FindActor(actorId);
                so.FindProperty("conversant").objectReferenceValue = FindActor(conversantId);

                so.ApplyModifiedProperties();

                so = new SerializedObject(_conversation);

                var entriesProperty = so.FindProperty("entries");
                entriesProperty.InsertArrayElementAtIndex(entriesProperty.arraySize);
                var entryProperty = entriesProperty.GetArrayElementAtIndex(entriesProperty.arraySize - 1);
                entryProperty.objectReferenceValue = entry;

                so.ApplyModifiedProperties();

                _visitedNodes.Add(guid, entry);
            }

            if (_dialogueEntry.Count > 0)
            {
                var so2 = new SerializedObject(_dialogueEntry.Peek());
                so2.FindProperty("nextId").uintValue = entry.Identifier;
                so2.ApplyModifiedProperties();
            }

            _dialogueEntry.Push(entry);

            return !result;
        }

        public void PopDialogue()
        {
            _dialogueEntry.Pop();
        }

        public void CreateResponse(string dialogue, EDialogueChoiceType choiceType)
        {
            _dialogueResponse = AddUdonSharpComponentWithUdonBehavior<CtDialogueResponse>(_dialogueEntry.Peek().gameObject);
            var so = new SerializedObject(_dialogueResponse);
            so.FindProperty("choiceType").enumValueIndex = Convert.ToInt32(choiceType);
            so.FindProperty("displayText").stringValue = dialogue;
            so.ApplyModifiedProperties();

            so = new SerializedObject(_dialogueEntry.Peek());
            var responses = so.FindProperty("responses");
            responses.InsertArrayElementAtIndex(responses.arraySize);
            var response = responses.GetArrayElementAtIndex(responses.arraySize - 1);
            response.objectReferenceValue = _dialogueResponse;
            so.ApplyModifiedProperties();
        }

        public T CreateCondition<T>()
            where T : CtResponseCondition
        {
            var condition = AddUdonSharpComponentWithUdonBehavior<T>(_dialogueEntry.Peek().gameObject);

            var so = new SerializedObject(_dialogueResponse);
            var responses = so.FindProperty("conditions");
            responses.InsertArrayElementAtIndex(responses.arraySize);
            var response = responses.GetArrayElementAtIndex(responses.arraySize - 1);
            response.objectReferenceValue = condition;
            so.ApplyModifiedProperties();
            return condition;
        }

        public T CreateConsequence<T>()
            where T : CtResponseConsequence
        {
            var condition = AddUdonSharpComponentWithUdonBehavior<T>(_dialogueEntry.Peek().gameObject);

            var so = new SerializedObject(_dialogueResponse);
            var responses = so.FindProperty("consequences");
            responses.InsertArrayElementAtIndex(responses.arraySize);
            var response = responses.GetArrayElementAtIndex(responses.arraySize - 1);
            response.objectReferenceValue = condition;
            so.ApplyModifiedProperties();
            return condition;
        }
    }
}