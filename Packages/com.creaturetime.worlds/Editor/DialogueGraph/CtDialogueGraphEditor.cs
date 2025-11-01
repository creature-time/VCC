
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [CustomEditor(typeof(CtDialogueGraphAsset))]
    public class CtDialogueGraphEditor : UnityEditor.Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
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
            generate.clicked += () =>
            {
                var asset = (CtDialogueGraphAsset)target;
                asset.Init();
                asset.Process();
            };
            root.Add(generate);

            return root;
        }
    }
}
