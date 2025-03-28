
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VRC.SDK3.Editor;

namespace CreatureTime
{
    public class CtDataBlockElement : VisualElement
    {
        private const string UnlockedUnicode = "Unlocked";
        private const string LockedUnicode = "Locked";

        private HelpBox _helpBox;
        private TextField _hexDisplay;
        private LongField _dataBlock;

        public LongField DataBlockElement => _dataBlock;

        public ulong Value
        {
            get => (ulong)_dataBlock.value;
            set => _dataBlock.value = (long)value;
        }

        public string Label
        {
            set => _dataBlock.label = value;
        }

        public string BindingPath
        {
            set => _dataBlock.bindingPath = value;
        }

        public CtDataBlockElement()
        {
            _helpBox = new HelpBox
            {
                messageType = HelpBoxMessageType.Info,
                text = "Data is currently empty."
            };
            Add(_helpBox);

            _dataBlock = new LongField
            {
                label = "Attribute Data",
                bindingPath = "attributeData",
                isReadOnly = true
            };
            Add(_dataBlock);

            _hexDisplay = new TextField
            {
                label = "Hex",
                isReadOnly = true
            };
            Add(_hexDisplay);

            Toggle toggleReadOnly = new Toggle
            {
                text = _dataBlock.isReadOnly ? LockedUnicode : UnlockedUnicode,
                value = _dataBlock.isReadOnly,
                style = { minWidth = 128, maxWidth = 128 }
            };
            _dataBlock.Add(toggleReadOnly);

            toggleReadOnly.RegisterValueChangedCallback(evt =>
            {
                toggleReadOnly.text = evt.newValue ? LockedUnicode : UnlockedUnicode;
                _dataBlock.isReadOnly = evt.newValue;
            });

            _dataBlock.RegisterValueChangedCallback(evt =>
            {
                ulong value = (ulong)evt.newValue;
                _helpBox.SetVisible(!CtDataBlock.IsValid(value));
                _hexDisplay.value = $"0x{value:x16}";
            });
        }

        public void Bind(SerializedObject serializedObject)
        {
            _dataBlock.Bind(serializedObject);
        }
    }
}