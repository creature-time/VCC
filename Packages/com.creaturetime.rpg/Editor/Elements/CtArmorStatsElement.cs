
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Editor;

namespace CreatureTime
{
    public class CtArmorStatsElement : CtAbstractItemStatsElement
    {
        private ObjectField _armorSelect;
        private SliderInt _levelReq;

        public EArmorSlot AllowedArmorSlot { get; set; } = EArmorSlot.None;

        public string Label
        {
            set => _dataBlock.Label = value;
        }

        public string BindingPath
        {
            set => _dataBlock.BindingPath = value;
        }

        public CtArmorStatsElement()
        {
            VisualElement utilsLayout = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            _container.Add(utilsLayout);

            Button randomize = new Button
            {
                text = "Reset"
            };
            utilsLayout.Add(randomize);

            Button clearButton = new Button
            {
                text = "Clear"
            };
            utilsLayout.Add(clearButton);

            _armorSelect = new ObjectField
            {
                label = "Armor",
                objectType = typeof(CtArmorDef)
            };
            _container.Add(_armorSelect);

            _levelReq = new SliderInt(0, 13)
            {
                label = "Requirement (Unused)"
            };
            _container.Add(_levelReq);

            _armorSelect.RegisterValueChangedCallback(evt =>
            {
                randomize.SetEnabled(evt.newValue);
                UpdateData();
            });
            randomize.clicked += () =>
            {
                CtArmorDef armorDefinition = _armorSelect.value as CtArmorDef;
                if (!armorDefinition)
                {
                    Debug.LogError("There was no armor definition to generation armor.");
                    return;
                }
                // _dataBlock.Value = armorDefinition.GenerateWeapon();
            };
            _dataBlock.DataBlockElement.RegisterValueChangedCallback(_ => SetupFields());

            clearButton.clicked += () => { _dataBlock.Value = CtDataBlock.InvalidData; };

            SetupFields();
        }

        private void SetupFields()
        {
            string displayName = "<Empty>";
            Texture2D texture = null;
            string stats = String.Empty;

            CtArmorDef found = null;
            EItemRarity rarity = EItemRarity.None;
            int req = 0;
            // EWeaponPrefix prefix = EWeaponPrefix.None;
            // EWeaponSuffix suffix = EWeaponSuffix.None;

            ulong data = _dataBlock.Value;
            if (CtDataBlock.IsValid(data))
            {
                EDataType dataType = CtDataBlock.GetDataType(data);
                if (dataType == EDataType.Equipment)
                {
                    string color = "#000000";
                    ushort identifier = CtDataBlock.GetEquipmentIdentifier(data);
                    List<CtArmorDef> armorDefinitions =
                        GameObject.FindObjectsOfType<CtArmorDef>().ToList();
                    found = armorDefinitions.Find(definition => definition.Identifier == identifier);
                    if (found)
                    {
                        displayName = found.DisplayName;
                        texture = found.Icon as Texture2D;

                        if (!texture)
                            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                                "Assets/CreatureTime/Worlds/CreatureTimeRPG/Editor/unknown.png");
                    }

                    // TODO: Setup equipment rarity.
                    color = "#FFFFFF";

                    // rarity = CtDataBlock.GetArmorRarity(data);
                    // switch (rarity)
                    // {
                    //     case EItemRarity.None:
                    //         break;
                    //     case EItemRarity.Common:
                    //         break;
                    //     case EItemRarity.Magical:
                    //         color = "#0000ff";
                    //         break;
                    //     case EItemRarity.Uncommon:
                    //         color = "#ff00ff";
                    //         break;
                    //     case EItemRarity.Rare:
                    //         color = "#ffff00";
                    //         break;
                    //     default:
                    //         Debug.LogError($"Item rarity not supported (rarity={rarity}).");
                    //         break;
                    // }

                    stats += $"<color={color}>{displayName}</color>\n";

                    stats = stats.Trim();
                }
            }

            _icon.image = texture;
            _title.text = (displayName);
            _stats.SetVisible(false);
            _stats.text = stats;

            _armorSelect.SetValueWithoutNotify(found);

            _title.tooltip = stats;
        }

        private void UpdateData()
        {
            CtArmorDef armorDefinition = _armorSelect.value as CtArmorDef;
            if (!armorDefinition)
            {
                _dataBlock.Value = CtDataBlock.InvalidData;
                return;
            }

            ushort identifier = armorDefinition.Identifier;
            if (armorDefinition.ArmorSlot != AllowedArmorSlot)
            {
                _dataBlock.Value = CtDataBlock.InvalidData;
                return;
            }

            ulong data = CtDataBlock.CreateEquipmentData(
                identifier);
            _dataBlock.Value = data;
            // _serializedObject.ApplyModifiedProperties();
            // Debug.Log($"data {_dataBlock.Value:x16}");
        }

        public void Bind(SerializedObject serializedObject)
        {
            _dataBlock.DataBlockElement.Bind(serializedObject);
        }
    }
}