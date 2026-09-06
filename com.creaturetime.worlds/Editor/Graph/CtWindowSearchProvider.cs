using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CreatureTime.Editor.Graph
{
    public struct SearchContextElement
    {
        public object Target { get; private set; }
        public string Title { get; private set; }

        public SearchContextElement(object target, string title)
        {
            Target = target;
            Title = title;
        }
    }

    public class CtWindowSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public CtGraphView view;

        public static List<SearchContextElement> elements;
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Dialogue Graph Nodes")));

            elements = new List<SearchContextElement>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attribute = type.GetCustomAttribute(typeof(CtNodeInfoAttribute));
                    if (attribute != null)
                    {
                        var attr = (CtNodeInfoAttribute)attribute;
                        var node = Activator.CreateInstance(type);

                        if (string.IsNullOrEmpty(attr.MenuItem)) continue;
                        elements.Add(new SearchContextElement(node, attr.MenuItem));
                    }
                }
            }

            // TODO: Sorting!

            foreach (var element in elements)
            {
                var entry = new SearchTreeEntry(new GUIContent(element.Title));
                entry.level = 1;
                entry.userData = new SearchContextElement(element.Target, element.Title);
                tree.Add(entry);
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowMousePosition = context.screenMousePosition - view.Window.position.position;
            var graphMousePosition = view.contentViewContainer.WorldToLocal(windowMousePosition);

            var element = (SearchContextElement)searchTreeEntry.userData;
            var node = (CtGraphNode)element.Target;
            node.Position = new Rect(graphMousePosition, new Vector2());
            view.Model.AddNode(node);

            return true;
        }
    }
}
