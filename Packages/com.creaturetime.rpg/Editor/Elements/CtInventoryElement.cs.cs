
using System;
using UnityEditor;
using UnityEngine.UIElements;
using VRC.SDK3.Editor;

namespace CreatureTime
{
    public class CtInventoryElement : VisualElement
    {
        private SerializedObject _serializedObject;
        private string _bindingPath = String.Empty;
        private EnumField _inventoryType;
        private CtAbstractItemStatsElement _currentDataBlock;

        private CtArmorStatsElement _armorStats;
        private CtWeaponStatsElement _weaponStats;
        private CtOffHandStatsElement _offHandStats;

        public string BindingPath
        {
            set
            {
                _bindingPath = value;
                _weaponStats.BindingPath = _bindingPath;
                _armorStats.BindingPath = _bindingPath;
                _offHandStats.BindingPath = _bindingPath;
            }
        }

        public CtInventoryElement()
        {
            _inventoryType = new EnumField
            {
                label = "Inventory Type"
            };
            _inventoryType.Init(EDataType.None);
            Add(_inventoryType);

            _inventoryType.RegisterValueChangedCallback(_UpdateInventoryType);

            _weaponStats = new CtWeaponStatsElement();
            _weaponStats.SetVisible(false);
            Add(_weaponStats);

            _armorStats = new CtArmorStatsElement();
            _armorStats.SetVisible(false);
            Add(_armorStats);

            _offHandStats = new CtOffHandStatsElement();
            _offHandStats.SetVisible(false);
            Add(_offHandStats);
        }

        private void _UpdateInventoryType(ChangeEvent<Enum> evt)
        {
            _UpdateBindings();
        }

        public void Bind(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;

            _weaponStats.Bind(_serializedObject);
            _armorStats.Bind(_serializedObject);
            _offHandStats.Bind(_serializedObject);

            var bindingProperty = _serializedObject.FindProperty(_bindingPath);
            ulong value = bindingProperty.ulongValue;
            var dataType = EDataType.None;
            if (value != CtDataBlock.InvalidData)
                dataType = CtDataBlock.GetDataType(value);
            _inventoryType.value = dataType;

            _UpdateBindings();
        }

        private void _UpdateBindings()
        {
            if (_currentDataBlock != null)
            {
                _currentDataBlock.SetVisible(false);
            }

            switch ((EDataType)_inventoryType.value)
            {
                case EDataType.None:
                    if (_currentDataBlock != null)
                        _currentDataBlock.DataBlock.Value = CtDataBlock.InvalidData;
                    _currentDataBlock = null;
                    break;
                case EDataType.Weapon:
                    _weaponStats.SetVisible(true);
                    _weaponStats.DataBlock.Value = CtDataBlock.InvalidData;
                    _currentDataBlock = _weaponStats;
                    break;
                case EDataType.Equipment:
                    _armorStats.SetVisible(true);
                    _armorStats.DataBlock.Value = CtDataBlock.InvalidData;
                    _currentDataBlock = _armorStats;
                    break;
                case EDataType.OffHand:
                    _offHandStats.SetVisible(true);
                    _offHandStats.DataBlock.Value = CtDataBlock.InvalidData;
                    _currentDataBlock = _offHandStats;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}