
using UnityEditor;
using UnityEngine.UIElements;

namespace CreatureTime
{
    public abstract class CtAbstractEditorWindow : EditorWindow
    {
        private const string StyleSheetFilePath = 
            "Packages/com.creaturetime.worlds/Editor/CtStyleSheet.uss";

        public const string DefaultWhiteX16 = "Packages/com.creaturetime.worlds/Editor/Resources/default_white_x16.png";
        public const string DefaultWhiteX24 = "Packages/com.creaturetime.worlds/Editor/Resources/default_white_x24.png";
        public const string DefaultWhiteX32 = "Packages/com.creaturetime.worlds/Editor/Resources/default_white_x32.png";
        public const string DefaultWhiteX64 = "Packages/com.creaturetime.worlds/Editor/Resources/default_white_x64.png";
        public const string DefaultWhiteX128 = "Packages/com.creaturetime.worlds/Editor/Resources/default_white_x128.png";

        private void OnEnable()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetFilePath);
            rootVisualElement.styleSheets.Add(styleSheet);

            SetUp();
        }

        private protected abstract void SetUp();
    }
}