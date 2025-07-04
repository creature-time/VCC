
using UnityEditor;
using UnityEngine.UIElements;

namespace CreatureTime
{
    public abstract class CtAbstractElement : Editor
    {
        private const string StyleSheetFilePath = 
            "Packages/com.creaturetime.worlds/Editor/CtStyleSheet.uss";

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement rootVisualElement = new VisualElement();

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetFilePath);
            rootVisualElement.styleSheets.Add(styleSheet);

            return rootVisualElement;
        }
    }
}