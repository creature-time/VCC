
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CreatureTime
{
     [CustomEditor(typeof(CtSkillDef), true)]
     public class CtSkillDefEditor : CtEditor
     {
         private Label _description;

         public override VisualElement CreateInspectorGUI()
         {
             var rootVisualElement = base.CreateInspectorGUI();

             _description = new Label
             {
                 style =
                 {
                     fontSize = 14,
                     whiteSpace = WhiteSpace.Normal,
                     marginTop = 8,
                     marginBottom = 8,
                     marginLeft = 8,
                     marginRight = 8,
                     paddingTop = 8,
                     paddingBottom = 8,
                     paddingLeft = 8,
                     paddingRight = 8,
                     borderTopWidth = 1,
                     borderBottomWidth = 1,
                     borderLeftWidth = 1,
                     borderRightWidth = 1,
                     borderTopColor = Color.black,
                     borderBottomColor = Color.black,
                     borderLeftColor = Color.black,
                     borderRightColor = Color.black,
                     borderBottomLeftRadius = 8,
                     borderBottomRightRadius = 8,
                     borderTopLeftRadius = 8,
                     borderTopRightRadius = 8,
                 }
             };
             rootVisualElement.Add(_description);

             Type fallbackEditorType = typeof(Editor).Assembly.GetType("UnityEditor.GenericInspector");
             VisualElement defaultElements = CreateEditor(targets, fallbackEditorType).CreateInspectorGUI();
             rootVisualElement.Add(defaultElements);

             UpdateDescription(serializedObject);

             // Whenever any serialized property on this serialized object changes its value, call CheckForWarnings.
             rootVisualElement.TrackSerializedObjectValue(serializedObject, UpdateDescription);

             return rootVisualElement;
         }

         private void UpdateDescription(SerializedObject _)
         {
             CtSkillDef skillDefinition = target as CtSkillDef;
             string description = skillDefinition.GetDescription(12);
             _description.text = description;
         }
     }
}
