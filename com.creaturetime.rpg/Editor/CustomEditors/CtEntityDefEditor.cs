
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CreatureTime.Editor.Rpg
{
    // [CustomEditor(typeof(CtEntityDef), true)]
    public class CtEntityDefEditor : CtAbstractElement
    {
        private CtProfessionAndAttributesElement _attributeDataField;
        private SliderInt _levelField;
        private ObjectField _npcBehavior;
        private SerializedProperty _expProperty;

        public override VisualElement CreateInspectorGUI()
        {
            var rootVisualElement = base.CreateInspectorGUI();

            serializedObject.Update();

            _expProperty = serializedObject.FindProperty("exp");

            StyleColor alternatingColor = new StyleColor(new Color(0, 0, 0, 0.1f));

            VisualElement header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    backgroundColor = new StyleColor(Color.black),
                    backgroundSize = new BackgroundSize(128, 128),
                    backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat),
                    unityBackgroundImageTintColor = new Color(0.25f, 0.25f, 0.25f)
                }
            };
            rootVisualElement.Add(header);

            StyleColor borderColor = new StyleColor(Color.black);
            Image iconPreview = new Image
            {
                style =
                {
                    backgroundColor = new StyleColor(Color.black),
                    width = 64, height = 64,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftColor = borderColor,
                    borderRightColor = borderColor,
                    borderTopColor = borderColor,
                    borderBottomColor = borderColor,
                }
            };
            header.Add(iconPreview);

            VisualElement infoLayout = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    justifyContent = Justify.Center,
                    flexGrow = 1,
                    paddingLeft = 4,
                    paddingRight = 4,
                    paddingTop = 4,
                    paddingBottom = 4,
                }
            };
            header.Add(infoLayout);

            TextField entityName = new TextField
            {
                bindingPath = "displayName",
                style =
                {
                    fontSize = 24,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    textOverflow = TextOverflow.Clip
                }
            };
            infoLayout.Bind(serializedObject);
            infoLayout.Add(entityName);

            CtRpgFormulas.ConvertExpToLevel(_expProperty.intValue, out var level, out var start, out var end,
                out var baseHealth);

            _levelField = new SliderInt(1, 30)
            {
                value = level,
                label = "Lvl #",
                style =
                {
                    fontSize = 12,
                }
            };
            infoLayout.Add(_levelField);

            {
                MonoScript componentScriptSource = null;
                CtEntityDef source = serializedObject.targetObject as CtEntityDef;
                if (source)
                {
                    string[] assets = AssetDatabase.FindAssets($"t:MonoScript {source.GetType().Name}",
                        new[] { "Packages/com.creaturetime.rpg/Runtime" });
                    if (assets.Length > 0)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
                        componentScriptSource = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                    }
                }

                ObjectField sourceScript = new ObjectField
                {
                    label = "Source Script",
                    objectType = typeof(MonoScript),
                    value = componentScriptSource,
                    style =
                    {
                        marginLeft = 8,
                        marginRight = 8,
                        marginTop = 8,
                        marginBottom = 8
                    }
                };
                sourceScript.SetEnabled(false);
                rootVisualElement.Add(sourceScript);
            }

            ObjectField icon = new ObjectField
            {
                label = "Icon",
                bindingPath = "icon",
                objectType = typeof(Texture)
            };
            rootVisualElement.Add(icon);

            if (serializedObject.targetObject.GetType() == typeof(CtNpcDef))
            {
                _npcBehavior = new ObjectField
                {
                    label = "Npc Behavior",
                    objectType = typeof(CtNpcBehavior),
                    bindingPath = "behavior"
                };
                rootVisualElement.Add(_npcBehavior);
            }

            icon.RegisterValueChangedCallback(evt =>
            {
                var texture = (Texture2D)evt.newValue;
                if (!texture)
                    texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        "Assets/CreatureTime/Worlds/CreatureTimeRPG/Editor/unknown.png");

                iconPreview.image = texture;
                header.style.backgroundImage = texture;
            });

            Foldout professionGroup = new Foldout
            {
                text = "Profession"
            };
            rootVisualElement.Add(professionGroup);

            _attributeDataField = new CtProfessionAndAttributesElement
            {
                BindingPath = "attributeData",
            };
            _attributeDataField.Bind(serializedObject);
            professionGroup.Add(_attributeDataField);

            Foldout weaponsGroup = new Foldout
            {
                text = "Weapons"
            };
            rootVisualElement.Add(weaponsGroup);

            CtWeaponStatsElement mainHandWeapon = new CtWeaponStatsElement
            {
                Label = "Main-Hand Weapon Data",
                BindingPath = "mainHandWeaponData",
                style =
                {
                    backgroundColor = alternatingColor
                }
            };
            mainHandWeapon.Bind(serializedObject);
            weaponsGroup.Add(mainHandWeapon);

            CtOffHandStatsElement offHandWeapon = new CtOffHandStatsElement
            {
                Label = "Off-Hand Weapon Data",
                BindingPath = "offHandWeaponData"
            };
            offHandWeapon.Bind(serializedObject);
            weaponsGroup.Add(offHandWeapon);

            Foldout armorGroup = new Foldout
            {
                text = "Armor"
            };
            rootVisualElement.Add(armorGroup);

            CtArmorStatsElement armorHead = new CtArmorStatsElement
            {
                Label = "Head",
                ForceArmorSlot = true,
                AllowedArmorSlot = EArmorSlot.Head,
                BindingPath = "headSlotData",
                style =
                {
                    backgroundColor = alternatingColor
                }
            };
            armorHead.Bind(serializedObject);
            armorGroup.Add(armorHead);

            CtArmorStatsElement armorChest = new CtArmorStatsElement
            {
                Label = "Chest",
                ForceArmorSlot = true,
                AllowedArmorSlot = EArmorSlot.Chest,
                BindingPath = "chestSlotData"
            };
            armorChest.Bind(serializedObject);
            armorGroup.Add(armorChest);

            CtArmorStatsElement armorHands = new CtArmorStatsElement
            {
                Label = "Hands",
                ForceArmorSlot = true,
                AllowedArmorSlot = EArmorSlot.Hands,
                BindingPath = "handsSlotData",
                style =
                {
                    backgroundColor = alternatingColor
                }
            };
            armorHands.Bind(serializedObject);
            armorGroup.Add(armorHands);

            CtArmorStatsElement armorLegs = new CtArmorStatsElement
            {
                Label = "Legs",
                ForceArmorSlot = true,
                AllowedArmorSlot = EArmorSlot.Legs,
                BindingPath = "legsSlotData"
            };
            armorLegs.Bind(serializedObject);
            armorGroup.Add(armorLegs);

            CtArmorStatsElement armorFeet = new CtArmorStatsElement
            {
                Label = "Feet",
                ForceArmorSlot = true,
                AllowedArmorSlot = EArmorSlot.Feet,
                BindingPath = "feetSlotData",
                style =
                {
                    backgroundColor = alternatingColor
                }
            };
            armorFeet.Bind(serializedObject);
            armorGroup.Add(armorFeet);

            _levelField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue < 1)
                    _levelField.value = 1;
                else if (evt.newValue > 30)
                    _levelField.value = 30;

                _expProperty.intValue = CtRpgFormulas.ConvertLevelToMinExp(_levelField.value);
                serializedObject.ApplyModifiedProperties();

                _UpdateLevelAndAttributes(_levelField.value);
            });

            Foldout skills = new Foldout
            {
                text = "Skills"
            };
            rootVisualElement.Add(skills);

            for (int i = 0; i < 10; i++)
            {
                CtSkillStatsElement skill = new CtSkillStatsElement
                {
                    BindingPath = $"skillSlot{i}"
                };
                skill.Bind(serializedObject);
                skills.Add(skill);
            }

            if (serializedObject.targetObject.GetType() == typeof(CtPlayerDef))
            {
                var inventory = serializedObject.FindProperty("inventory");
                for (int i = 0; i < inventory.arraySize; i++)
                {
                    var elementProperty = inventory.GetArrayElementAtIndex(i);
                    CtInventoryElement invItem = new CtInventoryElement
                    {
                        BindingPath = elementProperty.propertyPath
                    };
                    invItem.Bind(serializedObject);
                    rootVisualElement.Add(invItem);
                }
            }

            _UpdateLevelAndAttributes(_levelField.value);

            Foldout foldout = new Foldout
            {
                text = "Default Parameters",
                value = false
            };
            rootVisualElement.Add(foldout);

            Type fallbackEditorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GenericInspector");
            VisualElement defaultElements = CreateEditor(targets, fallbackEditorType).CreateInspectorGUI();
            foldout.Add(defaultElements);

            return rootVisualElement;
        }

        private void _UpdateLevelAndAttributes(int level)
        {
            var text = $"Lvl {level}";
            if (level > 20)
                text += "*";

            _levelField.label = text;
            _attributeDataField.CharacterLevel = level;
        }
    }
}