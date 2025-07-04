
using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    [CreateAssetMenu(fileName = "professionData", menuName = "CreatureTime/Rpg/Profession Definition", order = 1)]
    public class CtProfessionDefData : ScriptableObject
    {
        [SerializeField] public ushort identifier;
        [SerializeField] public string displayName;
        [SerializeField] public CtAttributeDefData attributes1;
        [SerializeField] public CtAttributeDefData attributes2;
        [SerializeField] public CtAttributeDefData attributes3;
        [SerializeField] public CtAttributeDefData attributes4;
        [SerializeField] public CtAttributeDefData attributes5;
        [SerializeField] public Color theme = Color.black;
    }
}