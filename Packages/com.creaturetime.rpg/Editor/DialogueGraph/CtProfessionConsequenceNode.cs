
using System;
using CreatureTime.RpgGame;
using UnityEditor;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CtNodeInfo("Profession", "#5aa754", "Profession", false, false)]
    public class CtProfessionConsequenceNode : CtConsequenceNode
    {
        [CtExposedProperty, SerializeField] private ushort professionId;

        public override void Process(CtDialogueGraphAsset asset)
        {
            Debug.Log($"Profession: {professionId}");
            var recruitConsequence = asset.CreateConsequence<CtProfessionConsequence>();

            var so = new SerializedObject(recruitConsequence);
            so.FindProperty("professionDef").objectReferenceValue = asset.FindProfessionDef(professionId);
            so.ApplyModifiedProperties();
        }
    }
}
