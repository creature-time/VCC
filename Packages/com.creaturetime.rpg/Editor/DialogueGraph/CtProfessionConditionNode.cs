
using System;
using CreatureTime.RpgGame;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Check Profession", "#a7545a", "Check Profession", false, false)]
    public class CtProfessionConditionNode : CtConditionNode
    {
        [CtExposedProperty, SerializeField] private ushort professionId;
        [CtExposedProperty, SerializeField] private bool isProfession;

        public override void Process(CtDialogueGraphAsset asset)
        {
            Debug.Log($"Profession: {isProfession} {professionId}");
            var recruitCondition = asset.CreateCondition<CtProfessionCondition>();

            var so = new SerializedObject(recruitCondition);
            so.FindProperty("professionDef").objectReferenceValue = asset.FindProfessionDef(professionId);
            so.FindProperty("isProfession").boolValue = isProfession;
            so.ApplyModifiedProperties();
        }
    }
}
