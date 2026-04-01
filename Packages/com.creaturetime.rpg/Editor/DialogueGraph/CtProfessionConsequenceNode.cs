
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
        [CtExposedProperty, SerializeField] private CtProfessionDefData profession;

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
            Debug.Log($"Profession: {profession}");
            var recruitConsequence = asset.CreateConsequence<CtProfessionConsequence>();

            var so = new SerializedObject(recruitConsequence);
            so.FindProperty("professionDef").objectReferenceValue = FindProfessionDef(profession.Identifier);
            so.ApplyModifiedProperties();
        }
    }
}
