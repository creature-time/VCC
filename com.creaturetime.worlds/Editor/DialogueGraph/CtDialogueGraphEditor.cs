
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [CustomEditor(typeof(CtDialogueGraphAsset))]
    public class CtDialogueGraphEditor : UnityEditor.Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID);
            if (asset.GetType() != typeof(CtDialogueGraphAsset)) return false;

            CtDialogueGraphEditorWindow.Open((CtDialogueGraphAsset)asset);
            return true;
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
