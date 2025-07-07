
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Editor;
using Object = UnityEngine.Object;

namespace CreatureTime
{
    public class CtArmorStatsElement : CtAbstractItemStatsElement
    {
        private ObjectField _armorSelect;

        public EArmorSlot AllowedArmorSlot { get; set; }

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
            // utilsLayout.Add(randomize);

            Button clearButton = new Button
            {
                text = "Clear"
            };
            // utilsLayout.Add(clearButton);

            _armorSelect = new ObjectField
            {
                label = "Armor",
                objectType = typeof(CtArmorSetDef)
            };
            _container.Add(_armorSelect);

            _armorSelect.RegisterValueChangedCallback(evt =>
            {
                randomize.SetEnabled(evt.newValue);
                UpdateData();
            });
            randomize.clicked += () =>
            {
                CtArmorSlotDef armorSetDefinition = _armorSelect.value as CtArmorSlotDef;
                if (!armorSetDefinition)
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
            const string RarityDefaultColor = "#000000";
            const string RarityCommonColor = "#000000";
            const string RarityMagicalColor = "#182e6f";
            const string RarityUncommonColor = "#520075";
            const string RarityRareColor = "#db9d00";

            string displayName = "<Empty>";
            Texture2D texture = null;
            string stats = String.Empty;

            CtArmorSetDef found = null;
            // EWeaponPrefix prefix = EWeaponPrefix.None;
            // EWeaponSuffix suffix = EWeaponSuffix.None;

            ulong data = _dataBlock.Value;
            if (CtDataBlock.IsValid(data))
            {
                EDataType dataType = CtDataBlock.GetDataType(data);
                if (dataType == EDataType.Equipment)
                {
                    int armorRating = 0;
                    string color = RarityDefaultColor;
                    ushort identifier = CtDataBlock.GetEquipmentIdentifier(data);
                    var armorDefs = 
                        Object.FindObjectsByType<CtArmorSetDef>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
                    found = armorDefs.Find(def => def.Identifier == identifier);
                    if (found)
                    {
                        var armorSlot = found.GetArmorSlot(AllowedArmorSlot);
                        if (armorSlot)
                        {
                            var rarity = found.Rarity;
                            switch (rarity)
                            {
                                case EItemRarity.None:
                                    color = RarityDefaultColor;
                                    break;
                                case EItemRarity.Common:
                                    color = RarityCommonColor;
                                    break;
                                case EItemRarity.Magical:
                                    color = RarityMagicalColor;
                                    break;
                                case EItemRarity.Uncommon:
                                    color = RarityUncommonColor;
                                    break;
                                case EItemRarity.Rare:
                                    color = RarityRareColor;
                                    break;
                                default:
                                    Debug.LogError($"Item rarity not supported (rarity={rarity}).");
                                    break;
                            }

                            displayName = $"<color={color}>{armorSlot.DisplayName}</color>";
                            texture = armorSlot.Icon;
                            armorRating = armorSlot.ArmorRating;
                        }

                        if (!texture)
                            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                                "Assets/CreatureTime/Worlds/CreatureTimeRPG/Editor/unknown.png");
                    }

                    stats += $"<color={color}>{displayName}</color>\nArmor: {armorRating}";
                    stats = stats.Trim();
                }
            }

            _icon.image = texture;
            _title.text = displayName;
            _stats.SetVisible(false);
            _stats.text = stats;

            _armorSelect.SetValueWithoutNotify(found);

            _title.tooltip = stats;
        }

        private void UpdateData()
        {
            var armorSetDef = (CtArmorSetDef)_armorSelect.value;
            if (!armorSetDef)
            {
                _dataBlock.Value = CtDataBlock.InvalidData;
                return;
            }

            ulong data = CtDataBlock.CreateEquipmentData(armorSetDef.Identifier);
            _dataBlock.Value = data;
        }

        public void Bind(SerializedObject serializedObject)
        {
            _dataBlock.DataBlockElement.Bind(serializedObject);
        }
    }
}