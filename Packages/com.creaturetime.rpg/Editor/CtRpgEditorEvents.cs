
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;
using VRC.SDKBase.Editor.Api;
using Object = UnityEngine.Object;

namespace CreatureTime
{
    public static class CtRpgEditorEvents
    {
        private static VRCWorld _GetWorld()
        {
            VRCWorld worldData;

            var sceneDescriptor = VRC_SceneDescriptor.Instance;
            var pipelineManager = sceneDescriptor.GetComponent<PipelineManager>();
            if (String.IsNullOrEmpty(pipelineManager.blueprintId))
            {
                worldData = new VRCWorld();
                worldData.Capacity = 32;
                worldData.RecommendedCapacity = 16;
            }
            else
            {
                var task = VRCApi.GetWorld(pipelineManager.blueprintId, true);
                Task.Run(() => task).Wait();
                worldData = task.Result;
            }

            return worldData;
        }

        [MenuItem("CreatureTime/Rpg/Update Skills")]
        private static void UpdateSkills()
        {
            Dictionary<ECombatEffectFlags, string> methodFlags = new Dictionary<ECombatEffectFlags, string>
            {
                { ECombatEffectFlags.Use, "OnUse" },
                { ECombatEffectFlags.PersistentEffect, "OnPersistentEffect" },
                { ECombatEffectFlags.SkillUsedEffect, "OnSkillUsed" },
                { ECombatEffectFlags.TickEffect, "OnTickEffect" },
            };

            CtSkillDef[] skillDefinitions =
                GameObject.FindObjectsByType<CtSkillDef>(FindObjectsSortMode.None);
            foreach (CtSkillDef skillDefinition in skillDefinitions)
            {
                var serializedObject = new SerializedObject(skillDefinition);
                var flagsProp = serializedObject.FindProperty("flags");

                var flags = ECombatEffectFlags.None;

                foreach (KeyValuePair<ECombatEffectFlags, string> entry in methodFlags)
                {
                    MethodInfo methodInfo = skillDefinition.GetType().GetMethod(entry.Value);
                    if (methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType)
                    {
                        flags |= entry.Key;
                    }
                }

                flagsProp.enumValueFlag = Convert.ToInt32(flags);
                serializedObject.ApplyModifiedProperties();

                Debug.Log($"[EnterPlaymode] {skillDefinition.DisplayName} set flags to {skillDefinition.Flags}");
            }
        }

        [MenuItem("CreatureTime/Rpg/Update Player Render Targets")]
        private static void UpdatePlayerRenderTargets()
        {
            var worldData = _GetWorld();

            string renderTexturesDir =
                "Packages/com.creaturetime.rpg/Resources/RenderTextures";
            string generatedDir = "Generated";
            string generatedRenderTexturesDir = $"{renderTexturesDir}/{generatedDir}";

            string templatePath = null;

            string[] assets = AssetDatabase.FindAssets("PlayerSlotTemplate t:RenderTexture",
                new string[] { renderTexturesDir });
            if (assets.Length > 0)
            {
                templatePath = AssetDatabase.GUIDToAssetPath(assets[0]);
            }

            if (String.IsNullOrEmpty(templatePath))
            {
                Debug.LogError("Render texture template was not found!");
                return;
            }

            if (!AssetDatabase.IsValidFolder(generatedRenderTexturesDir))
                AssetDatabase.CreateFolder(renderTexturesDir, generatedDir);

            assets = AssetDatabase.FindAssets("t:RenderTexture", new string[] { generatedRenderTexturesDir });

            int playerCount = worldData.Capacity;
            for (int i = playerCount; i < assets.Length; i++)
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(assets[i]));

            for (int i = 0; i < playerCount; i++)
                AssetDatabase.CopyAsset(templatePath, $"{generatedRenderTexturesDir}/PlayerSlot{i}.renderTexture");

            assets = AssetDatabase.FindAssets("t:RenderTexture", new string[] { generatedRenderTexturesDir });
            var renderTextures = new RenderTexture[assets.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assets[i]);
                renderTextures[i] = AssetDatabase.LoadAssetAtPath<RenderTexture>(assetPath);
            }

            var instance = (CtPlayerManager)Object.FindObjectOfType(typeof(CtPlayerManager));
            SerializedObject serializedObject = new SerializedObject(instance);
            SerializedProperty playerRenderTexturesProp = serializedObject.FindProperty("playerRenderTextures");
            playerRenderTexturesProp.arraySize = renderTextures.Length;
            for (int i = 0; i < renderTextures.Length; i++)
            {
                SerializedProperty arrayIndexProp = playerRenderTexturesProp.GetArrayElementAtIndex(i);
                arrayIndexProp.objectReferenceValue = renderTextures[i];
            }

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CreatureTime/Rpg/Assign Unique Ids to All Sequence Nodes")]
        private static void AssignUniqueIdsToAllSequenceNodes()
        {
            var sequenceNodes = GameObject.FindObjectsOfType<CtSequenceNode>();
            foreach (var sequenceNode in sequenceNodes)
            {
                var so = new SerializedObject(sequenceNode);
                var path = sequenceNode.transform._GetFullPath();
                so.FindProperty("identifier").stringValue = path;
                so.ApplyModifiedProperties();
            }
        }

        [MenuItem("CreatureTime/Rpg/Update All")]
        private static void UpdateAll()
        {
            UpdateSkills();
            UpdatePlayerRenderTargets();
            AssignUniqueIdsToAllSequenceNodes();
        }

        private static string _GetFullPath(this Transform tr)
        {
            var parents = tr.GetComponentsInParent<Transform>();

            var results = string.Empty;
            if (parents.Length > 0)
            {
                results += parents[0].name;
                for (int i = parents.Length - 2; i >= 0; i--)
                    results += "/" + parents[i].name;
            }

            return results;
        }
    }
}