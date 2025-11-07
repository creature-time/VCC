using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace CreatureTime.Editor.Graph
{
    public abstract class CtAbstractGraphModel : IBindable
    {
        struct PortData
        {
            public string Id;
            public Type InputType;
            public Type OutputType;
            public bool IsArray;
            public string ParentId;
        }

        private SerializedObject _serializedObject;
        private SerializedProperty _graphProperty;
        private SerializedProperty _nodesProperty;
        private SerializedProperty _edgesProperty;

        private Dictionary<string, List<string>> inputPortLookup = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> outputPortLookup = new Dictionary<string, List<string>>();
        private Dictionary<string, Dictionary<string, PortData>> portDataLookup = new Dictionary<string, Dictionary<string, PortData>>();

        // Events.
        public event Action<SerializedProperty> OnNodeAdded;
        public event Action<SerializedProperty> OnNodeRemoved;
        public event Action<SerializedProperty> OnPortConnect;
        public event Action<SerializedProperty> OnPortDisconnect;
        public event Action<string, int, string> OnInputPortAdded;
        public event Action<string, int, string> OnInputPortRemoved;
        public event Action<string, int, string> OnOutputPortAdded;
        public event Action<string, int, string> OnOutputPortRemoved;
        public event Action OnModelReset;

        public IBinding binding { get; set; }
        public string bindingPath { get; set; }

        public void Bind(SerializedObject serializedObject)
        {
            if (_serializedObject != null)
            {
                if (_graphProperty != null)
                {
                    _nodesProperty = null;
                    _edgesProperty = null;
                    _graphProperty = null;
                }
            }

            _serializedObject = serializedObject;
            if (_serializedObject != null)
            {
                if (string.IsNullOrEmpty(bindingPath))
                {
                    _nodesProperty = _serializedObject.FindProperty("nodes");
                    _edgesProperty = _serializedObject.FindProperty("edges");
                }
                else
                {
                    _graphProperty = _serializedObject.FindProperty(bindingPath);
                    if (_graphProperty != null)
                    {
                        _nodesProperty = _graphProperty.FindPropertyRelative("nodes");
                        _edgesProperty = _graphProperty.FindPropertyRelative("edges");
                    }
                }

                if (_nodesProperty != null)
                {
                    for (int i = 0; i < _nodesProperty.arraySize; i++)
                    {
                        var elementProperty = _nodesProperty.GetArrayElementAtIndex(i);
                        PopulatePorts(elementProperty);
                    }
                }
            }

            OnModelReset?.Invoke();
        }

        public IEnumerable<SerializedProperty> GetNodes()
        {
            if (_nodesProperty == null) yield break;

            for (int i = 0; i < _nodesProperty.arraySize; ++i)
            {
                yield return _nodesProperty.GetArrayElementAtIndex(i);
            }
        }

        public IEnumerable<SerializedProperty> GetEdges()
        {
            if (_edgesProperty == null) yield break;

            for (int i = 0; i < _edgesProperty.arraySize; ++i)
            {
                yield return _edgesProperty.GetArrayElementAtIndex(i);
            }
        }

        public void AddNode(CtGraphNode node)
        {
            int index = _nodesProperty.arraySize;
            _nodesProperty.InsertArrayElementAtIndex(index);
            var elementProperty = _nodesProperty.GetArrayElementAtIndex(index);

            elementProperty.managedReferenceValue = node;

            Type type = node.GetType();
            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                     BindingFlags.NonPublic))
            {
                var fieldProperty = elementProperty.FindPropertyRelative(fieldInfo.Name);
                var inputPortInfo = fieldInfo.GetCustomAttribute<CtInputPortInfoAttribute>();
                var outputPortInfo = fieldInfo.GetCustomAttribute<CtOutputPortInfoAttribute>();

                if (fieldInfo.FieldType.IsArray) continue;
                if (inputPortInfo == null && outputPortInfo == null) continue;

                fieldProperty.stringValue = Guid.NewGuid().ToString();
            }

            _serializedObject.ApplyModifiedProperties();

            PopulatePorts(elementProperty);

            OnNodeAdded?.Invoke(elementProperty);
        }

        public void RemoveNode(string guid)
        {
            for (int i = _edgesProperty.arraySize - 1; i >= 0; i--)
            {
                var edgeElement = _edgesProperty.GetArrayElementAtIndex(i);
                if (edgeElement.FindPropertyRelative("outputId").stringValue == guid || edgeElement.FindPropertyRelative("inputId").stringValue == guid)
                {
                    Disconnect(edgeElement.FindPropertyRelative("guid").stringValue);
                }
            }

            for (int i = 0; i < _nodesProperty.arraySize; i++)
            {
                var nodeElement = _nodesProperty.GetArrayElementAtIndex(i);
                if (nodeElement.FindPropertyRelative("guid").stringValue == guid)
                {
                    OnNodeRemoved?.Invoke(nodeElement);
                    _nodesProperty.DeleteArrayElementAtIndex(i);
                }
            }

            _nodesProperty.serializedObject.ApplyModifiedProperties();
        }

        private PortData AddPortData(string nodeId, string portId, Type inputType, Type outputType, bool isArray, string parentId)
        {
            var portData = new PortData
            {
                Id = portId,
                InputType = inputType,
                OutputType = outputType,
                IsArray = isArray,
                ParentId = parentId
            };

            if (!portDataLookup.TryGetValue(nodeId, out var portLookup))
            {
                portLookup = new Dictionary<string, PortData>();
                portDataLookup.Add(nodeId, portLookup);
            }

            portLookup.TryAdd(portData.Id, portData);

            return portData;
        }

        private void AddPort(string nodeId, string portId, Type inputType, Type outputType, bool isArray, string parentId)
        {
            var portData = AddPortData(nodeId, portId, inputType, outputType, isArray, parentId);

            if (portData.InputType != null)
            {
                int index = AddInputPort(nodeId, portData);
                OnInputPortAdded?.Invoke(nodeId, index, portId);
            }

            if (portData.OutputType != null)
            {
                int index = AddOutputPort(nodeId, portData);
                OnOutputPortAdded?.Invoke(nodeId, index, portId);
            }
        }

        private void RemovePort(string nodeId, PortData portData)
        {
            for (int i = 0; i < _nodesProperty.arraySize; i++)
            {
                var nodeProperty = _nodesProperty.GetArrayElementAtIndex(i);
                if (nodeProperty.FindPropertyRelative("guid").stringValue == nodeId)
                {
                    if (string.IsNullOrEmpty(portData.ParentId)) break;

                    var arrayFieldProperty = nodeProperty.FindPropertyRelative(portData.ParentId);
                    for (int j = 0; j < arrayFieldProperty.arraySize; j++)
                    {
                        if (arrayFieldProperty.GetArrayElementAtIndex(j).stringValue == portData.Id)
                            arrayFieldProperty.DeleteArrayElementAtIndex(j);
                    }
                    arrayFieldProperty.serializedObject.ApplyModifiedProperties();

                    if (portData.InputType != null)
                    {
                        var ports = inputPortLookup[nodeId];
                        OnInputPortRemoved?.Invoke(nodeId, ports.IndexOf(portData.Id), portData.Id);
                        ports.Remove(portData.Id);
                    }

                    if (portData.OutputType != null)
                    {
                        var ports = outputPortLookup[nodeId];
                        OnOutputPortRemoved?.Invoke(nodeId, ports.IndexOf(portData.Id), portData.Id);
                        ports.Remove(portData.Id);
                    }

                    portDataLookup[nodeId].Remove(portData.Id);
                    break;
                }
            }
        }

        private void HandleDynamicPort(string nodeId, ref string portId)
        {
            var portData = portDataLookup[nodeId][portId];
            if (portData.IsArray)
            {
                for (int i = 0; i < _nodesProperty.arraySize; i++)
                {
                    var nodeProperty = _nodesProperty.GetArrayElementAtIndex(i);
                    if (nodeProperty.FindPropertyRelative("guid").stringValue == nodeId)
                    {
                        var arrayProperty = nodeProperty.FindPropertyRelative(portData.Id);
                        int idx = arrayProperty.arraySize;
                        arrayProperty.InsertArrayElementAtIndex(idx);
                        var eleProp = arrayProperty.GetArrayElementAtIndex(idx);
                        var dynamicPortId = Guid.NewGuid().ToString();
                        eleProp.stringValue = dynamicPortId;
                        eleProp.serializedObject.ApplyModifiedProperties();

                        AddPort(nodeId, dynamicPortId, portData.InputType, portData.OutputType, false,
                            portData.Id);

                        portId = dynamicPortId;
                        return;
                    }
                }
            }
        }

        public void Connect(string outputId, string outputPortId, string inputId, string inputPortId)
        {
            Undo.RecordObject(_serializedObject.targetObject, "Connected Edge");

            var edgesToRemove = new List<SerializedProperty>();
            for (int i = _edgesProperty.arraySize - 1; i >= 0; i--)
            {
                var edgeProperty = _edgesProperty.GetArrayElementAtIndex(i);
                if (edgeProperty.FindPropertyRelative("outputId").stringValue == outputId &&
                    edgeProperty.FindPropertyRelative("outputPortId").stringValue == outputPortId)
                    edgesToRemove.Add(edgeProperty);
                else if (edgeProperty.FindPropertyRelative("inputId").stringValue == inputId &&
                         edgeProperty.FindPropertyRelative("inputPortId").stringValue == inputPortId)
                {
                    var type = GetInputPortType(inputId, inputPortId);
                    if (type != typeof(CtGraphPortTypes.FlowPort))
                        edgesToRemove.Add(edgeProperty);
                }
            }

            foreach (var edgeProperty in edgesToRemove)
                Disconnect(edgeProperty.FindPropertyRelative("guid").stringValue, outputPortId, inputPortId);

            int index = _edgesProperty.arraySize;
            _edgesProperty.InsertArrayElementAtIndex(index);
            var elementProperty = _edgesProperty.GetArrayElementAtIndex(index);

            elementProperty.FindPropertyRelative("guid").stringValue = Guid.NewGuid().ToString();

            elementProperty.FindPropertyRelative("outputId").stringValue = outputId;
            HandleDynamicPort(outputId, ref outputPortId);
            elementProperty.FindPropertyRelative("outputPortId").stringValue = outputPortId;

            elementProperty.FindPropertyRelative("inputId").stringValue = inputId;
            HandleDynamicPort(inputId, ref inputPortId);
            elementProperty.FindPropertyRelative("inputPortId").stringValue = inputPortId;

            _serializedObject.ApplyModifiedProperties();

            OnPortConnect?.Invoke(elementProperty);
        }

        public IEnumerable<string> GetInputPorts(string nodeId)
        {
            if (!inputPortLookup.TryGetValue(nodeId, out var ports))
                yield break;

            foreach (var portId in ports)
            {
                yield return portId;
            }
        }

        public IEnumerable<string> GetOutputPorts(string nodeId)
        {
            if (!outputPortLookup.TryGetValue(nodeId, out var ports))
                yield break;

            foreach (var portId in ports)
            {
                yield return portId;
            }
        }

        public string GetInputPortName(string nodeId, string portId)
        {
            if (!portDataLookup.TryGetValue(nodeId, out var lookup))
                return null;
            if (!lookup.TryGetValue(portId, out var portData))
                return null;
            if (portData.InputType == typeof(CtGraphPortTypes.FlowPort)) return null;
            return string.IsNullOrEmpty(portData.ParentId) ? portData.Id : null;
        }

        public string GetOutputPortName(string nodeId, string portId)
        {
            if (!portDataLookup.TryGetValue(nodeId, out var lookup))
                return null;
            if (!lookup.TryGetValue(portId, out var portData))
                return null;
            if (portData.OutputType == typeof(CtGraphPortTypes.FlowPort)) return null;
            return string.IsNullOrEmpty(portData.ParentId) ? portData.Id : null;
        }

        public Type GetInputPortType(string nodeId, string portId)
        {
            if (!portDataLookup.TryGetValue(nodeId, out var lookup))
                return null;
            if (!lookup.TryGetValue(portId, out var portData))
                return null;
            return portData.InputType;
        }

        public Type GetOutputPortType(string nodeId, string portId)
        {
            if (!portDataLookup.TryGetValue(nodeId, out var lookup))
                return null;
            if (!lookup.TryGetValue(portId, out var portData))
                return null;
            return portData.OutputType;
        }

        private void PopulatePorts(SerializedProperty nodeProperty)
        {
            var nodeId = nodeProperty.FindPropertyRelative("guid").stringValue;

            Type nodeType = nodeProperty.managedReferenceValue.GetType();
            var nodeInfo = nodeType.GetCustomAttribute<CtNodeInfoAttribute>();
            if (nodeInfo.HasFlowInput)
            {
                var portData = AddPortData(nodeId, $"flowinput_{nodeId}", typeof(CtGraphPortTypes.FlowPort), null, false, string.Empty);
                AddInputPort(nodeId, portData);
            }

            if (nodeInfo.HasFlowOutput)
            {
                var portData = AddPortData(nodeId, $"flowoutput_{nodeId}", null, typeof(CtGraphPortTypes.FlowPort), false, string.Empty);
                AddOutputPort(nodeId, portData);
            }

            while (nodeType != typeof(CtGraphNode))
            {
                foreach (var fieldInfo in nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                             BindingFlags.NonPublic))
                {
                    var fieldProperty = nodeProperty.FindPropertyRelative(fieldInfo.Name);
                    var inputPortInfo = fieldInfo.GetCustomAttribute<CtInputPortInfoAttribute>();
                    var outputPortInfo = fieldInfo.GetCustomAttribute<CtOutputPortInfoAttribute>();

                    if (inputPortInfo == null && outputPortInfo == null) continue;

                    bool isArray = fieldInfo.FieldType.IsArray;
                    AddPort(nodeId, fieldProperty.name, inputPortInfo?.Type, outputPortInfo?.Type, isArray,
                        String.Empty);
                    if (isArray)
                    {
                        for (int i = 0; i < fieldProperty.arraySize; i++)
                        {
                            var elementProperty = fieldProperty.GetArrayElementAtIndex(i);
                            AddPort(nodeId, elementProperty.stringValue, inputPortInfo?.Type, outputPortInfo?.Type,
                                false, fieldProperty.name);
                        }
                    }
                }

                nodeType = nodeType.BaseType;
            }
        }

        private int AddInputPort(string nodeId, PortData portData)
        {
            if (!inputPortLookup.TryGetValue(nodeId, out var ports))
            {
                ports = new List<string>();
                inputPortLookup.Add(nodeId, ports);
            }

            if (string.IsNullOrEmpty(portData.ParentId))
                ports.Add(portData.Id);
            else
                ports.Insert(ports.IndexOf(portData.ParentId), portData.Id);
            return ports.IndexOf(portData.Id);
        }

        private int AddOutputPort(string nodeId, PortData portData)
        {
            if (!outputPortLookup.TryGetValue(nodeId, out var ports))
            {
                ports = new List<string>();
                outputPortLookup.Add(nodeId, ports);
            }

            if (string.IsNullOrEmpty(portData.ParentId))
                ports.Add(portData.Id);
            else
                ports.Insert(ports.IndexOf(portData.ParentId), portData.Id);
            return ports.IndexOf(portData.Id);
        }

        public void Disconnect(string guid)
        {
            Disconnect(guid, string.Empty, string.Empty);
        }

        public void Disconnect(string guid, string ignoreOutputId, string ignoreInputId)
        {
            for (int i = 0; i < _edgesProperty.arraySize; ++i)
            {
                var edgeProperty = _edgesProperty.GetArrayElementAtIndex(i);
                if (edgeProperty.FindPropertyRelative("guid").stringValue == guid)
                {
                    OnPortDisconnect?.Invoke(edgeProperty);

                    var outputNodeId = edgeProperty.FindPropertyRelative("outputId").stringValue;
                    var outputPortId = edgeProperty.FindPropertyRelative("outputPortId").stringValue;

                    var inputNodeId = edgeProperty.FindPropertyRelative("inputId").stringValue;
                    var inputPortId = edgeProperty.FindPropertyRelative("inputPortId").stringValue;

                    _edgesProperty.DeleteArrayElementAtIndex(i);
                    _serializedObject.ApplyModifiedProperties();

                    if (ignoreOutputId != outputPortId && portDataLookup.TryGetValue(outputNodeId, out var lookup))
                        if (lookup.TryGetValue(outputPortId, out var portData))
                            if (!string.IsNullOrEmpty(portData.ParentId))
                            {
                                var canRemove = true;
                                for (int j = 0; j < _edgesProperty.arraySize; ++j)
                                {
                                    var otherInputPortId = _edgesProperty.GetArrayElementAtIndex(j)
                                        .FindPropertyRelative("inputPortId").stringValue;
                                    if (otherInputPortId == outputPortId)
                                    {
                                        canRemove = false;
                                        break;
                                    }
                                }

                                if (canRemove)
                                    RemovePort(outputNodeId, portData);
                            }

                    if (ignoreInputId != inputPortId && portDataLookup.TryGetValue(inputNodeId, out lookup))
                        if (lookup.TryGetValue(inputPortId, out var portData))
                            if (!string.IsNullOrEmpty(portData.ParentId))
                            {
                                var canRemove = true;
                                for (int j = 0; j < _edgesProperty.arraySize; ++j)
                                {
                                    var otherOutputPortId = _edgesProperty.GetArrayElementAtIndex(j)
                                        .FindPropertyRelative("outputPortId").stringValue;
                                    if (otherOutputPortId == inputPortId)
                                    {
                                        canRemove = false;
                                        break;
                                    }
                                }

                                if (canRemove)
                                    RemovePort(inputNodeId, portData);
                            }

                    break;
                }
            }
        }
    }
}
