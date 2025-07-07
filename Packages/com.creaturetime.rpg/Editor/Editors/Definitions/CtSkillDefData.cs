
using System;
using UnityEditor;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    [CreateAssetMenu(fileName = "skillDef_00000_unnamed", menuName = "CreatureTime/Rpg/Skill Definition", order = 1)]
    public class CtSkillDefData : CtAbstractDefData
    {
        public override string GenerateName => 
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override int Identifier => identifier;

        [SerializeField] public ushort identifier;
        [SerializeField] public MonoScript script;
        [SerializeField] public string displayName;
        [SerializeField] public Texture2D icon;
        [SerializeField] public CtAttributeDefData attributeType;
    }
}