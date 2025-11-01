
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace CreatureTime.Editor.Graph
{
    public class CtGraphView : GraphView
    {
        private EditorWindow _window;
        private CtAbstractGraphModel _model;

        public EditorWindow Window => _window;

        public List<CtGraphEditorNode> _nodes = new List<CtGraphEditorNode>();
        public Dictionary<string, CtGraphEditorNode> _nodeLookup = new Dictionary<string, CtGraphEditorNode>();
        public Dictionary<string, Edge> _edgeLookup = new Dictionary<string, Edge>();

        private CtWindowSearchProvider _searchProvider;

        public CtGraphView(EditorWindow window)
        {
            _window = window;

            _searchProvider = ScriptableObject.CreateInstance<CtWindowSearchProvider>();
            _searchProvider.view = this;
            nodeCreationRequest = NodeCreationRequest;

            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.creaturetime.worlds/Editor/DialogueGraph/USS/DialogueGraphEditor.uss");
            styleSheets.Add(style);

            var gridBackground = new GridBackground();
            gridBackground.name = "Grid";
            Add(gridBackground);
            gridBackground.SendToBack();

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        }

        public void Connect(Node output, Port outputPort, Node input, Port inputPort)
        {
            _model.Connect(
                output.viewDataKey, outputPort.viewDataKey,
                input.viewDataKey, inputPort.viewDataKey);
        }

        public void Disconnect(string guid)
        {
            _model.Disconnect(guid);
        }

        public CtAbstractGraphModel Model
        {
            get => _model;
            set
            {
                if (_model != null)
                {
                    deleteSelection -= OnDeleteSelection;
                    graphViewChanged -= OnGraphViewChangedEvent;

                    foreach (var element in graphElements)
                        RemoveElement(element);

                    _model.OnNodeAdded -= OnNodeAdded;
                    _model.OnNodeRemoved -= OnNodeRemoved;
                    _model.OnPortConnect -= OnPortConnect;
                    _model.OnPortDisconnect -= OnPortDisconnect;
                    _model.OnInputPortAdded -= OnInputPortAdded;
                    _model.OnInputPortRemoved -= OnInputPortRemoved;
                    _model.OnOutputPortAdded -= OnOutputPortAdded;
                    _model.OnOutputPortRemoved -= OnOutputPortRemoved;
                    _model.OnModelReset -= OnModelReset;
                }

                _model = value;
                if (_model != null)
                {
                    _model.OnNodeAdded += OnNodeAdded;
                    _model.OnNodeRemoved += OnNodeRemoved;
                    _model.OnPortConnect += OnPortConnect;
                    _model.OnPortDisconnect += OnPortDisconnect;
                    _model.OnInputPortAdded += OnInputPortAdded;
                    _model.OnInputPortRemoved += OnInputPortRemoved;
                    _model.OnOutputPortAdded += OnOutputPortAdded;
                    _model.OnOutputPortRemoved += OnOutputPortRemoved;
                    _model.OnModelReset += OnModelReset;

                    OnModelReset();

                    deleteSelection += OnDeleteSelection;
                    graphViewChanged += OnGraphViewChangedEvent;
                }
            }
        }

        private void OnInputPortAdded(string nodeId, int index, string portId)
        {
            if (_nodeLookup.TryGetValue(nodeId, out var editorNode))
                editorNode.InsertInputPort(index, portId);
        }

        private void OnOutputPortAdded(string nodeId, int index, string portId)
        {
            if (_nodeLookup.TryGetValue(nodeId, out var editorNode))
                editorNode.InsertOutputPort(index, portId);
        }

        private void OnInputPortRemoved(string nodeId, int index, string portId)
        {
            _nodeLookup[nodeId].RemoveInputPort(index);
        }

        private void OnOutputPortRemoved(string nodeId, int index, string portId)
        {
            _nodeLookup[nodeId].RemoveOutputPort(index);
        }

        private void OnModelReset()
        {
            foreach (var element in graphElements)
                RemoveElement(element);

            if (_model == null) return;

            DrawNodes();
            DrawEdges();
        } 

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            foreach (var node in _nodes)
            {
                if (startPort.node == node) continue;

                List<CtGraphEditorPort> ports;
                if (startPort.direction == Direction.Input)
                    ports = node.OutputPorts;
                else
                    ports = node.InputPorts;

                foreach (var port in ports)
                {
                    if (port == startPort) continue;
                    if (port.portType != startPort.portType) continue;
                    compatiblePorts.Add(port);
                }
            }

            return compatiblePorts;
        }

        private void OnDeleteSelection(string operationName, AskUser user)
        {
            if (selection.Count == 0) return;

            // Undo.RecordObject(_serializedObject.targetObject, "Remove Item(s)");
            var edgesToRemove = selection.OfType<Edge>();
            if (edgesToRemove.Any())
            {
                foreach (var edge in edgesToRemove.Reverse())
                    edge.output.Disconnect(edge);
            }

            var nodesToRemove = selection.OfType<CtGraphEditorNode>().Reverse();
            if (nodes.Any())
            {
                foreach (var nodeToRemove in nodesToRemove)
                    _model.RemoveNode(nodeToRemove.viewDataKey);
            }
        }

        private void NodeCreationRequest(NodeCreationContext obj)
        {
            _searchProvider.target = (VisualElement)focusController.focusedElement;
            SearchWindow.Open(new SearchWindowContext(obj.screenMousePosition), _searchProvider);
        }

        private void OnNodeAdded(SerializedProperty nodeProperty) 
        {
            // node.typeName = node.GetType().AssemblyQualifiedName;

            var editorNode = new CtGraphEditorNode(this)
            {
                bindingPath = nodeProperty.propertyPath
            };
            editorNode.Bind(nodeProperty.serializedObject);
            _nodes.Add(editorNode);
            _nodeLookup.Add(nodeProperty.FindPropertyRelative("guid").stringValue, editorNode);
            AddElement(editorNode);
        }

        private void OnNodeRemoved(SerializedProperty nodeElement)
        {
            var guid = nodeElement.FindPropertyRelative("guid").stringValue;
            var node = _nodeLookup[guid];
            RemoveElement(node);

            _nodeLookup.Remove(guid);
        }

        private void OnPortConnect(SerializedProperty edgeProperty)
        {
            var outputNode = _nodeLookup[edgeProperty.FindPropertyRelative("outputId").stringValue];
            var outputPort = outputNode.GetOutputPort(edgeProperty.FindPropertyRelative("outputPortId").stringValue);

            var inputNode = _nodeLookup[edgeProperty.FindPropertyRelative("inputId").stringValue];
            var inputPort = inputNode.GetInputPort(edgeProperty.FindPropertyRelative("inputPortId").stringValue);

            var edge = new Edge();
            // edge.SetEdgeData(edgeData);

            // Edge edge = port.ConnectTo(portOther);
            edge.viewDataKey = edgeProperty.FindPropertyRelative("guid").stringValue;
            edge.output = outputPort.direction == Direction.Output ? outputPort : inputPort;
            edge.input = outputPort.direction == Direction.Input ? outputPort : inputPort;
            Assert.AreNotEqual(edge.output, edge.input);

            outputPort.CommitConnect(edge);
            inputPort.CommitConnect(edge);

            edge.UpdateEdgeControl();

            AddElement(edge);

            _edgeLookup.Add(edge.viewDataKey, edge);
        }

        private void OnPortDisconnect(SerializedProperty edgesProperty)
        {
            var guid = edgesProperty.FindPropertyRelative("guid").stringValue;
            var edge = _edgeLookup[guid];

            ((CtGraphEditorPort)edge.output).CommitDisconnect(edge);
            ((CtGraphEditorPort)edge.input).CommitDisconnect(edge);

            _edgeLookup.Remove(guid);
            RemoveElement(edge);
        }

        private GraphViewChange OnGraphViewChangedEvent(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements != null)
            {
                // Undo.RecordObject(_serializedObject.targetObject, "Node(s) Moved");
                var editorNodes = graphViewChange.movedElements.OfType<CtGraphEditorNode>();
                foreach (var editorNode in editorNodes)
                {
                    editorNode.UpdatePosition();
                }
                // _serializedObject.ApplyModifiedProperties();
            }

            return graphViewChange;
        }

        private void DrawNodes()
        {
            foreach (var nodeProperty in _model.GetNodes())
                OnNodeAdded(nodeProperty);
        }

        private void DrawEdges()
        {
            foreach (var edgeProperty in _model.GetEdges())
                OnPortConnect(edgeProperty);
        }
    }
}
