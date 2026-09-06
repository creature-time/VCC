
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
        private EnumField _armorSlot;

        private bool _forceArmorSlot;

        public bool ForceArmorSlot
        {
            set
            {
                _forceArmorSlot = value;
                _armorSlot.SetEnabled(!_forceArmorSlot);
            }
        }

        public EArmorSlot AllowedArmorSlot
        {
            get => (EArmorSlot)_armorSlot.value;
            set => _armorSlot.value = value;
        }

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

            _armorSlot = new EnumField
            {
                label = "Slot"
            };
            _armorSlot.SetEnabled(!_forceArmorSlot);
            _armorSlot.Init(EArmorSlot.Head);
            _container.Add(_armorSlot);

            _armorSelect.RegisterValueChangedCallback(evt =>
            {
                randomize.SetEnabled(evt.newValue);
                UpdateData();
            });

            _armorSlot.RegisterValueChangedCallback(evt =>
            {
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
            string displayName = "<Empty>";
            Texture2D texture = null;
            string stats = String.Empty;

            CtArmorSetDef found = null;
            EArmorSlot armorSlot = EArmorSlot.Head;
            // EWeaponPrefix prefix = EWeaponPrefix.None;
            // EWeaponSuffix suffix = EWeaponSuffix.None;

            ulong data = _dataBlock.Value;
            if (CtDataBlock.IsValid(data))
            {
                EDataType dataType = CtDataBlock.GetDataType(data);
                if (dataType == EDataType.Equipment)
                {
                    int armorRating = 0;
                    ushort identifier = CtDataBlock.GetEquipmentIdentifier(data);
                    var armorDefs = 
                        Object.FindObjectsByType<CtArmorSetDef>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
                    found = armorDefs.Find(def => def.Identifier == identifier);
                    if (found)
                    {
                        armorSlot = CtDataBlock.GetEquipmentSlot(data);
                        var armorSlotDef = found.GetArmorSlot(armorSlot);
                        if (armorSlotDef)
                        {
                            if (armorSlotDef.TryGetFormattedStats(out displayName, out stats))
                            {
                                texture = armorSlotDef.Icon;
                            }
                        }

                        if (!texture)
                            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                                "Assets/CreatureTime/Worlds/CreatureTimeRPG/Editor/unknown.png");
                    }
                }
            }

            _icon.image = texture;
            _title.text = displayName;
            _stats.text = stats;

            _armorSelect.SetValueWithoutNotify(found);
            _armorSlot.SetValueWithoutNotify(armorSlot);

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

            ulong data = CtDataBlock.CreateEquipmentData(armorSetDef.Identifier, AllowedArmorSlot);
            _dataBlock.Value = data;
        }

        public void Bind(SerializedObject serializedObject)
        {
            _dataBlock.DataBlockElement.Bind(serializedObject);
        }
    }
}