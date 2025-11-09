
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    public class CtDialogueGraphEditorWindow : EditorWindow
    {
        public static void Open(CtDialogueGraphAsset asset)
        {
            var window = CreateWindow<CtDialogueGraphEditorWindow>(typeof(CtDialogueGraphEditorWindow), typeof(SceneView));
            window.titleContent = new GUIContent($"{asset.name}", EditorGUIUtility.ObjectContent(asset, typeof(CtDialogueGraphAsset)).image);
            window.Load(asset);
        }

        [SerializeField] private CtDialogueGraphAsset _asset;
        private SerializedObject _serializedObject;
        private CtGraphView _graphView;
        private CtDialogueGraphModel _model;

        private void OnEnable()
        {
            if (!_asset) return;
            DrawGraph();
        }

        private void OnGUI()
        {
            if (!_asset) return;
            hasUnsavedChanges = EditorUtility.IsDirty(_asset);
        }

        public void Load(CtDialogueGraphAsset asset)
        {
            _asset = asset;
            DrawGraph();
        }

        private void DrawGraph()
        {
            _serializedObject = new SerializedObject(_asset);

            var generate = new Button
            {
                text = "Generate"
            };
            generate.clicked += CtDialogueGraphAsset.GenerateDialogue;
            rootVisualElement.Add(generate);

            _graphView = new CtGraphView(this);
            _graphView.graphViewChanged += OnGraphChanged;
            rootVisualElement.Add(_graphView);

            _model = new CtDialogueGraphModel
            {
                bindingPath = "graph"
            };
            _model.Bind(_serializedObject);
            _graphView.Model = _model;
        }

        private GraphViewChange OnGraphChanged(GraphViewChange graphViewChange)
        {
            EditorUtility.SetDirty(_asset);
            return graphViewChange;
        }
    }
}
