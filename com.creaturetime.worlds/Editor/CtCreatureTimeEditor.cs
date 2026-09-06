
using System;
using System.Linq;
using UnityEditor;
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
        public static void ShowWindow()
        {
            var wnd = GetWindow<CtCreatureTimeEditor>(typeof(CtCreatureTimeEditor), typeof(SceneView));
            wnd.titleContent =
                new GUIContent("Creature Time", (Texture2D)EditorGUIUtility.Load(DefaultWhiteX16));
        }

        private CtTabElement _tabElement;

        protected override void OnEnable()
        {
            _tabElement = new CtTabElement
            {
                style = { flexGrow = 1f }
            };
            rootVisualElement.Add(_tabElement);

            // Find all objects for each singleton type from all assemblies.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (Type typ in
                     assembly.GetTypes()
                         .Where(myType =>
                             myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(CtCreatureTimeSubEditor))))
            {
                var subEditor = (CtCreatureTimeSubEditor)Activator.CreateInstance(typ);
                _tabElement.AddTab(subEditor.Name, subEditor);
            }
        }
    }
}