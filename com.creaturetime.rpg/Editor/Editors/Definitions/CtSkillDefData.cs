using System;
using System.Collections.Generic;
using CreatureTime.RpgGame;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreatureTime
{
    [Serializable]
    [CreateAssetMenu(fileName = "skillDef_00000_unnamed", menuName = "CreatureTime/Rpg/Skill Definition", order = 1)]
    public class CtSkillDefData : CtAbstractDefData
    {
        public override string GenerateName =>
            $"{identifier:00000}_{(string.IsNullOrEmpty(displayName) ? "NoName" : displayName.Replace(' ', '-'))}";

        public override ushort Identifier => identifier;

        [SerializeField] public ushort identifier;
        [SerializeField] public MonoScript script;
        [SerializeField] public string displayName;
        [SerializeField] public Texture2D icon;
        [SerializeField] public bool isWeaponSkill;
        [SerializeField] public CtAttributeDefData attributeType;
        [SerializeField] public ETargetType targetType;
        [SerializeField] public ESkillSubType subType;
        [SerializeField] public bool isBeneficial;
        [SerializeField] public ESkillType skillType;
        [SerializeField] public int cost;
        [SerializeField] public int rechargeTime;
    }

    // public class CtSkillDefDepNode : CtAbstractDepNode
    // {
    //     public override int Identifier => typeof(CtSkillDefData).GetHashCode();
    //     public override int[] Dependencies => new[] { typeof(CtAttributeDefData).GetHashCode() };
    //
    //     public override bool Process(Dictionary<object, object> context)
    //     {
    //         var gameData = Object.FindFirstObjectByType<CtGameData>();
    //         if (!gameData)
    //             return false;
    //
    //         if (!context.TryGetValue(nameof(CtSkillDefData), out var skillDefsObj)) return false;
    //         if (!context.TryGetValue(nameof(CtAttributeDef), out var attributeDefsObj)) return false;
    //
    //         var skillDefDataList = skillDefsObj as List<CtSkillDefData>;
    //         var attributeDefLookUp = attributeDefsObj as Dictionary<CtAttributeDefData, CtAttributeDef>;
    //
    //         skillDefDataList.Sort((a, b) => a.identifier.CompareTo(b.identifier));
    //
    //         var definitions = new List<CtSkillDef>();
    //         for (int i = 0; i < skillDefDataList.Count; i++)
    //         {
    //             var data = skillDefDataList[i];
    //             if (!data.attributeType)
    //             {
    //                 Debug.LogWarning("Skill did not have attribute type.");
    //                 continue;
    //             }
    //
    //             if (!data.script)
    //             {
    //                 Debug.LogWarning("Skill did not have script type.");
    //                 continue;
    //             }
    //
    //             var gameObject = new GameObject(data.GenerateName);
    //             gameObject.transform.SetParent(attributeDefLookUp[data.attributeType].transform);
    //
    //             var def = AddUdonSharpComponentWithUdonBehavior<CtSkillDef>(gameObject, data.script.GetClass());
    //             var so = new SerializedObject(def);
    //
    //             so.FindProperty("identifier").intValue = data.identifier;
    //             so.FindProperty("displayName").stringValue = data.displayName;
    //             so.FindProperty("icon").objectReferenceValue = data.icon;
    //             so.FindProperty("isWeaponSkill").boolValue = data.isWeaponSkill;
    //             so.FindProperty("attributeType").intValue = data.attributeType.identifier;
    //             so.FindProperty("targetType").intValue = Convert.ToInt32(data.targetType);
    //             so.FindProperty("subType").enumValueIndex = Convert.ToInt32(data.subType);
    //             so.FindProperty("isBeneficial").boolValue = data.isBeneficial;
    //             so.FindProperty("skillType").intValue = Convert.ToInt32(data.skillType);
    //             so.FindProperty("cost").intValue = data.cost;
    //             so.FindProperty("rechargeTime").intValue = data.rechargeTime;
    //
    //             so.ApplyModifiedPropertiesWithoutUndo();
    //
    //             definitions.Add(def);
    //         }
    //
    //         var conditionsGroup = gameData.transform.Find("Conditions");
    //         foreach (var condition in conditionsGroup.GetComponentsInChildren<CtSkillDef>(true))
    //             definitions.Add(condition);
    //
    //         var soGameData = new SerializedObject(gameData);
    //
    //         var dataProp = soGameData.FindProperty("skillDefinitions");
    //         dataProp.arraySize = definitions.Count;
    //         for (int i = 0; i < definitions.Count; i++)
    //             dataProp.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
    //
    //         soGameData.ApplyModifiedProperties();
    //
    //         CtSkillDefFuncs.AssignSkillFlags();
    //
    //         return true;
    //     }
    //
    //     private T AddUdonSharpComponentWithUdonBehavior<T>(GameObject gameObject)
    //         where T : UdonSharpBehaviour
    //     {
    //         return (T)AddUdonSharpComponentWithUdonBehavior(gameObject, typeof(T));
    //     }
    //
    //     private T AddUdonSharpComponentWithUdonBehavior<T>(GameObject gameObject, Type type)
    //         where T : UdonSharpBehaviour
    //     {
    //         return (T)AddUdonSharpComponentWithUdonBehavior(gameObject, type);
    //     }
    //
    //     private UdonSharpBehaviour AddUdonSharpComponentWithUdonBehavior(GameObject gameObject, Type type)
    //     {
    //         return gameObject.AddUdonSharpComponent(type);
    //     }
    // }
}