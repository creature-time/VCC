
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UdonSharp;
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

        [MenuItem("CreatureTime/Rpg/Update Skills", false, 0)]
        private static void _UpdateSkills()
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

        private static void _UpdateRenderTargets(int capacity)
        {
            const string renderTextureTemplate =
                "Packages/com.creaturetime.rpg/Resources/AvatarRenderTextureTemplate.renderTexture";

            string[] generatedDir = "CreatureTime/_Generated/AvatarRenderTextures".Split('/');
            string generatedPath = "Assets";
            foreach (var dir in generatedDir)
            {
                string subPath = $"{generatedPath}/{dir}";
                if (!AssetDatabase.IsValidFolder(subPath))
                    AssetDatabase.CreateFolder(generatedPath, dir);
                generatedPath = subPath;
            }

            string[] assets = AssetDatabase.FindAssets("t:RenderTexture", new string[] { generatedPath });
            for (int i = capacity; i < assets.Length; i++)
            {
                string guid = AssetDatabase.GUIDToAssetPath(assets[i]);
                if (!String.IsNullOrEmpty(guid))
                    AssetDatabase.DeleteAsset(guid);
            }

            for (int i = 0; i < capacity; i++)
                AssetDatabase.CopyAsset(renderTextureTemplate, 
                    $"{generatedPath}/AvatarRenderTexture_{i:0000}.renderTexture");

            assets = AssetDatabase.FindAssets("t:RenderTexture", new string[] { generatedPath });
            var renderTextures = new RenderTexture[assets.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assets[i]);
                renderTextures[i] = AssetDatabase.LoadAssetAtPath<RenderTexture>(assetPath);
            }

            var instance = (CtPlayerManager)Object.FindObjectOfType(typeof(CtPlayerManager));
            var serializedObject = new SerializedObject(instance);
            var playerRenderTexturesProp = serializedObject.FindProperty("playerRenderTextures");
            playerRenderTexturesProp.arraySize = renderTextures.Length;
            for (int i = 0; i < renderTextures.Length; i++)
            {
                var arrayIndexProp = playerRenderTexturesProp.GetArrayElementAtIndex(i);
                arrayIndexProp.objectReferenceValue = renderTextures[i];
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void _UpdatePartyTemplate(Transform template, int partySize)
        {
            var party = template.GetComponent<CtParty>();
            var serializedObject = new SerializedObject(party);

            var prop = serializedObject.FindProperty("members");
            prop.arraySize = partySize;
            for (int i = 0; i < prop.arraySize; i++)
                prop.GetArrayElementAtIndex(i).uintValue = CtConstants.InvalidId;

            prop = serializedObject.FindProperty("membersCmp");
            prop.arraySize = partySize;
            for (int i = 0; i < prop.arraySize; i++)
                prop.GetArrayElementAtIndex(i).uintValue = CtConstants.InvalidId;

            serializedObject.ApplyModifiedProperties();
        }

        private static void _UpdateParties(int capacity)
        {
            var partyManager = (CtPartyManager)Object.FindObjectOfType(typeof(CtPartyManager));

            var xform = partyManager.transform.Find("PlayerParties/_Template");
            _UpdatePartyTemplate(xform, 4);
            _UpdateParties<CtPartyManager, CtParty>(partyManager, "playerParty", "playerParties", capacity, xform);

            xform = partyManager.transform.Find("EnemyParties/_Template");
            _UpdatePartyTemplate(xform, 4);
            _UpdateParties<CtPartyManager, CtParty>(partyManager, "enemyParty", "enemyParties", capacity, xform);

            var battleStates = Object.FindObjectsOfType<CtBattleState>();
            foreach (var battleState in battleStates)
            {
                _UpdatePartyTemplate(battleState.transform, 4);
            }
        }

        private static void _UpdateParties<TManager, T>(TManager manager, string prefix, string targetPropertyName, int capacity, Transform partyTemplate)
            where TManager : UdonSharpBehaviour
            where T : UdonSharpBehaviour
        {
            var group = partyTemplate.transform.parent;
            for (int i = group.childCount - 1; i >= 0; --i)
            {
                var child = group.GetChild(i);
                if (child == partyTemplate.transform)
                    continue;
                Object.DestroyImmediate(child.gameObject);
            }

            var serializedObject = new SerializedObject(manager);

            var prop = serializedObject.FindProperty(targetPropertyName);
            prop.arraySize = capacity;
            for (int i = 0; i < capacity; i++)
            {
                var prefab = Object.Instantiate(partyTemplate.gameObject, partyTemplate.transform.parent);
                prefab.SetActive(true);
                prefab.name = $"{prefix}_{i:0000}";
                prop.GetArrayElementAtIndex(i).objectReferenceValue = prefab.GetComponent<T>();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void _UpdatePlayerDefs(int capacity)
        {
            var playerManager = (CtPlayerManager)Object.FindObjectOfType(typeof(CtPlayerManager));
            var serializedObject = new SerializedObject(playerManager);
            var prop = serializedObject.FindProperty("playerDefs");
            prop.arraySize = capacity;
            serializedObject.ApplyModifiedProperties();
        }

        private static void _UpdateEntities(int capacity)
        {
            var entityManager = (CtEntityManager)Object.FindObjectOfType(typeof(CtEntityManager));

            _UpdateParties<CtEntityManager, CtEntity>(entityManager, "playerEntity", "playerEntities", capacity, 
                entityManager.transform.Find("PlayerEntities/_Template"));

            // NOTE: Max player party member count minus one.
            // TODO: Grab the template for the player party and grab the member count.
            int maxRecruitCount = capacity * 3;
            _UpdateParties<CtEntityManager, CtEntity>(entityManager, "recruitEntity", "recruitEntities", maxRecruitCount, 
                entityManager.transform.Find("RecruitEntities/_Template"));

            // TODO: Grab the template for the enemy party and grab the member count.
            int maxEnemyCount = capacity * 4;
            _UpdateParties<CtEntityManager, CtEntity>(entityManager, "enemyEntity", "enemyEntities", maxEnemyCount, 
                entityManager.transform.Find("EnemyEntities/_Template"));
        }

        [MenuItem("CreatureTime/Rpg/Update Counts", false, 1)]
        private static void _UpdateCounts()
        {
            var worldData = _GetWorld();

            _UpdateRenderTargets(worldData.Capacity);
            _UpdateParties(worldData.Capacity);
            _UpdatePlayerDefs(worldData.Capacity);
            _UpdateEntities(worldData.Capacity);
        }

        // [MenuItem("CreatureTime/Rpg/Assign Unique Ids to All Sequence Nodes")]
        // private static void AssignUniqueIdsToAllSequenceNodes()
        // {
        //     var sequenceNodes = GameObject.FindObjectsOfType<CtSequenceNode>();
        //     foreach (var sequenceNode in sequenceNodes)
        //     {
        //         var so = new SerializedObject(sequenceNode);
        //         var path = sequenceNode.transform._GetFullPath();
        //         so.FindProperty("identifier").stringValue = path;
        //         so.ApplyModifiedProperties();
        //     }
        // }

        [MenuItem("CreatureTime/Rpg/Update All")]
        private static void UpdateAll()
        {
            _UpdateSkills();
            _UpdateCounts();
            // AssignUniqueIdsToAllSequenceNodes();
        }

        // private static string _GetFullPath(this Transform tr)
        // {
        //     var parents = tr.GetComponentsInParent<Transform>();
        //
        //     var results = string.Empty;
        //     if (parents.Length > 0)
        //     {
        //         results += parents[0].name;
        //         for (int i = parents.Length - 2; i >= 0; i--)
        //             results += "/" + parents[i].name;
        //     }
        //
        //     return results;
        // }
    }
}