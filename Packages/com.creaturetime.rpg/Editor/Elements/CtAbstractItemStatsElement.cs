
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CreatureTime
{
    [CustomPropertyDrawer(typeof(CtSkillAttribute), true)]
    public class CtSkillDrawer : PropertyDrawer
    {
        // IMGUI fallback
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var skillId = property.uintValue;

            var skillDefs = Object.FindObjectsOfType<CtSkillDef>(true);
            var index = Array.FindIndex(skillDefs, def => def.Identifier == skillId);
            CtSkillDef skillDef = null;
            Texture icon = null;
            var displayName = "-";
            var description = "-";
            if (index != -1)
            {
                skillDef = skillDefs[index];
                icon = skillDef.Icon;
                displayName = skillDef.DisplayName;
                description = skillDef.GetDescription(12);
            }

            GUIStyle bordered = new GUIStyle(GUI.skin.box);
            bordered.padding = new RectOffset(8, 8, 8, 8);

            GUILayout.BeginVertical(bordered);

            GUIStyle title = new GUIStyle(GUI.skin.box);
            title.alignment = TextAnchor.MiddleLeft;
            title.stretchWidth = true;

            GUILayout.Box(property.displayName, title);

            GUILayout.BeginHorizontal();

            float iconSize = 64f;

            var iconStyle = new GUIStyle(GUI.skin.label);
            iconStyle.fixedWidth = iconSize;
            iconStyle.fixedHeight = iconSize;
            GUILayout.Label(icon, iconStyle);

            GUILayout.BeginVertical();

            var style = new GUIStyle(GUI.skin.label);
            style.richText = true;

            GUILayout.Label(displayName, style);
            GUILayout.Label(description, style);

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            skillDef = (CtSkillDef)EditorGUILayout.ObjectField(new GUIContent("Skill"), skillDef,
                typeof(CtSkillDef), true);
            if (EditorGUI.EndChangeCheck())
            {
                property.uintValue = skillDef ? skillDef.Identifier : CtConstants.InvalidId;
                property.serializedObject.ApplyModifiedProperties();
            }

            GUILayout.EndVertical();
        }
    }

    [CustomPropertyDrawer(typeof(CtItemAttribute), true)]
    public class CtItemDrawer : PropertyDrawer
    {
        private void SetupWeaponDef(Rect rect, float lineHeight, float padding, SerializedProperty property, ulong data)
        {
            var identifier = CtDataBlock.GetWeaponIdentifier(data);
            var weaponDefs = Object.FindObjectsOfType<CtWeaponDef>(true);
            var index = Array.FindIndex(weaponDefs, def => def.Identifier == identifier);

            CtWeaponDef weaponDef = null;
            Texture icon = null;
            var itemName = "-";
            var itemDescription = "-";
            EItemRarity itemRarity = EItemRarity.None;

            var requirement = -1;
            if (index != -1)
            {
                weaponDef = weaponDefs[index];
                icon = weaponDef.Icon;

                weaponDef.GetFormattedStats(data, ref itemName, ref itemDescription, ref itemRarity, ref requirement);
            }

            _SetUpItemHeader(ref rect, lineHeight, padding, icon, itemName, itemDescription);

            var rarity = CtDataBlock.GetWeaponRarity(data);
            requirement = CtDataBlock.GetWeaponRequirement(data);
            
            EditorGUI.BeginChangeCheck();
            rect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
            weaponDef = (CtWeaponDef)EditorGUI.ObjectField(rect, new GUIContent("Weapon"), weaponDef,
                typeof(CtWeaponDef), true);
            if (EditorGUI.EndChangeCheck())
            {
                data = CtDataBlock.CreateWeaponData(weaponDef.Identifier, EWeaponPrefix.None, EWeaponSuffix.None,
                    requirement, rarity);
                property.stringValue = CtDataBlock.Serialize(data);
                property.serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUI.BeginChangeCheck();
            rect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
            rarity = (EItemRarity)EditorGUI.EnumPopup(rect, new GUIContent("Rarity"), rarity);
            if (EditorGUI.EndChangeCheck())
            {
                data = CtDataBlock.CreateWeaponData(weaponDef.Identifier, EWeaponPrefix.None, EWeaponSuffix.None,
                    requirement, rarity);
                property.stringValue = CtDataBlock.Serialize(data);
                property.serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUI.BeginChangeCheck();
            rect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
            requirement = EditorGUI.IntField(rect, new GUIContent("Requirement"), requirement);
            if (EditorGUI.EndChangeCheck())
            {
                data = CtDataBlock.CreateWeaponData(weaponDef.Identifier, EWeaponPrefix.None, EWeaponSuffix.None,
                    requirement, rarity);
                property.stringValue = CtDataBlock.Serialize(data);
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private void SetupEquipmentInfo(Rect rect, float lineHeight, float padding, SerializedProperty property, ulong data, bool isDynamic, EArmorSlot armorSlot)
        {
            var identifier = CtDataBlock.GetEquipmentIdentifier(data);
            if (isDynamic)
                armorSlot = CtDataBlock.GetEquipmentSlot(data);
            var armorSetDefs = Object.FindObjectsOfType<CtArmorSetDef>(true);
            var index = Array.FindIndex(armorSetDefs, def => def.Identifier == identifier);
            CtArmorSetDef armorSetDef = null;
            Texture icon = null;
            var itemName = "-";
            var itemDescription = "-";

            if (index != -1)
            {
                armorSetDef = armorSetDefs[index];
                var armorSlotDef = armorSetDef.GetArmorSlot(armorSlot);
                icon = armorSlotDef.Icon;

                armorSlotDef.TryGetFormattedStats(out itemName, out itemDescription);
            }

            _SetUpItemHeader(ref rect, lineHeight, padding, icon, itemName, itemDescription);

            EditorGUI.BeginChangeCheck();
            rect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
            armorSetDef = (CtArmorSetDef)EditorGUI.ObjectField(rect, new GUIContent("Armor Set"), armorSetDef,
                typeof(CtArmorSetDef), true);
            if (EditorGUI.EndChangeCheck())
            {
                data = CtDataBlock.CreateEquipmentData(armorSetDef.Identifier, armorSlot);
                property.stringValue = CtDataBlock.Serialize(data);
                property.serializedObject.ApplyModifiedProperties();
            }
            
            if (isDynamic)
            {
                EditorGUI.BeginChangeCheck();
                rect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
                armorSlot = (EArmorSlot)EditorGUI.EnumPopup(rect, new GUIContent("ArmorSlot"), armorSlot);
                if (EditorGUI.EndChangeCheck())
                {
                    data = CtDataBlock.CreateEquipmentData(armorSetDef.Identifier, armorSlot);
                    property.stringValue = CtDataBlock.Serialize(data);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void _SetUpItemHeader(ref Rect rect, float lineHeight, float padding, Texture icon, string itemName, string itemDescription)
        {
            float iconSize = 64f;

            var headerRect = new Rect(rect.x, rect.y + lineHeight + padding, iconSize, iconSize);
            if (icon)
            {
                GUI.DrawTexture(headerRect, icon);
            }
            else
            {
                EditorGUI.DrawRect(headerRect, Color.black);
            }

            headerRect = new Rect(headerRect.x + iconSize + padding, headerRect.y, rect.width - iconSize - padding, iconSize);
            var style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            style.alignment = TextAnchor.UpperLeft;
            EditorGUI.LabelField(headerRect, $"{itemName}\n{itemDescription}", style);

            rect = new Rect(rect.x, rect.y + iconSize, rect.width, rect.height);
        }

        private void SetupOffHandInfo(Rect rect, float lineHeight, float padding, SerializedProperty property, ulong data)
        {
            var weaponId = CtDataBlock.GetOffHandIdentifier(data);
            var offHandDefs = Object.FindObjectsOfType<CtOffHandDef>(true);
            var index = Array.FindIndex(offHandDefs, def => def.Identifier == weaponId);

            CtOffHandDef offHandDef = null;
            Texture icon = null;
            string itemName = "-";
            string itemDescription = "-";
            EItemRarity itemRarity = EItemRarity.None;
            var modifierStat = -1;
            var requirement = -1;
            if (index != -1)
            {
                offHandDef = offHandDefs[index];
                icon = offHandDef.Icon;

                offHandDef.GetFormattedStats(data, ref itemName, ref itemDescription, ref itemRarity, ref modifierStat,
                    ref requirement);
            }

            _SetUpItemHeader(ref rect, lineHeight, padding, icon, itemName, itemDescription);

            var rarity = CtDataBlock.GetOffHandRarity(data);
            requirement = CtDataBlock.GetOffHandRequirement(data);
            modifierStat = CtDataBlock.GetOffHandModifierStat(data);
            
            EditorGUI.BeginChangeCheck();
            rect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
            offHandDef = (CtOffHandDef)EditorGUI.ObjectField(rect, new GUIContent("Off-Hand"), offHandDef,
                typeof(CtOffHandDef), true);
            if (EditorGUI.EndChangeCheck())
            {
                data = CtDataBlock.CreateOffHandData(offHandDef.Identifier, EOffHandPrefix.None, EOffHandSuffix.None,
                    requirement, rarity, modifierStat);
                property.stringValue = CtDataBlock.Serialize(data);
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginChangeCheck();
            rect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
            rarity = (EItemRarity)EditorGUI.EnumPopup(rect, new GUIContent("Rarity"), rarity);
            if (EditorGUI.EndChangeCheck())
            {
                data = CtDataBlock.CreateOffHandData(offHandDef.Identifier, EOffHandPrefix.None, EOffHandSuffix.None,
                    requirement, rarity, modifierStat);
                property.stringValue = CtDataBlock.Serialize(data);
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginChangeCheck();
            rect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
            requirement = EditorGUI.IntField(rect, new GUIContent("Requirement"), requirement);
            if (EditorGUI.EndChangeCheck())
            {
                data = CtDataBlock.CreateOffHandData(offHandDef.Identifier, EOffHandPrefix.None, EOffHandSuffix.None,
                    requirement, rarity, modifierStat);
                property.stringValue = CtDataBlock.Serialize(data);
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginChangeCheck();
            rect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
            modifierStat = EditorGUI.IntField(rect, new GUIContent("Modifier Stat"), modifierStat);
            if (EditorGUI.EndChangeCheck())
            {
                data = CtDataBlock.CreateOffHandData(offHandDef.Identifier, EOffHandPrefix.None, EOffHandSuffix.None,
                    requirement, rarity, modifierStat);
                property.stringValue = CtDataBlock.Serialize(data);
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = 2;

            var attr = (CtItemAttribute)attribute;

            var count = 1;
            var isDynamic = attr.DataType == EDataType.None;
            if (isDynamic)
                count += 1;

            var data = CtDataBlock.InvalidData;
            if (!string.IsNullOrEmpty(property.stringValue))
                data = CtDataBlock.Deserialize(property.stringValue);

            EDataType dataType = EDataType.None;
            if (CtDataBlock.IsValid(data))
                dataType = isDynamic ? CtDataBlock.GetDataType(data) : attr.DataType;

            switch (dataType)
            {
                case EDataType.None:
                    break;
                case EDataType.Weapon:
                    count += 3;
                    break;
                case EDataType.Equipment:
                    count += 1;
                    if (isDynamic)
                        count += 1;
                    break;
                case EDataType.OffHand:
                    count += 4;
                    break;
                case EDataType.Item:
                    // count += 4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return count * lineHeight + padding * (count - 1) + 64;
        }

        // IMGUI fallback
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = 2;

            var attr = (CtItemAttribute)attribute;

            var isDynamic = attr.DataType == EDataType.None;

            // Validate data is valid to begin with.
            var data = CtDataBlock.InvalidData;
            if (!string.IsNullOrEmpty(property.stringValue))
                data = CtDataBlock.Deserialize(property.stringValue);

            if (!CtDataBlock.IsValid(data))
            {
                if (!isDynamic)
                {
                    switch (attr.DataType)
                    {
                        case EDataType.None:
                            data = CtDataBlock.InvalidData;
                            break;
                        case EDataType.Weapon:
                            data = CtDataBlock.CreateWeaponData(CtConstants.InvalidId, EWeaponPrefix.None, EWeaponSuffix.None,
                                0, EItemRarity.None);
                            break;
                        case EDataType.OffHand:
                            data = CtDataBlock.CreateOffHandData(CtConstants.InvalidId, EOffHandPrefix.None, EOffHandSuffix.None,
                                0, EItemRarity.None, 0);
                            break;
                        case EDataType.Equipment:
                            data = CtDataBlock.CreateEquipmentData(CtConstants.InvalidId, attr.ArmorSlot);
                            break;
                        case EDataType.Item:
                            // data = CtDataBlock.CreateEquipmentData(CtConstants.InvalidId, attr.ArmorSlot);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    property.stringValue = CtDataBlock.Serialize(data);
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
                else
                {
                    property.stringValue = CtDataBlock.Serialize(data);
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EDataType dataType = EDataType.None;
            if (CtDataBlock.IsValid(data))
                dataType = isDynamic ? CtDataBlock.GetDataType(data) : attr.DataType;

            GUIStyle title = new GUIStyle(GUI.skin.box);
            title.alignment = TextAnchor.MiddleLeft;
            title.stretchWidth = true;

            var rect = new Rect(position.x, position.y, position.width, lineHeight);
            EditorGUI.LabelField(rect, label, title);

            if (isDynamic)
            {
                rect = new Rect(rect.x, rect.y + lineHeight + padding, position.width, lineHeight);

                EditorGUI.BeginChangeCheck();
                dataType = (EDataType)EditorGUI.EnumPopup(rect, new GUIContent("Data Type"), dataType);
                if (EditorGUI.EndChangeCheck())
                {
                    switch (dataType)
                    {
                        case EDataType.None:
                            data = CtDataBlock.InvalidData;
                            break;
                        case EDataType.Weapon:
                            data = CtDataBlock.CreateWeaponData(CtConstants.InvalidId, EWeaponPrefix.None, EWeaponSuffix.None,
                                0, EItemRarity.None);
                            break;
                        case EDataType.OffHand:
                            data = CtDataBlock.CreateOffHandData(CtConstants.InvalidId, EOffHandPrefix.None, EOffHandSuffix.None,
                                0, EItemRarity.None, 0);
                            break;
                        case EDataType.Equipment:
                            data = CtDataBlock.CreateEquipmentData(CtConstants.InvalidId, EArmorSlot.Head);
                            break;
                        case EDataType.Item:
                            // data = CtDataBlock.CreateEquipmentData(CtConstants.InvalidId, EArmorSlot.Head);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    property.stringValue = CtDataBlock.Serialize(data);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            switch (dataType)
            {
                case EDataType.None:
                    break;
                case EDataType.Weapon:
                    SetupWeaponDef(rect, lineHeight, padding, property, data);
                    break;
                case EDataType.OffHand:
                    SetupOffHandInfo(rect, lineHeight, padding, property, data);
                    break;
                case EDataType.Equipment:
                    SetupEquipmentInfo(rect, lineHeight, padding, property, data, isDynamic, attr.ArmorSlot);
                    break;
                case EDataType.Item:
                    // SetupEquipmentInfo(rect, lineHeight, padding, property, data, isDynamic, attr.ArmorSlot);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class CtAbstractItemStatsElement : VisualElement
    {
        protected Image _icon;
        protected Label _title;
        protected Label _stats;
        protected VisualElement _container;
        protected CtDataBlockElement _dataBlock;

        public string BindingPath
        {
            set => _dataBlock.BindingPath = value;
        }

        public CtDataBlockElement DataBlock => _dataBlock;

        public CtAbstractItemStatsElement()
        {
            StyleColor borderColor = new StyleColor(Color.black);
            VisualElement layout = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginLeft = 4,
                    marginRight = 4,
                    marginTop = 4,
                    marginBottom = 4
                }
            };
            Add(layout);

            VisualElement iconLayout = new VisualElement();
            layout.Add(iconLayout);

            VisualElement contentLayout = new VisualElement()
            {
                style =
                {
                    flexGrow = 1.0f,
                }
            };
            layout.Add(contentLayout);

            _icon = new Image
            {
                style =
                {
                    backgroundColor = new StyleColor(Color.black),
                    width = 32, height = 32,
                    marginLeft = 4,
                    marginRight = 4,
                    marginTop = 4,
                    marginBottom = 4,
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
            iconLayout.Add(_icon);

            _title = new Label
            {
                text = "<Invalid Title>",
                style =
                {
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginLeft = 4,
                    marginBottom = 4,
                    marginRight = 4,
                    marginTop = 4
                }
            };
            contentLayout.Add(_title);

            _stats = new Label
            {
                style =
                {
                    fontSize = 12,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };
            contentLayout.Add(_stats);

            _container = new VisualElement();
            contentLayout.Add(_container);

            _dataBlock = new CtDataBlockElement();
            _dataBlock.visible = false;
            _container.Add(_dataBlock);
        }

        public void Bind(SerializedObject serializedObject)
        {
            _dataBlock.DataBlockElement.Bind(serializedObject);
        }
    }
}