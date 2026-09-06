
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
        [CtExposedProperty, SerializeField] private CtProfessionDefData profession;
        [CtExposedProperty, SerializeField] private bool isProfession;

        public CtProfessionDef FindProfessionDef(ushort professionId)
        {
            var definitions = GameObject.FindObjectsOfType<CtProfessionDef>(true);
            foreach (var definition in definitions)
            {
                if (definition.Identifier == professionId)
                    return definition;
            }

            return null;
        }

        public override void Process(CtDialogueGraphAsset asset)
        {
            var recruitCondition = asset.CreateCondition<CtProfessionCondition>();

            var so = new SerializedObject(recruitCondition);
            so.FindProperty("professionDef").objectReferenceValue = FindProfessionDef(profession.Identifier);
            so.FindProperty("isProfession").boolValue = isProfession;
            so.ApplyModifiedProperties();
        }
    }
}
