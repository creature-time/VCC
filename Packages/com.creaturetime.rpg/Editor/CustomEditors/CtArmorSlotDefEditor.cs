
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CreatureTime.Editor.Rpg
{
    [CustomEditor(typeof(CtArmorSlotDef), true)]
    public class CtArmorSlotDefEditor : CtAbstractElement
    {
        private Label _description;

        private IntegerField _debugDamage;
        private IntegerField _debugSourceLevel;
        private EnumField _debugDamageType;
        private Label _debugDamageResult;

        public override VisualElement CreateInspectorGUI()
        {
            var rootVisualElement = base.CreateInspectorGUI();

            if (targets.Length == 1)
            {
                _description = new Label
                {
                    style =
                    {
                        fontSize = 14,
                        whiteSpace = WhiteSpace.Normal,
                        marginTop = 8,
                        marginBottom = 8,
                        marginLeft = 8,
                        marginRight = 8,
                        paddingTop = 8,
                        paddingBottom = 8,
                        paddingLeft = 8,
                        paddingRight = 8,
                        borderTopWidth = 1,
                        borderBottomWidth = 1,
                        borderLeftWidth = 1,
                        borderRightWidth = 1,
                        borderTopColor = Color.black,
                        borderBottomColor = Color.black,
                        borderLeftColor = Color.black,
                        borderRightColor = Color.black,
                        borderBottomLeftRadius = 8,
                        borderBottomRightRadius = 8,
                        borderTopLeftRadius = 8,
                        borderTopRightRadius = 8,
                    }
                };
                rootVisualElement.Add(_description);
            }

            UpdateDescription(serializedObject);

            var debugDamage = new Foldout
            {
                text = "Debug Damage",
                value = false
            };
            rootVisualElement.Add(debugDamage);

            _debugDamage = new IntegerField
            {
                label = "Damage",
                value = 42
            };
            debugDamage.Add(_debugDamage);

            _debugDamage.RegisterValueChangedCallback(evt => _DebugDamage());

            _debugSourceLevel = new IntegerField
            {
                label = "Character Level",
                value = 1
            };
            debugDamage.Add(_debugSourceLevel);

            _debugSourceLevel.RegisterValueChangedCallback(evt => _DebugDamage());

            _debugDamageType = new EnumField
            {
                label = ""
            };
            _debugDamageType.Init(EDamageType.None);
            debugDamage.Add(_debugDamageType);

            _debugDamageType.RegisterValueChangedCallback(evt => _DebugDamage());

            _debugDamageResult = new Label();
            debugDamage.Add(_debugDamageResult);

            var foldout = new Foldout
            {
                text = "Default Parameters",
                value = false
            };
            rootVisualElement.Add(foldout);

            Type fallbackEditorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GenericInspector");
            var defaultElements = CreateEditor(targets, fallbackEditorType).CreateInspectorGUI();
            foldout.Add(defaultElements);

            // Whenever any serialized property on this serialized object changes its value, call CheckForWarnings.
            rootVisualElement.TrackSerializedObjectValue(serializedObject, UpdateDescription);

            _DebugDamage();

            return rootVisualElement;
        }

        private void UpdateDescription(SerializedObject _)
        {
            var armorSlotDef = target as CtArmorSlotDef;
            _description.text = "<Invalid>";
            if (armorSlotDef.TryGetFormattedStats(out var equipmentName, out var stats))
                _description.text = $"<b>{equipmentName}</b>\n{stats}";
        }

        private void _DebugDamage()
        {
            var armorSlotDef = (CtArmorSlotDef)target;
            var armorRating = armorSlotDef.CalcArmorRating((EDamageType)_debugDamageType.value);
            _debugDamageResult.text = CtSkillDef.CalcValue(_debugDamage.value, _debugSourceLevel.value, armorRating).ToString();
        }
    }
}
