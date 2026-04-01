
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CreatureTime
{
    [CustomPropertyDrawer(typeof(CtProfessionAndAttributesAttribute))]
    public class CtProfessionAndAttributesDrawer : PropertyDrawer
    {
        private CtProfessionDef[] _professionDefinitions;
        private string[] _choices;

        public CtProfessionAndAttributesDrawer()
        {
            var professionDefinitions = Object.FindObjectsOfType<CtProfessionDef>(true).ToList();
            professionDefinitions.Sort((a, b) => a.Identifier - b.Identifier);

            _professionDefinitions = new CtProfessionDef[professionDefinitions.Count];
            _choices = new string[professionDefinitions.Count];
            for (int i = 0; i < professionDefinitions.Count; i++)
            {
                var professionDef = professionDefinitions[i];
                _professionDefinitions[i] = professionDef;
                _choices[i] = professionDef.DisplayName;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var data = CtDataBlock.InvalidData;
            if (!string.IsNullOrEmpty(property.stringValue))
                data = CtDataBlock.Deserialize(property.stringValue);

            var professionId = CtDataBlock.GetProfession(data);
            var index = Array.FindIndex(_professionDefinitions, def => def.Identifier == professionId);
            if (index == -1) index = 0;

            var professionDef = _professionDefinitions[index];

            // Return the total height needed for all fields + spacing
            var count = professionDef.Attributes.Length;

            count += 2;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = 2f;

            // total height = two lines + padding between them
            return lineHeight * (count + 1) + padding * count;
        }

        // IMGUI fallback
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = 2;

            GUIStyle title = new GUIStyle(GUI.skin.box);
            title.alignment = TextAnchor.MiddleLeft;
            title.stretchWidth = true;

            var rect = new Rect(position.x, position.y, position.width, lineHeight);
            EditorGUI.LabelField(rect, "Profession", title);

            // Validate data is valid to begin with.
            var data = CtDataBlock.InvalidData;
            if (!string.IsNullOrEmpty(property.stringValue))
                data = CtDataBlock.Deserialize(property.stringValue);

            if (!CtDataBlock.IsValid(data))
            {
                var firstProfession = _professionDefinitions[0];
                data = CtDataBlock.SetProfession(firstProfession.Identifier, firstProfession.Attributes.Length);
                property.stringValue = CtDataBlock.Serialize(data);
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            var professionId = CtDataBlock.GetProfession(data);
            var index = Array.FindIndex(_professionDefinitions, def => def.Identifier == professionId);
            if (index == -1) index = 0;

            var professionDef = _professionDefinitions[index];
            EditorGUI.BeginChangeCheck();
            rect = new Rect(rect.x, rect.y + lineHeight + padding, position.width, lineHeight);
            var selectedIndex = EditorGUI.Popup(rect, "Profession", index, _choices);
            if (EditorGUI.EndChangeCheck())
            {
                professionDef = _professionDefinitions[selectedIndex];
                professionId = professionDef.Identifier;
                data = CtDataBlock.SetProfession(professionId, professionDef.Attributes.Length);
                property.stringValue = CtDataBlock.Serialize(data);
                property.serializedObject.ApplyModifiedProperties();
            }

            rect = new Rect(rect.x, rect.y + lineHeight + padding, position.width, lineHeight);
            EditorGUI.LabelField(rect, "Attributes", title);

            for (var i = 0; i < professionDef.Attributes.Length; i++)
            {
                rect = new Rect(rect.x, rect.y + lineHeight + padding, position.width, lineHeight);

                var attributeDef = professionDef.Attributes[i];

                EditorGUI.BeginChangeCheck();
                var value = EditorGUI.IntField(rect, new GUIContent(attributeDef.DisplayName), CtDataBlock.GetAttributeRank(data, i));
                if (EditorGUI.EndChangeCheck())
                {
                    data = CtDataBlock.SetAttributeRank(i, value, data);
                    property.stringValue = CtDataBlock.Serialize(data);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        // // UI Toolkit path
        // public override VisualElement CreatePropertyGUI(SerializedProperty property)
        // {
        //     var attr = (MyAttribute)attribute;
        //
        //     var field = new PropertyField(property, attr.label);
        //     return field;
        // }
    }

    public class CtProfessionAndAttributesElement : VisualElement
    {
        private List<CtProfessionDef> _professionDefinitions;
        private DropdownField _professions;
        private HelpBox _unusedPointError;
        private IntegerField _unused;
        private Foldout _attributeContainer;
        private SliderInt[] _attributes = { };
        private CtDataBlockElement _dataBlock;

        private SerializedProperty _levelProp;

        private int _professionIndex = -1;
        private int _characterLevel = -1;

        public string BindingPath
        {
            set => _dataBlock.BindingPath = value;
        }

        public int CharacterLevel
        {
            set
            {
                _characterLevel = value;
                HandleUnusedPoints();
            }
        }

        public CtProfessionAndAttributesElement()
        {
            StyleColor alternatingColor = new StyleColor(new Color(0, 0, 0, 0.1f));

            _professionDefinitions = Object.FindObjectsOfType<CtProfessionDef>(true).ToList();
            _professionDefinitions.Sort((a, b) => a.Identifier);

            List<string> choices = new List<string>();
            foreach (CtProfessionDef definition in _professionDefinitions)
                choices.Add(definition.DisplayName);

            _professions = new DropdownField
            {
                label = "Profession",
                choices = choices
            };
            Add(_professions);

            _professions.RegisterValueChangedCallback(_ => OnProfessionChanged());

            _unusedPointError = new HelpBox
            {
                messageType = HelpBoxMessageType.Error,
                text = "Too many points spent!"
            };
            Add(_unusedPointError);

            _unused = new IntegerField
            {
                label = "Unused"
            };
            _unused.SetEnabled(false);
            Add(_unused);
            _unused.RegisterValueChangedCallback(evt => HandleUnusedPoints());

            _attributeContainer = new Foldout
            {
                text = "Attributes",
                style =
                {
                    backgroundColor = alternatingColor
                }
            };
            Add(_attributeContainer);

            _dataBlock = new CtDataBlockElement();
            _dataBlock.visible = false;
            _dataBlock.DataBlockElement.RegisterValueChangedCallback(_ => OnAttributeDataChanged());
            Add(_dataBlock);

            // UpdateAttributeElements();
        }

        private void HandleUnusedPoints()
        {
            int unusedPoints = 0;
            if (CtDataBlock.IsValid(_dataBlock.Value))
            {
                if (_characterLevel > 0)
                {
                    int maxAttributePoints = CtRpgFormulas.CalcAttributePoints(_characterLevel);
                    int usedPoints = CtDataBlock.TotalPointsForAttributeRank(_dataBlock.Value);
                    unusedPoints = maxAttributePoints - usedPoints;
                }
            }

            _unused.value = unusedPoints;
            _unusedPointError.visible = _unused.value < 0;
        }

        private int FindProfessionIndexByIdentifier(ushort identifier)
        {
            int index = -1;
            if (!CtDataBlock.IsValid(_dataBlock.Value))
                return index;

            return _professionDefinitions.FindIndex(definition => definition.Identifier == identifier);
        }

        private void OnProfessionChanged()
        {
            if (_professionIndex == _professions.index)
                return;
            _professionIndex = _professions.index;
            UpdateAttributeElements();
            SetData();
        }

        private void UpdateAttributeElements()
        {
            _attributeContainer.Clear();

            if (_professionIndex == -1)
            {
                _attributes = new SliderInt[] { };
                return;
            }

            CtProfessionDef professionDefinition = _professionDefinitions[_professionIndex];
            CtAttributeDef[] attributeDefinitions =
                professionDefinition.GetComponentsInChildren<CtAttributeDef>();
            _attributes = new SliderInt[attributeDefinitions.Length];
            for (int i = 0; i < attributeDefinitions.Length; ++i)
            {
                if (i < attributeDefinitions.Length)
                {
                    SliderInt field = new SliderInt(0, 12)
                    {
                        label = attributeDefinitions[i].DisplayName,
                        style =
                        {
                            flexGrow = 1
                        }
                    };
                    _attributeContainer.Add(field);
                    _attributes[i] = field;
                    field.RegisterValueChangedCallback(_ => SetData());
                }
            }
        }

        private void OnAttributeDataChanged()
        {
            int professionIndex = -1;
            if (CtDataBlock.IsValid(_dataBlock.Value))
                professionIndex = FindProfessionIndexByIdentifier(CtDataBlock.GetProfession(_dataBlock.Value));

            if (_professionIndex != professionIndex)
            {
                _professions.index = professionIndex;
                _professionIndex = professionIndex;
                UpdateAttributeElements();
            }

            for (int i = 0; i < _attributes.Length; ++i)
                _attributes[i].SetValueWithoutNotify(CtDataBlock.GetAttributeRank(_dataBlock.Value, i));

            HandleUnusedPoints();
        }

        public void Bind(SerializedObject serializedObject)
        {
            _dataBlock.DataBlockElement.Bind(serializedObject);
        }

        private void SetData()
        {
            CtProfessionDef professionDefinition = _professionDefinitions[_professionIndex];
            ulong data = CtDataBlock.SetProfession(professionDefinition.Identifier,
                (ushort)_attributes.Length);
            for (int i = 0; i < _attributes.Length; ++i)
            {
                ushort value = (ushort)_attributes[i].value;
                data = CtDataBlock.SetAttributeRank(i, value, data);
            }

            _dataBlock.Value = data;
        }
    }
}