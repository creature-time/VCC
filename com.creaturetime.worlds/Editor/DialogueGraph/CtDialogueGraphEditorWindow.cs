
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    public class CtOverlayPanel : VisualElement
    {
        public CtOverlayPanel()
        {
                        // Create panel
            name = "OverlayPropertyEditor";

            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.creaturetime.worlds/Editor/DialogueGraph/USS/OverlayPropertyEditor.uss");
            styleSheets.Add(style);

            this.style.top = 8;
            this.style.right = 8;
        }

        public void AddSection(string title)
        {
            // Add sample content
            var header = new Box();
            header.AddToClassList("unity-box");
            header.name = "OverlayPropertyEditor-Header";
            Add(header);
            header.Add(new Label(title));
        }
    }

    public class CtDialogueGraphEditorWindow : CtAbstractEditorWindow
    {
        public static void Open(CtDialogueGraphAsset asset)
        {
            // Find previously opened window and bring to front.
            var windows = Resources.FindObjectsOfTypeAll<CtDialogueGraphEditorWindow>();
            foreach (var w in windows)
            {
                if (w._asset != asset) continue;

                w.Focus();
                return;
            }

            var window = CreateWindow<CtDialogueGraphEditorWindow>(typeof(CtDialogueGraphEditorWindow), typeof(SceneView));
            window.titleContent = new GUIContent(asset.name, EditorGUIUtility.ObjectContent(asset, typeof(CtDialogueGraphAsset)).image);
            window.Load(asset);
        }

        [SerializeField] private CtDialogueGraphAsset _asset;

        private SerializedObject _serializedObject;
        private VisualElement _graphWrapper;
        private CtGraphView _graphView;
        private CtDialogueGraphModel _model;
        private ListView _actorsView;
        private CtOverlayPanel _overlayPanel;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!_asset) return;
            DrawGraph();
        }

        private void OnGUI()
        {
            if (!_asset) return;
            hasUnsavedChanges = EditorUtility.IsDirty(_asset);
        }

        private void Load(CtDialogueGraphAsset asset)
        {
            _asset = asset;
            DrawGraph();
        }

        private List<CtActor> _actors = new List<CtActor>();

        private void DrawGraph()
        {
            _actors.Clear();
            _serializedObject = new SerializedObject(_asset);

            var generate = new Button
            {
                text = "Generate"
            };
            generate.clicked += CtDialogueGraphAsset.GenerateDialogue;
            rootVisualElement.Add(generate);

            _graphWrapper = new VisualElement();
            _graphWrapper.style.flexGrow = 1;
            rootVisualElement.Add(_graphWrapper);

            _graphView = new CtGraphView(this);
            _graphView.graphViewChanged += OnGraphChanged;
            _graphWrapper.Add(_graphView);

            _model = new CtDialogueGraphModel
            {
                bindingPath = "graph"
            };
            _model.Bind(_serializedObject);
            _graphView.Model = _model;

            _overlayPanel = new CtOverlayPanel
            {
                style =
                {
                    top = 8,
                    right = 8
                }
            };
            _overlayPanel.visible = false;
            _graphWrapper.Add(_overlayPanel);

            // _graphView.RegisterCallback<ClickEvent>(evt =>
            // {
            //     _overlayPanel.Clear();
            //
            //     var nodes = _graphView.selection.FindAll(s => s is CtGraphEditorNode).ToArray();
            //     var hasNode = nodes.Length > 0;
            //     _overlayPanel.visible = hasNode;
            //     if (!hasNode) return;
            //
            //     // TODO: Preview based on type.
            //     // var dialogueNode = Array.Find(nodes, n => n.NodeProperty.managedReferenceValue is CtDialogueNode);
            //     // if (dialogueNode is not null)
            //     // {
            //     //     _overlayPanel.AddSection("Dialogue");
            //     // }
            // });
        }

        private GraphViewChange OnGraphChanged(GraphViewChange graphViewChange)
        {
            EditorUtility.SetDirty(_asset);
            return graphViewChange;
        }
    }
}
