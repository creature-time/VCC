
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CreatureTime
{
    public class CtTabElement : VisualElement
    {
        private Toolbar _toolbar;
        private VisualElement _content;

        public CtTabElement()
        {
            _toolbar = new Toolbar
            {
                style =
                {
                    paddingTop = 2,
                    paddingBottom = 0,
                    height = 24
                }
            };
            Add(_toolbar);

            _content = new VisualElement
            {
                style =
                {
                    flexGrow = 1.0f
                }
            };
            Add(_content);
        }

        public void AddTab(string text, VisualElement element)
        {
            element.style.flexGrow = 1.0f;

            var tab = new Button
            {
                text = text,
                style =
                {
                    borderBottomWidth = 0,
                    height = 20,
                    fontSize = 14,
                    paddingBottom = 8,
                    paddingTop = 8,
                    paddingLeft = 8,
                    paddingRight = 8,
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 0,
                    borderBottomRightRadius = 0,
                }
            };
            tab.userData = element;
            tab.clicked += () => { _AssignTab(element); };
            _toolbar.Add(tab);

            if (_currentElement == null)
                _AssignTab(element);
        }

        private VisualElement _currentElement;

        private void _AssignTab(VisualElement element)
        {
            if (_currentElement != null)
            {
                _content.Remove(_currentElement);
            }

            _currentElement = element;
            if (_currentElement != null)
            {
                _content.Add(_currentElement);
            }
        }
    }
}