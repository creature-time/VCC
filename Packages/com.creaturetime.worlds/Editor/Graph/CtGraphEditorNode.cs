
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CreatureTime.Editor.Graph
{
    public class CtGraphEditorNode : Node, IBindable
    {
        public class NodeProperties
        {
            public Port DynamicPort;
            public SerializedProperty FieldProperty;
            public CtInputPortInfoAttribute InputPortInfo;
            public CtOutputPortInfoAttribute OutputPortInfo;

            public NodeProperties(Port dynamicPort, SerializedProperty fieldProperty, CtInputPortInfoAttribute inputPortInfo, CtOutputPortInfoAttribute outputPortInfo)
            {
                DynamicPort = dynamicPort;
                FieldProperty = fieldProperty;
                InputPortInfo = inputPortInfo;
                OutputPortInfo = outputPortInfo;
            }
        }

        private SerializedProperty _nodeProperty;
        private List<CtGraphEditorPort> _inputPorts = new List<CtGraphEditorPort>();
        private List<CtGraphEditorPort> _outputPorts = new List<CtGraphEditorPort>();
        private CtGraphView _view;

        private Dictionary<string, CtGraphEditorPort> _dynamicInputPortLookup = new Dictionary<string, CtGraphEditorPort>();
        private Dictionary<string, CtGraphEditorPort> _dynamicOutputPortLookup = new Dictionary<string, CtGraphEditorPort>();

        public SerializedProperty NodeProperty => _nodeProperty;

        public List<CtGraphEditorPort> InputPorts => _inputPorts;
        public List<CtGraphEditorPort> OutputPorts => _outputPorts;

        public CtGraphEditorPort GetInputPort(string identifier)
        {
            foreach (var port in _inputPorts)
            {
                if (port.viewDataKey == identifier)
                    return port;
            }
            return null;
        }

        public CtGraphEditorPort GetOutputPort(string identifier)
        {
            foreach (var port in _outputPorts)
            {
                if (port.viewDataKey == identifier)
                    return port;
            }
            return null;
        }

        public IBinding binding { get; set; }

        public string bindingPath { get; set; }

        private CtGraphEditorPort CreatePort(string portName, Direction direction, Type type)
        {
            var port = CtGraphEditorPort.Create<Edge>(this, Orientation.Horizontal, direction, type);
            if (type != null)
                SetupPortColor(port);
            port.portName = portName;
            port.tooltip = "";
            return port;
        }

        private void CreateFields(Type type, SerializedProperty serializedProperty, VisualElement parentElement)
        {
            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                     BindingFlags.NonPublic))
            {
                var fieldProperty = serializedProperty.FindPropertyRelative(fieldInfo.Name);
                var exposedProperty = fieldInfo.GetCustomAttribute<CtExposedPropertyAttribute>();
                if (exposedProperty != null)
                    parentElement.Add(CreateField(fieldProperty));
            }
        }

        private PropertyField CreateField(SerializedProperty serializedProperty)
        {
            var field = new PropertyField(serializedProperty)
            {
                bindingPath = serializedProperty.propertyPath
            };
            field.Bind(serializedProperty.serializedObject);
            return field;
        }

        public void Bind(SerializedObject serializedObject)
        {
            extensionContainer.Clear();

            _nodeProperty = serializedObject.FindProperty(bindingPath);
            var nodeType = _nodeProperty.managedReferenceValue.GetType();

            var nodeInfo = nodeType.GetCustomAttribute<CtNodeInfoAttribute>();
            title = nodeInfo.Title;
            name = nodeInfo.MenuItem;
            elementTypeColor = nodeInfo.Color;
            titleContainer.style.backgroundColor = elementTypeColor;

            viewDataKey = _nodeProperty.FindPropertyRelative("guid").stringValue;
            SetPosition(_nodeProperty.FindPropertyRelative("position").rectValue);

            var nodeId = _nodeProperty.FindPropertyRelative("guid").stringValue;

            foreach (var portId in _view.Model.GetInputPorts(nodeId))
            {
                string portName = _view.Model.GetInputPortName(nodeId, portId);
                var port = CreatePort(portName, Direction.Input, _view.Model.GetInputPortType(nodeId, portId));
                port.viewDataKey = portId;
                _inputPorts.Add(port);
                inputContainer.Add(port);
            }

            foreach (var portId in _view.Model.GetOutputPorts(nodeId))
            {
                string portName = _view.Model.GetOutputPortName(nodeId, portId);
                var port = CreatePort(portName, Direction.Output, _view.Model.GetOutputPortType(nodeId, portId));
                port.viewDataKey = portId;
                _outputPorts.Add(port);
                outputContainer.Add(port);
            }

            CreateFields(_nodeProperty.managedReferenceValue.GetType(), _nodeProperty, extensionContainer);

            RefreshExpandedState();
        }

        public void InsertInputPort(int index, string portId)
        {
            var nodeId = _nodeProperty.FindPropertyRelative("guid").stringValue;
            var portType = _view.Model.GetInputPortType(nodeId, portId);
            var port = CreatePort(null, Direction.Input, portType);
            port.viewDataKey = portId;
            _inputPorts.Insert(index, port);
            inputContainer.Insert(index, port);
        }

        public void InsertOutputPort(int index, string portId)
        {
            var nodeId = _nodeProperty.FindPropertyRelative("guid").stringValue;
            var portType = _view.Model.GetOutputPortType(nodeId, portId);
            var port = CreatePort(null, Direction.Output, portType);
            port.viewDataKey = portId;
            _outputPorts.Insert(index, port);
            outputContainer.Insert(index, port);
        }

        public void RemoveInputPort(int index)
        {
            // var port = _outputPorts[index];
            _inputPorts.RemoveAt(index);
            inputContainer.RemoveAt(index);
        }

        public void RemoveOutputPort(int index)
        {
            // var port = _outputPorts[index];
            _outputPorts.RemoveAt(index);
            outputContainer.RemoveAt(index);
        }

        public CtGraphEditorNode(CtGraphView view)
        {
            AddToClassList("dialogue-graph-node");

            _view = view;
        }

        private void SetupPortColor(Port port)
        {
            var portTypeInfo = port.portType.GetCustomAttribute<CtPortTypeInfoAttribute>();
            if (portTypeInfo == null) return;
            port.portColor = portTypeInfo.Color;
        }

        // private void CreateFlowInputPort()
        // {
        //     var nodeProperties = new NodeProperties(null, null, null, null);
        //
        //     var port = DialogueGraphEditorPort.Create<Edge>(this, Orientation.Horizontal, Direction.Input, typeof(PortTypes.FlowPort));
        //     SetupPortColor(port);
        //     port.portName = "";
        //     port.tooltip = "";
        //     port.viewDataKey = $"flowinput_{viewDataKey}";
        //     port.userData = nodeProperties;
        //     _inputPorts.Add(port);
        //     inputContainer.Add(port);
        // }
        //
        // private void CreateFlowOutputPort()
        // {
        //     var nodeProperties = new NodeProperties(null, null, null, null);
        //
        //     var port = DialogueGraphEditorPort.Create<Edge>(this, Orientation.Horizontal, Direction.Output, typeof(PortTypes.FlowPort));
        //     SetupPortColor(port);
        //     port.portName = "";
        //     port.tooltip = "";
        //     port.viewDataKey = $"flowoutput_{viewDataKey}";
        //     port.userData = nodeProperties;
        //     _outputPorts.Add(port);
        //     outputContainer.Add(port);
        // }

        public void UpdatePosition()
        {
            _nodeProperty.FindPropertyRelative("position").rectValue = GetPosition();
            _nodeProperty.serializedObject.ApplyModifiedProperties();
        }

        // public void OnDynamicInputAdded(string fieldName, int index)
        // {
        //     // var dynamicNodeProperties = (NodeProperties)dynamicPort.userData;
        //     // var elementProperty = dynamicNodeProperties.FieldProperty.GetArrayElementAtIndex(index);
        //
        //     // var elementProperty = AddElement(dynamicNodeProperties.FieldProperty);
        //     // var nodeProperties = new NodeProperties(dynamicNodeProperties.DynamicPort, dynamicNodeProperties.FieldProperty, dynamicNodeProperties.InputPortInfo, dynamicNodeProperties.OutputPortInfo);
        //
        //     if (_dynamicInputPortLookup.TryGetValue(fieldName, out var dynamicPort))
        //     {
        //         var dynamicNodeProperties = (NodeProperties)dynamicPort.userData;
        //         var elementProperty = dynamicNodeProperties.FieldProperty.GetArrayElementAtIndex(index);
        //         CreateDynamicPort(dynamicNodeProperties.FieldProperty, elementProperty,
        //             _dynamicInputPortLookup[fieldName], _inputPorts, inputContainer, dynamicNodeProperties);
        //     }
        //
        //     if (_dynamicOutputPortLookup.TryGetValue(fieldName, out dynamicPort))
        //     {
        //         var dynamicNodeProperties = (NodeProperties)dynamicPort.userData;
        //         var elementProperty = dynamicNodeProperties.FieldProperty.GetArrayElementAtIndex(index);
        //         CreateDynamicPort(dynamicNodeProperties.FieldProperty, elementProperty,
        //             _dynamicOutputPortLookup[fieldName], _outputPorts, outputContainer, dynamicNodeProperties);
        //     }
        // }

        // public Port OnDynamicOutputAdded(Port port)
        // {
        //     var dynamicPort = _dynamicInputPortLookup[fieldName];
        //
        //     var dynamicNodeProperties = (NodeProperties)dynamicPort.userData;
        //     var elementProperty = dynamicNodeProperties.FieldProperty.GetArrayElementAtIndex(index);
        //
        //     // var elementProperty = AddElement(dynamicNodeProperties.FieldProperty);
        //     // var nodeProperties = new NodeProperties(dynamicNodeProperties.DynamicPort, dynamicNodeProperties.FieldProperty, dynamicNodeProperties.InputPortInfo, dynamicNodeProperties.OutputPortInfo);
        //
        //     if (dynamicNodeProperties.OutputPortInfo != null)
        //         CreateDynamicPort(dynamicNodeProperties.FieldProperty, elementProperty, _dynamicOutputPortLookup[fieldName], _outputPorts, outputContainer, dynamicNodeProperties);
        //
        //     CreateDynamicPort(dynamicNodeProperties.FieldProperty, elementProperty, _dynamicInputPortLookup[fieldName], _inputPorts, inputContainer, dynamicNodeProperties);
        // }

        // public void DestroyDynamicInputPortData(CtGraphEditorPort port)
        // {
        //     var fieldName = "";
        //
        //     var nodeProperties = (NodeProperties)port.userData;
        //     if (nodeProperties.DynamicPort == null) return;
        //
        //     var arraySize = nodeProperties.FieldProperty.arraySize;
        //     int index = RemoveElement(nodeProperties.FieldProperty, port, _inputPorts, _dynamicInputPortLookup);
        //
        //     if (nodeProperties.InputPortInfo != null)
        //     {
        //         DestroyDynamicPort(index, arraySize, _dynamicInputPortLookup[fieldName], _inputPorts, inputContainer);
        //     }
        //
        //     if (nodeProperties.OutputPortInfo != null)
        //     {
        //         DestroyDynamicPort(index, arraySize, _dynamicOutputPortLookup[fieldName], _outputPorts, outputContainer);
        //     }
        // }
        //
        // public void DestroyDynamicOutputPortData(CtGraphEditorPort port)
        // {
        //     var fieldName = "";
        //
        //     var nodeProperties = (NodeProperties)port.userData;
        //     if (nodeProperties.DynamicPort == null) return;
        //
        //     var arraySize = nodeProperties.FieldProperty.arraySize;
        //     var index = RemoveElement(nodeProperties.FieldProperty, port, _outputPorts, _dynamicOutputPortLookup);
        //
        //     if (nodeProperties.InputPortInfo != null)
        //     {
        //         DestroyDynamicPort(index, arraySize, _dynamicInputPortLookup[fieldName], _inputPorts, inputContainer);
        //     }
        //
        //     if (nodeProperties.OutputPortInfo != null)
        //     {
        //         DestroyDynamicPort(index, arraySize, _dynamicOutputPortLookup[fieldName], _outputPorts, outputContainer);
        //     }
        // }

        // private SerializedProperty AddElement(SerializedProperty fieldProperty)
        // {
        //     int index = fieldProperty.arraySize;
        //     fieldProperty.InsertArrayElementAtIndex(index);
        //     var elementProperty = fieldProperty.GetArrayElementAtIndex(index);
        //     elementProperty.stringValue = GenerateId();
        //     fieldProperty.serializedObject.ApplyModifiedProperties();
        //     return elementProperty;
        // }

        // private int RemoveElement(SerializedProperty fieldProperty, CtGraphEditorPort port, List<CtGraphEditorPort> ports, Dictionary<string, CtGraphEditorPort> lookup)
        // {
        //     var dynamicPort = lookup[fieldProperty.name];
        //     var startIndex = ports.IndexOf(dynamicPort) - fieldProperty.arraySize;
        //     int index = ports.IndexOf(port) - startIndex;
        //     fieldProperty.DeleteArrayElementAtIndex(index);
        //     fieldProperty.serializedObject.ApplyModifiedProperties();
        //     return index;
        // }
        //
        // private Port CreateDynamicPort(SerializedProperty fieldProperty, SerializedProperty elementProperty, CtGraphEditorPort dynamicPort, List<CtGraphEditorPort> ports, VisualElement container, NodeProperties nodeProperties)
        // {
        //     int index = ports.IndexOf(dynamicPort);
        //     var port = CreatePort(null, dynamicPort.direction, dynamicPort.portType, fieldProperty);
        //     port.viewDataKey = elementProperty.stringValue;
        //     port.userData = nodeProperties;
        //     ports.Insert(index, port);
        //     container.Insert(index, port);
        //     return port;
        // }
        //
        // private void DestroyDynamicPort(int elementIndex, int arraySize, CtGraphEditorPort dynamicPort, List<CtGraphEditorPort> ports, VisualElement container)
        // {
        //     var startIndex = ports.IndexOf(dynamicPort) - arraySize;
        //     var index = startIndex + elementIndex;
        //     ports.RemoveAt(index);
        //     container.RemoveAt(index);
        // }

        public void Connect(Node output, Port outputPort, Node input, Port inputPort)
        {
            _view.Connect(output, outputPort, input, inputPort);
        }

        public void Disconnect(string guid)
        {
            _view.Disconnect(guid);
        }
    }
}
