using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace CreatureTime
{
    public class CtSingletonEditor : MonoBehaviour
    {
        public static Dictionary<Type, CtSingleton> GetCurrentSingletonTypes()
        {
            var singletons = new Dictionary<Type, CtSingleton>();

            // Find all objects for each singleton type from all assemblies.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (Type typ in
                     assembly.GetTypes()
                         .Where(myType =>
                             myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(CtSingleton))))
            {
                singletons.Add(typ, (CtSingleton)FindFirstObjectByType(typ, FindObjectsInactive.Include));
            }

            return singletons;
        }

        public static void AssignSingletons(Dictionary<Type, CtSingleton> singletons, GameObject root = null)
        {
            // Throw error if singleton does not exist in the scene.
            foreach (var pair in singletons)
                if (!pair.Value)
                    Debug.LogWarning($"Failed to find singleton for type ({pair.Key})");

            // Find all the components and their fields and set the value of the singleton if the type is the
            // singleton.
            UdonSharpBehaviour[] components;
            components = root ? 
                root.GetComponentsInChildren<UdonSharpBehaviour>(true) : 
                FindObjectsOfType<UdonSharpBehaviour>(true);

            foreach (var component in components)
            {
                var type = component.GetType();
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).ToList();
                do
                {
                    foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                        fields.Add(fieldInfo);
                    type = type.BaseType;
                } while (type != null && type != typeof(UdonSharpBehaviour));

                foreach (var fieldInfo in fields)
                {
                    if (!singletons.TryGetValue(fieldInfo.FieldType, out var singleton))
                        continue;

                    fieldInfo.SetValue(component, singleton);

                    EditorUtility.SetDirty(component);
                }
            }
        }
    }
}