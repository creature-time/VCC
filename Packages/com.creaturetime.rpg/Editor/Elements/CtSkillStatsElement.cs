
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Editor;

namespace CreatureTime
{
    public class CtSkillStatsElement : VisualElement
    {
        private IntegerField _skillInternal;
        private Button _skillIcon;
        private ObjectField _skill;
        private string _bindingPath;

        public string BindingPath
        {
            set => _skillInternal.bindingPath = value;
        }

        public CtSkillStatsElement()
        {
            style.flexDirection = FlexDirection.Row;

            _skillInternal = new IntegerField();
            _skillInternal.SetVisible(false);
            Add(_skillInternal);

            _skillIcon = new Button
            {
                style =
                {
                    minHeight = 32,
                    maxHeight = 32,
                    minWidth = 32,
                    maxWidth = 32,
                    backgroundColor = new StyleColor(Color.black),
                    backgroundSize = new BackgroundSize(32, 32),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat)
                }
            };
            Add(_skillIcon);

            VisualElement skillInfo = new VisualElement
            {
                style =
                {
                    flexGrow = 1.0f,
                    justifyContent = Justify.Center
                }
            };
            Add(skillInfo);

            _skill = new ObjectField
            {
                objectType = typeof(CtSkillDef)
            };
            skillInfo.Add(_skill);

            _skill.RegisterValueChangedCallback(OnSkillChanged);
            _skillInternal.RegisterValueChangedCallback(OnSkillInternalChanged);
        }

        private void OnSkillInternalChanged(ChangeEvent<int> evt)
        {
            int identifier = _skillInternal.value;

            List<CtSkillDef> armorDefinitions =
                GameObject.FindObjectsOfType<CtSkillDef>(true).ToList();
            CtSkillDef found = armorDefinitions.Find(definition => definition.Identifier == identifier);
            _skill.value = found;

            OnUpdateSkillInfo(found);
        }

        private void OnSkillChanged(ChangeEvent<Object> evt)
        {
            CtSkillDef skillDefinition = evt.newValue as CtSkillDef;
            OnUpdateSkillInfo(skillDefinition);

            _skillInternal.value = skillDefinition ? skillDefinition.Identifier : -1;
        }

        private void OnUpdateSkillInfo(CtSkillDef skillDef)
        {
            Texture2D texture = null;
            if (skillDef)
            {
                texture = skillDef.Icon;
                if (!texture)
                    texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        "Assets/CreatureTime/Worlds/CreatureTimeRPG/Editor/unknown.png");
            }

            _skillIcon.style.backgroundImage = texture;
        }

        public void Bind(SerializedObject serializedObject)
        {
            _skillInternal.Bind(serializedObject);
        }
    }
}