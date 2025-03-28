
using UnityEditor;
using UnityEngine.UIElements;

namespace CreatureTime
{
    public class CtEditorWindow : EditorWindow
    {
        private const string StyleSheetFilePath = 
            "Packages/com.creaturetime.worlds/Editor/CtStyleSheet.uss";

        private void OnEnable()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetFilePath);
            rootVisualElement.styleSheets.Add(styleSheet);
        }
    }
}