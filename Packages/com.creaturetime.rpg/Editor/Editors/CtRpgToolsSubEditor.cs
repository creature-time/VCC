
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Core;
using VRC.SDKBase;
using VRC.SDKBase.Editor.Api;
using VRC.Udon.Serialization.OdinSerializer.Utilities;
using Object = UnityEngine.Object;

namespace CreatureTime
{
    public class CtRpgToolsSubEditor : CtCreatureTimeSubEditor
    {
        public override string Name => "RPG Tools";

        public CtRpgToolsSubEditor()
        {
            var updateSkills = new Button
            {
                text = "Update Skills",
            };
            updateSkills.clickable.clicked += _UpdateSkills;
            Add(updateSkills);

            var assignAllGlobalFields = new Button
            {
                text = "Assign All Global Fields",
            };
            assignAllGlobalFields.clickable.clicked += _UpdateAllGlobalFields;
            Add(assignAllGlobalFields);

            var updateCounts = new Button
            {
                text = "Update Counts",
            };
            updateCounts.clickable.clicked += _UpdateCounts;
            Add(updateCounts);

            Add(new Label());

            var runAll = new Button
            {
                text = "Run All",
            };
            runAll.clickable.clicked += _RunAll;
            Add(runAll);
        }

        private static async Task<VRCWorld> _GetWorld()
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
                worldData = await VRCApi.GetWorld(pipelineManager.blueprintId, true);
            }

            return worldData;
        }

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

        private static void _UpdateTemplateCounts(int capacity)
        {
            var partyManager = (CtPartyManager)Object.FindObjectOfType(typeof(CtPartyManager));

            var xform = partyManager.transform.Find("PlayerParties/_Template");
            _UpdatePartyTemplate(xform, 4);
            _UpdateTemplateCounts<CtPartyManager, CtParty>(partyManager, "playerParty", "playerParties", 0, capacity, xform);

            xform = partyManager.transform.Find("EnemyParties/_Template");
            _UpdatePartyTemplate(xform, 4);
            _UpdateTemplateCounts<CtPartyManager, CtParty>(partyManager, "enemyParty", "enemyParties", 1000, capacity, xform);
        }

        private static void _UpdateTemplateCounts<TManager, T>(TManager manager, string prefix, string targetPropertyName, 
            int start, int capacity, Transform partyTemplate)
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

                // Remove EditorOnly tag
                prefab.gameObject.tag = "Untagged";

                var t = prefab.GetComponent<T>();
                var so = new SerializedObject(t);
                var idProp = so.FindProperty("identifier");
                idProp.intValue = i + start;
                so.ApplyModifiedProperties();

                prefab.SetActive(true);
                prefab.name = $"{prefix}_{i + start:0000}";
                prop.GetArrayElementAtIndex(i).objectReferenceValue = t;
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

            _UpdateTemplateCounts<CtEntityManager, CtEntity>(entityManager, "playerEntity", "playerEntities", 1, capacity, 
                entityManager.transform.Find("PlayerEntities/_Template"));

            // NOTE: Max player party member count minus one.
            // TODO: Grab the template for the player party and grab the member count.
            int maxRecruitCount = capacity * 3;
            _UpdateTemplateCounts<CtEntityManager, CtEntity>(entityManager, "recruitEntity", "recruitEntities", 1000, 
                maxRecruitCount, entityManager.transform.Find("RecruitEntities/_Template"));

            // TODO: Grab the template for the enemy party and grab the member count.
            int maxEnemyCount = capacity * 4;
            _UpdateTemplateCounts<CtEntityManager, CtEntity>(entityManager, "enemyEntity", "enemyEntities", 2000, 
                maxEnemyCount, entityManager.transform.Find("EnemyEntities/_Template"));
        }

        private static void _UpdateBattleStates(int capacity)
        {
            var battleStateManager = (CtBattleStateManager)Object.FindObjectOfType(typeof(CtBattleStateManager));

            _UpdateTemplateCounts<CtBattleStateManager, CtBattleState>(battleStateManager, "BattleState", "battleStates", 0, 
                capacity, battleStateManager.transform.Find("BattleStates/_Template"));

            var blackboards = battleStateManager.GetComponentsInChildren<CtBlackboard>(false);

            var rpgGame = GameObject.FindObjectOfType<CtRpgGame>();
            if (!rpgGame)
                throw new Exception("Failed to find RpgGame.");

            var stateMachine = rpgGame.GetComponent<CtStateMachine>();
            if (!stateMachine)
                throw new Exception("Failed to find StateMachine.");

            var serializedObject = new SerializedObject(stateMachine);

            var prop = serializedObject.FindProperty("contexts");
            prop.arraySize = blackboards.Length;
            for (int i = 0; i < blackboards.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = blackboards[i];

            serializedObject.ApplyModifiedProperties();
        }

        private static async void _UpdateCounts()
        {
            var worldData = await _GetWorld();

            _UpdateRenderTargets(worldData.Capacity);
            _UpdateTemplateCounts(worldData.Capacity);
            _UpdatePlayerDefs(worldData.Capacity);
            _UpdateEntities(worldData.Capacity);
            _UpdateBattleStates(worldData.Capacity);
        }

        private static void _UpdateAllGlobalFields()
        {
            Dictionary<Type, CtSingleton> singletons = new Dictionary<Type, CtSingleton>();

            // Find all objects for each singleton type from all assemblies.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type typ in
                         assembly.GetTypes()
                             .Where(myType =>
                                 myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(CtSingleton))))
                {
                    singletons.Add(typ, (CtSingleton)Object.FindFirstObjectByType(typ, FindObjectsInactive.Include));
                }

            // Throw error if singleton does not exist in the scene.
            foreach (var pair in singletons)
                if (!pair.Value)
                    throw new Exception($"Failed to find singleton for type ({pair.Key})");

            // Find all the components and their fields and set the value of the singleton if the type is the
            // singleton.
            foreach (var component in Object.FindObjectsOfType<UdonSharpBehaviour>(true))
            {
                var type = component.GetType();
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).ToList();
                foreach (var baseType in type.GetBaseTypes())
                    foreach (var fieldInfo in baseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                        fields.Add(fieldInfo);

                foreach (var fieldInfo in fields)
                {
                    if (!singletons.TryGetValue(fieldInfo.FieldType, out var singleton))
                        continue;

                    fieldInfo.SetValue(component, singleton);

                    EditorUtility.SetDirty(component);
                }
            }
        }

        private static void _RunAll()
        {
            _UpdateSkills();
            _UpdateAllGlobalFields();
            _UpdateCounts();
        }
    }
}