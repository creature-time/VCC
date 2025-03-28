
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Editor;

namespace CreatureTime
{
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

            _professionDefinitions = Object.FindObjectsOfType<CtProfessionDef>().ToList();
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
            _unused.RegisterValueChangedCallback(evt => _unusedPointError.SetVisible(evt.newValue < 0));

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
            _dataBlock.SetVisible(false);
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
                    int maxAttributePoints = CtSkillDef.CalcAttributePoints(_characterLevel);
                    int usedPoints = CtDataBlock.TotalPointsForAttributeRank(_dataBlock.Value);
                    unusedPoints = maxAttributePoints - usedPoints;
                }
            }

            _unused.value = unusedPoints;
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