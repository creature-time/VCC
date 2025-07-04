
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CreatureTime
{
    public abstract class CtCreatureTimeSubEditor : VisualElement
    {
        public abstract string Name { get; }
    }

    public class CtCreatureTimeEditor : CtAbstractEditorWindow
    {
        [MenuItem("CreatureTime/Editor")]
        public static void ShowExample()
        {
            var wnd = GetWindow<CtCreatureTimeEditor>();
            wnd.titleContent = new GUIContent("Creature Time");
        }

        private Toolbar _toolbar;
        private VisualElement _content;

        private protected override void SetUp()
        {
            _toolbar = new Toolbar();
            rootVisualElement.Add(_toolbar);

            _content = new VisualElement
            {
                style =
                {
                    flexGrow = 1.0f
                }
            };
            rootVisualElement.Add(_content);

            List<CtCreatureTimeSubEditor> subEditors = new List<CtCreatureTimeSubEditor>();

            // Find all objects for each singleton type from all assemblies.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (Type typ in
                     assembly.GetTypes()
                         .Where(myType =>
                             myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(CtCreatureTimeSubEditor))))
            {
                var subEditor = (CtCreatureTimeSubEditor)Activator.CreateInstance(typ);
                AddTab(subEditor.Name, subEditor);
            }
        }

        public void AddTab(string text, VisualElement element)
        {
            element.style.flexGrow = 1.0f;

            var tab = new Button { text = text };
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