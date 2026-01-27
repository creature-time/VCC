
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [CustomEditor(typeof(CtDialogueGraphAsset))]
    public class CtDialogueGraphEditor : UnityEditor.Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            Debug.LogWarning(EditorUtility.InstanceIDToObject(instanceID));
            var asset = (CtDialogueGraphAsset)EditorUtility.InstanceIDToObject(instanceID);
            if (asset.GetType() == typeof(CtDialogueGraphAsset))
            {
                CtDialogueGraphEditorWindow.Open(asset);
                return true;
            }

            return false;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            var open = new Button
            {
                text = "Open"
            };
            open.clicked += () => CtDialogueGraphEditorWindow.Open((CtDialogueGraphAsset)target);
            root.Add(open);

            var generate = new Button
            {
                text = "Generate"
            };
            generate.clicked += CtDialogueGraphAsset.GenerateDialogue;
            root.Add(generate);

            var conversationIdField = new PropertyField(serializedObject.FindProperty("conversationId"))
            {
                label = "Conversation ID",
            };
            root.Add(conversationIdField);

            return root;
        }
    }
}
