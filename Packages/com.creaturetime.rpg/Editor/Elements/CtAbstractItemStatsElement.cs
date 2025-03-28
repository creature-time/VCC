
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Editor;

namespace CreatureTime
{
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
            _dataBlock.SetVisible(false);
            _container.Add(_dataBlock);
        }

        public void Bind(SerializedObject serializedObject)
        {
            _dataBlock.DataBlockElement.Bind(serializedObject);
        }
    }
}