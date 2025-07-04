
using UnityEditor;
using UnityEngine.UIElements;

namespace CreatureTime
{
    public abstract class CtAbstractEditorWindow : EditorWindow
    {
        private const string StyleSheetFilePath = 
            "Packages/com.creaturetime.worlds/Editor/CtStyleSheet.uss";

        private void OnEnable()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetFilePath);
            rootVisualElement.styleSheets.Add(styleSheet);

            SetUp();
        }

        private protected abstract void SetUp();
    }
}