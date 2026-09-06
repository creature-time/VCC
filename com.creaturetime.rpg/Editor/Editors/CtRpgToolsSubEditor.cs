
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Core;
using VRC.SDKBase;
using VRC.SDKBase.Editor.Api;
using Object = UnityEngine.Object;

namespace CreatureTime.RpgGame
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

            var skillDump = new Button
            {
                text = "Skill Dump",
            };
            skillDump.clickable.clicked += _SkillDump;
            Add(skillDump);

            Add(new Label());

            var runAll = new Button
            {
                text = "Run All",
            };
            runAll.clickable.clicked += _RunAll;
            Add(runAll);
        }

        private static void _SkillDump()
        {
            var attributeDefs = Object.FindObjectsOfType<CtAttributeDef>(true);
            var attributeDefsLookup = attributeDefs.ToDictionary(x => x.Identifier, x => x);
            var skillDefs = Object.FindObjectsOfType<CtSkillDef>(true);

            var csvContent = new StringBuilder("Skill Name,Description,Attribute,Target Type,Sub Type,Is Beneficial,Skill Type,Cost,Recharge Time\n");
            foreach (var skillDef in skillDefs)
            {
                if (skillDef.AttributeType == CtConstants.InvalidId) continue;
                csvContent.AppendLine($"{skillDef.DisplayName},\"{skillDef.GetDebugDescription()}\",{attributeDefsLookup[skillDef.AttributeType].DisplayName},\"{skillDef.TargetType}\",{skillDef.SubType},{skillDef.IsBeneficial},{skillDef.SkillType},{skillDef.Cost},{skillDef.RechargeTime}");
            }
            File.WriteAllText(Application.dataPath + "/SkillDump.csv", csvContent.ToString());
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
            CtSkillDefFuncs.AssignSkillFlags();
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

            for (int i = assets.Length; i < capacity; i++)
                AssetDatabase.CopyAsset(renderTextureTemplate, 
                    $"{generatedPath}/AvatarRenderTexture_{i:0000}.renderTexture");

            assets = AssetDatabase.FindAssets("t:RenderTexture", new string[] { generatedPath });
            var renderTextures = new RenderTexture[assets.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assets[i]);
                renderTextures[i] = AssetDatabase.LoadAssetAtPath<RenderTexture>(assetPath);
            }

            var instance = (CtAvatarSnapshot)Object.FindObjectOfType(typeof(CtAvatarSnapshot));
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

            var partySlots = party.GetComponentsInChildren<CtPartySlot>(true);
            // for (int i = 0; i < partySlots.Length; i++)
            // {
            //     var so = new SerializedObject(partySlots[i]);
            //     so.FindProperty("slotIndex").intValue = i;
            //     so.ApplyModifiedProperties();
            // }

            var prop = serializedObject.FindProperty("slots");
            prop.arraySize = partySlots.Length;
            for (int i = 0; i < prop.arraySize; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = partySlots[i];

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

        private static void _ClearTemplates(Transform template)
        {
            var group = template.transform.parent;
            for (int i = group.childCount - 1; i >= 0; --i)
            {
                var child = group.GetChild(i);
                if (child == template.transform)
                    continue;
                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void _UpdateTemplateCounts<TManager, T>(TManager manager, string prefix, string targetPropertyName, 
            int start, int capacity, Transform template)
            where TManager : UdonSharpBehaviour
            where T : UdonSharpBehaviour
        {
            _ClearTemplates(template);

            var serializedObject = new SerializedObject(manager);

            var prop = serializedObject.FindProperty(targetPropertyName);
            prop.arraySize = capacity;
            for (int i = 0; i < capacity; i++)
            {
                var prefab = Object.Instantiate(template.gameObject, template.transform.parent);

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

        private static void _UpdatePlayerEntities(int capacity, int extra = 4)
        {
            capacity += extra;

            var playerPersistenceManager = (CtPlayerPersistenceManager)Object.FindObjectOfType(typeof(CtPlayerPersistenceManager));

            var template = playerPersistenceManager.transform.Find("PlayerWorldPersistenceData/_Template");
            _ClearTemplates(template);

            var serializedObject = new SerializedObject(playerPersistenceManager);
            var entityManagerSerializedObject = new SerializedObject((CtEntityManager)Object.FindObjectOfType(typeof(CtEntityManager)));

            var prop = serializedObject.FindProperty("playerWorldPersistenceDataArray");
            prop.arraySize = capacity;

            var playerEntitiesProp = entityManagerSerializedObject.FindProperty("playerEntities");
            playerEntitiesProp.arraySize = capacity;

            for (int i = 0; i < capacity; i++)
            {
                var prefab = Object.Instantiate(template.gameObject, template.transform.parent);

                // Remove EditorOnly tag
                prefab.gameObject.tag = "Untagged";

                var t = prefab.GetComponentInChildren<CtPlayerEntity>();
                var so = new SerializedObject(t);
                var idProp = so.FindProperty("identifier");
                idProp.intValue = i;
                so.ApplyModifiedProperties();

                prefab.SetActive(true);
                prefab.name = $"playerEntity_{i:0000}";
                prop.GetArrayElementAtIndex(i).objectReferenceValue = prefab.GetComponent<CtPlayerWorldPersistenceData>();;
                playerEntitiesProp.GetArrayElementAtIndex(i).objectReferenceValue = t;
            }

            serializedObject.ApplyModifiedProperties();
            entityManagerSerializedObject.ApplyModifiedProperties();
        }

        private static void _UpdateEntities(int capacity)
        {
            var entityManager = (CtEntityManager)Object.FindObjectOfType(typeof(CtEntityManager));

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
            _UpdatePlayerEntities(worldData.Capacity);
            _UpdateEntities(worldData.Capacity);
            _UpdateBattleStates(worldData.Capacity);
        }

        private static void _UpdateAllGlobalFields()
        {
            CtSingletonEditor.AssignSingletons(CtSingletonEditor.GetCurrentSingletonTypes());
        }

        private static void _RunAll()
        {
            _UpdateSkills();
            _UpdateAllGlobalFields();
            _UpdateCounts();
        }
    }
}