
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
    public class CtWeaponStatsElement : CtAbstractItemStatsElement
    {
        private ObjectField _weaponSelect;
        private EnumField _rarity;

        private SliderInt _levelReq;
        // private EnumField _prefix;
        // private EnumField _suffix;

        public string Label
        {
            set => _dataBlock.Label = value;
        }

        public CtWeaponStatsElement()
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

            _weaponSelect = new ObjectField
            {
                label = "Weapon",
                objectType = typeof(CtWeaponDef)
            };
            _container.Add(_weaponSelect);

            _rarity = new EnumField
            {
                label = "Rarity"
            };
            _rarity.Init(EItemRarity.None);
            _container.Add(_rarity);

            _levelReq = new SliderInt(0, 13)
            {
                label = "Requirement"
            };
            _container.Add(_levelReq);

            // _prefix = new EnumField
            // {
            //     label = "Prefix"
            // };
            // _prefix.Init(EWeaponPrefix.None);
            // _container.Add(_prefix);
            //
            // _suffix = new EnumField
            // {
            //     label = "Suffix"
            // };
            // _suffix.Init(EWeaponSuffix.None);
            // _container.Add(_suffix);

            _rarity.RegisterValueChangedCallback(_ => UpdateData());
            _weaponSelect.RegisterValueChangedCallback(evt =>
            {
                randomize.SetEnabled(evt.newValue);
                UpdateData();
            });
            randomize.clicked += () =>
            {
                CtWeaponDef weaponDefinition = _weaponSelect.value as CtWeaponDef;
                if (!weaponDefinition)
                {
                    Debug.LogError("There was no weapon definition to generation weapon.");
                    return;
                }

                _dataBlock.Value = weaponDefinition.GenerateWeapon();
            };
            _levelReq.RegisterValueChangedCallback(evt =>
            {
                _levelReq.label = $"Requirement [{evt.newValue}]";
                UpdateData();
            });
            // _suffix.RegisterValueChangedCallback(_ => UpdateData());
            _dataBlock.DataBlockElement.RegisterValueChangedCallback(_ => SetupFields());

            clearButton.clicked += () => { _dataBlock.Value = CtDataBlock.InvalidData; };

            SetupFields();
        }

        private void SetupFields()
        {
            string displayName = "<Empty>";
            Texture2D texture = null;
            string stats = String.Empty;

            CtWeaponDef found = null;
            EItemRarity rarity = EItemRarity.None;
            int requirement = -1;
            // EWeaponPrefix prefix = EWeaponPrefix.None;
            // EWeaponSuffix suffix = EWeaponSuffix.None;

            ulong data = _dataBlock.Value;
            if (CtDataBlock.IsValid(data))
            {
                EDataType dataType = CtDataBlock.GetDataType(data);
                if (dataType == EDataType.Weapon)
                {
                    ushort identifier = CtDataBlock.GetWeaponIdentifier(data);
                    List<CtWeaponDef> weaponDefinitions =
                        GameObject.FindObjectsOfType<CtWeaponDef>().ToList();
                    found = weaponDefinitions.Find(definition => definition.Identifier == identifier);
                    if (found)
                    {
                        found.GetFormattedStats(data, ref displayName, ref stats, ref rarity, ref requirement);
                        stats = $"{displayName}\n{stats}";

                        texture = found.Icon as Texture2D;
                        if (!texture)
                            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                                "Assets/CreatureTime/Worlds/CreatureTimeRPG/Editor/unknown.png");
                    }
                }
            }

            _icon.image = texture;
            _title.text = displayName;
            _stats.SetVisible(false);
            _stats.text = stats;

            _weaponSelect.SetValueWithoutNotify(found);
            _rarity.SetValueWithoutNotify(rarity);
            _levelReq.SetValueWithoutNotify(requirement);
            // _prefix.SetValueWithoutNotify(prefix);
            // _suffix.SetValueWithoutNotify(suffix);

            _title.tooltip = stats;
        }

        private void UpdateData()
        {
            CtWeaponDef weaponDefinition = _weaponSelect.value as CtWeaponDef;
            ushort identifier = 0xFFFF;
            if (weaponDefinition)
                identifier = weaponDefinition.Identifier;

            ulong data = CtDataBlock.CreateWeaponData(
                identifier,
                // (EWeaponPrefix)_prefix.value,
                // (EWeaponSuffix)_suffix.value,
                EWeaponPrefix.None,
                EWeaponSuffix.None,
                _levelReq.value,
                (EItemRarity)_rarity.value);
            _dataBlock.Value = data;
        }

        public void Bind(SerializedObject serializedObject)
        {
            _dataBlock.DataBlockElement.Bind(serializedObject);
        }
    }
}