
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
        [SerializeField] private ushort conversationId = CtConstants.InvalidId;
        [SerializeField] private CtGraph graph;

        public CtGraph Graph => graph;

        private Dictionary<string, CtDialogueNodeBase> _nodeLookup = new Dictionary<string, CtDialogueNodeBase>();
        private Dictionary<string, Dictionary<string, CtDialogueNodeBase>> _inputLookup = new Dictionary<string, Dictionary<string, CtDialogueNodeBase>>();
        private Dictionary<string, Dictionary<string, CtDialogueNodeBase>> _outputLookup = new Dictionary<string, Dictionary<string, CtDialogueNodeBase>>();

        private ushort _identifierGen;
        public ushort IdentifierGen => _identifierGen;

        private CtDialogueDatabase _dialogueDatabase;
        private GameObject _conversationGameObject;
        private Transform _conversationsGroup;
        private Transform _triggersGroup;
        private CtConversation _conversation;
        private GameObject _gameObject;
        private Stack<CtDialogueEntry> _dialogueEntry = new Stack<CtDialogueEntry>();
        private Dictionary<string, CtDialogueEntry> _visitedNodes = new Dictionary<string, CtDialogueEntry>();
        private CtDialogueResponse _dialogueResponse;
        private string _triggerArrayProperty;

        public static void GenerateDialogue()
        {
            var dialogueDatabase = FindObjectOfType<CtDialogueDatabase>(true);
            var so = new SerializedObject(dialogueDatabase);
            var conversationsProperty = so.FindProperty("conversations");
            conversationsProperty.arraySize = 0;
            so.ApplyModifiedProperties();

            var conversationsGroup = dialogueDatabase.transform.Find("Conversations");
            for (int i = conversationsGroup.childCount - 1; i >= 0; i--)
                DestroyImmediate(conversationsGroup.GetChild(i).gameObject);

            var assets = new List<CtDialogueGraphAsset>();

            foreach (var assetId in AssetDatabase.FindAssets($"t:{nameof(CtDialogueGraphAsset)}"))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetId);
                if (string.IsNullOrEmpty(assetPath)) continue;

                var asset = AssetDatabase.LoadAssetAtPath<CtDialogueGraphAsset>(assetPath);
                if (asset)
                    assets.Add(asset);
            }

            assets.Sort((a, b) => a.conversationId.CompareTo(b.conversationId));

            ushort identifierGen = 0;
            foreach (var asset in assets)
            {
                asset.Init(identifierGen);
                asset.Process();
                identifierGen = asset.IdentifierGen;
            }
        }

        public void Init(ushort entryStartingId)
        {
            _identifierGen = entryStartingId;
            _dialogueDatabase = FindObjectOfType<CtDialogueDatabase>(true);
            _conversationsGroup = _dialogueDatabase.transform.Find("Conversations");
            _triggersGroup = _dialogueDatabase.transform.Find("Triggers");
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
            _visitedNodes.Clear();

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

            if (startNodes.Count > 0)
            {
                startNodes[0].Process(this);
                CtSingletonEditor.AssignSingletons(CtSingletonEditor.GetCurrentSingletonTypes(),
                    _conversationGameObject);
            }
        }
        
        public bool TryGetNode(string guid, out CtDialogueNodeBase node)
        {
            return _nodeLookup.TryGetValue(guid, out node);
        }

        public CtProfessionDef FindProfessionDef(ushort professionId)
        {
            var definitions = FindObjectsOfType<CtProfessionDef>(true);
            foreach (var definition in definitions)
            {
                if (definition.Identifier == professionId)
                    return definition;
            }
        
            return null;
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
            _conversationGameObject = new GameObject($"{conversationId:00000}-{name}");

            _conversationGameObject.transform.parent = _conversationsGroup;

            _conversation = AddUdonSharpComponentWithUdonBehavior<CtConversation>(_conversationGameObject);
            var so = new SerializedObject(_conversation);

            so.FindProperty("identifier").uintValue = conversationId;
            so.FindProperty("startEntryId").uintValue = _identifierGen;

            so.ApplyModifiedProperties();
        
            so = new SerializedObject(_dialogueDatabase);
        
            var conversationsProperty = so.FindProperty("conversations");
            conversationsProperty.InsertArrayElementAtIndex(conversationsProperty.arraySize);
            var conversationProperty = conversationsProperty.GetArrayElementAtIndex(conversationsProperty.arraySize - 1);;
            conversationProperty.objectReferenceValue = _conversation;
        
            so.ApplyModifiedProperties();
        }
        
        public void SetTrigger(string triggerArrayProperty)
        {
            _triggerArrayProperty = triggerArrayProperty;
        }
        
        public void CreateTrigger(string path, string eventTrigger)
        {
            var targetTransform = _triggersGroup.Find(path);
            if (!targetTransform) return;
        
            foreach (var udonScript in targetTransform.GetComponents<UdonSharpBehaviour>())
            {
                var type = udonScript.GetType();
                var methodInfo = type.GetMethod(eventTrigger);
                if (methodInfo == null) continue;
        
                var trigger =
                    AddUdonSharpComponentWithUdonBehavior<CtDialogueTrigger>(_dialogueEntry.Peek().gameObject);
                var so = new SerializedObject(trigger);
                so.FindProperty("target").objectReferenceValue = udonScript;
                so.FindProperty("eventTrigger").stringValue = eventTrigger;
                so.ApplyModifiedProperties();
        
                so = new SerializedObject(_dialogueEntry.Peek());
                var responses = so.FindProperty(_triggerArrayProperty);
                responses.InsertArrayElementAtIndex(responses.arraySize);
                var response = responses.GetArrayElementAtIndex(responses.arraySize - 1);
                response.objectReferenceValue = trigger;
                so.ApplyModifiedProperties();
            }
        }
        
        public bool CreateDialogue(string guid, string dialogue, ushort actorId, ushort conversantId)
        {
            bool result = _visitedNodes.TryGetValue(guid, out var entry);
            if (!result)
            {
                var identifier = _identifierGen++;
                // if (_dialogueResponse)
                // {
                //     var dialogueResponseSo = new SerializedObject(_dialogueResponse);
                //     dialogueResponseSo.FindProperty("nextId").uintValue = identifier;
                //     dialogueResponseSo.ApplyModifiedProperties();
                //     _dialogueResponse = null;
                // }
                // else
                // {
                //     if (_dialogueEntry.Count > 0)
                //     {
                //         var so2 = new SerializedObject(_dialogueEntry.Peek());
                //         so2.FindProperty("nextId").uintValue = identifier;
                //         so2.ApplyModifiedProperties();
                //     }
                // }

                _gameObject = new GameObject(dialogue);
                _gameObject.transform.parent = _conversationGameObject.transform;

                entry = AddUdonSharpComponentWithUdonBehavior<CtDialogueEntry>(_gameObject);
                var so = new SerializedObject(entry);

                so.FindProperty("identifier").uintValue = identifier;
                so.FindProperty("conversationId").uintValue = conversationId;
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
        
            // if (_dialogueEntry.Count > 0)
            // {
            //     var so2 = new SerializedObject(_dialogueEntry.Peek());
            //     so2.FindProperty("nextId").uintValue = entry.Identifier;
            //     so2.ApplyModifiedProperties();
            // }

            if (_dialogueResponse)
            {
                var dialogueResponseSo = new SerializedObject(_dialogueResponse);
                dialogueResponseSo.FindProperty("nextId").uintValue = entry.Identifier;
                dialogueResponseSo.ApplyModifiedProperties();
                _dialogueResponse = null;
            }
            else
            {
                if (_dialogueEntry.Count > 0)
                {
                    var so2 = new SerializedObject(_dialogueEntry.Peek());
                    so2.FindProperty("nextId").uintValue = entry.Identifier;
                    so2.ApplyModifiedProperties();
                }
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