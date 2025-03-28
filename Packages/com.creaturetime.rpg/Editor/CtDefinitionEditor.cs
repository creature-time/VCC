
using UnityEditor;
using UnityEngine;

namespace CreatureTime
{
    public class CtDefinitionEditor : CtEditorWindow
    {
        [MenuItem("CreatureTime/Definition Editor")]
        public static void ShowExample()
        {
            CtDefinitionEditor wnd = GetWindow<CtDefinitionEditor>();
            wnd.titleContent = new GUIContent("Definition Editor");
        }
    }
}