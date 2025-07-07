
using System;
using System.Collections.Generic;
using System.Reflection;
using CreatureTime.RpgGame;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Udon.Serialization.OdinSerializer.Utilities;
using Object = UnityEngine.Object;

namespace CreatureTime
{
    public abstract class CtRpgDefinition : VisualElement
    {
        public abstract void RefreshView();
    }

    public class CtRpgDefintionEditor<T> : CtRpgDefinition
        where T : CtAbstractDefData
    {
        public List<T> Data { get; private set; } = new List<T>();

        private MultiColumnListView _view;

        public CtRpgDefintionEditor(string title)
        {
            _CreateDefinitionsView(title);
        }

        private void _CreateDefinitionsView(string title)
        {
            var type = typeof(T);

            _view = new MultiColumnListView
            {
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                itemsSource = Data,
                selectionType = SelectionType.Multiple,
                style = { minWidth = 256, minHeight = 128, flexGrow = 1f }
            };

            _view.columns.Add(new Column
            {
                minWidth = 32, maxWidth = 32,
                makeCell = () => new Label(),
                bindCell = (element, i) => { }
            });

            int variant = 0;
            _PopulateRow(type, ref variant);

            Add(_view);
        }

        // private void _CreateCateogry(string text)
        // {
        //     var toolbar = new Toolbar();
        //     Add(toolbar);
        //
        //     var header = new Label
        //     {
        //         text = text,
        //         style =
        //         {
        //             backgroundColor = Color.black,
        //             color = Color.white,
        //             fontSize = 14
        //         }
        //     };
        //     toolbar.Add(header);
        // }

        private void _PopulateRow(Type type, ref int variant, string parentProp = "")
        {
            int levelVariant = variant;
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var propPath = string.IsNullOrEmpty(parentProp) ? field.Name : $"{parentProp}.{field.Name}";
                if (field.FieldType.IsStruct() && field.FieldType != typeof(Color))
                {
                    variant += 1;
                    _PopulateRow(field.FieldType, ref variant, propPath);
                }
                else
                {
                    _GenerateColumn(field.FieldType, ObjectNames.NicifyVariableName(field.GetNiceName()), propPath, levelVariant);
                }
            }
        }

        private void _GenerateColumn(Type type, string title, string bindingPath, int variant)
        {
            Func<VisualElement> makeCellFunc;
            if (type.IsEnum)
            {
                makeCellFunc = () =>
                {
                    var field = new EnumField();
                    return field;
                };
            }
            else if (type.IsSubclassOf(typeof(Object)))
            {
                makeCellFunc = () => new ObjectField
                {
                    objectType = type
                };
            }
            else
            {
                var typeToMakeCellFunc = new Dictionary<Type, Func<VisualElement>>();
                typeToMakeCellFunc.Add(typeof(string), () => new TextField
                {
                    value = "<unbounded>"
                });
                typeToMakeCellFunc.Add(typeof(Color), () => new ColorField());
                typeToMakeCellFunc.Add(typeof(bool), () => new Toggle());
                typeToMakeCellFunc.Add(typeof(ushort), () => new UnsignedIntegerField());
                typeToMakeCellFunc.Add(typeof(uint), () => new UnsignedIntegerField());
                typeToMakeCellFunc.Add(typeof(ulong), () => new UnsignedLongField());
                typeToMakeCellFunc.Add(typeof(short), () => new IntegerField());
                typeToMakeCellFunc.Add(typeof(int), () => new IntegerField());
                typeToMakeCellFunc.Add(typeof(long), () => new LongField());
                typeToMakeCellFunc.Add(typeof(float), () => new FloatField());

                if (typeToMakeCellFunc.ContainsKey(type))
                    makeCellFunc = typeToMakeCellFunc[type];
                else
                    makeCellFunc = () => new Label
                    {
                        text = $"<failed to find type (type={type})>"
                    };
            }

            Action<VisualElement, int> bindCellFunc;
            if (type.IsEnum)
            {
                bindCellFunc = (element, i) =>
                {
                    var field = (EnumField)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                };
            }
            else if (type.IsSubclassOf(typeof(Object)))
            {
                bindCellFunc = (element, i) =>
                {
                    var field = (ObjectField)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                };
            }
            else
            {
                var typeToBindCell = new Dictionary<Type, Action<VisualElement, int>>();
                typeToBindCell.Add(typeof(Color), (element, i) =>
                {
                    var field = (ColorField)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                });
                typeToBindCell.Add(typeof(string), (element, i) =>
                {
                    var field = (TextField)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                });
                typeToBindCell.Add(typeof(bool), (element, i) =>
                {
                    var field = (Toggle)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                });
                typeToBindCell.Add(typeof(ushort), (element, i) =>
                {
                    var field = (UnsignedIntegerField)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                });
                typeToBindCell.Add(typeof(uint), (element, i) =>
                {
                    var field = (UnsignedIntegerField)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                });
                typeToBindCell.Add(typeof(ulong), (element, i) =>
                {
                    var field = (UnsignedIntegerField)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                });
                typeToBindCell.Add(typeof(short), (element, i) =>
                {
                    var field = (IntegerField)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                });
                typeToBindCell.Add(typeof(int), (element, i) =>
                {
                    var field = (IntegerField)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                });
                typeToBindCell.Add(typeof(long), (element, i) =>
                {
                    var field = (IntegerField)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                });
                typeToBindCell.Add(typeof(float), (element, i) =>
                {
                    var field = (FloatField)element;
                    field.bindingPath = bindingPath;
                    _Bind(field, i);
                });

                if (typeToBindCell.ContainsKey(type))
                    bindCellFunc = typeToBindCell[type];
                else
                    bindCellFunc = (element, i) => { };
            }

            int minWidth = 64;
            if (type.IsSubclassOf(typeof(Object)))
            {
                minWidth = 128;
            }
            else
            {
                var minWidths = new Dictionary<Type, int>
                {
                    { typeof(string), 128 },
                };

                if (minWidths.ContainsKey(type))
                    minWidth = minWidths[type];
            }

            Color backgroundColor = new Color(0, 0, 0, 0);
            if (variant > 0)
            {
                Color[] variants = 
                {
                    Color.red,
                    Color.green,
                    Color.blue,
                    Color.magenta,
                    Color.cyan,
                    Color.yellow
                };
                for (int i = 0; i < variants.Length; i++)
                    variants[i].a = 0.2f;
                backgroundColor = variants[(variant - 1) % variants.Length];
            }

            var column = new Column
            {
                title = title,
                makeCell = makeCellFunc,
                bindCell = bindCellFunc,
                minWidth = minWidth,
                makeHeader = () =>
                {
                    return new Label
                    {
                        style =
                        {
                            flexGrow = 1.0f,
                            paddingLeft= 2,
                            paddingRight = 2,
                            paddingTop = 2,
                            paddingBottom = 2,
                            unityTextAlign = TextAnchor.MiddleLeft,
                            backgroundColor = backgroundColor
                        }
                    };
                },
                bindHeader = element =>
                {
                    var label = (Label)element;
                    label.text = title;
                }
            };
            _view.columns.Add(column);
        }

        private void _Bind(VisualElement field, int index)
        {
            field.Bind(new SerializedObject(Data[index]));
        }

        private void _FindDefinitionAssets()
        {
            Data.Clear();

            Type type = typeof(T);
            var assets = AssetDatabase.FindAssets($"t:{type.Name}", 
                new string[] { "Assets/CreatureTime/Editor/Data/Rpg" });
            foreach (var asset in assets)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(asset);
                Data.Add(AssetDatabase.LoadAssetAtPath<T>(assetPath));
            }

            Data.Sort((a, b) => a.Identifier.CompareTo(b.Identifier));
        }

        public override void RefreshView()
        {
            _FindDefinitionAssets();
            _view.RefreshItems();
        }
    }

    public class CtImportantButton : Button
    {
        public CtImportantButton()
        {
            style.fontSize = 16;
            Color color = Color.red;
            color.a = 0.2f;
            style.backgroundColor = color;
        }
    }

    public class CtRpgDefinitionsSubEditor : CtCreatureTimeSubEditor
    {
        private const string DefinitionsLocation = "Assets/CreatureTime/Editor/Data/Rpg";

        public override string Name => "RPG Def Editor";

        private Dictionary<Type, CtRpgDefinition> _definitionViews = new Dictionary<Type, CtRpgDefinition>();

        private CtTabElement _tabElement;

        private void RenameDefinitions<T>(string prefix)
            where T : CtAbstractDefData
        {
            var view = _GetView<T>();
            foreach (var data in view.Data)
            {
                var generatedName = data.GenerateName;
                string assetPath = AssetDatabase.GetAssetPath(data);
                AssetDatabase.RenameAsset(assetPath, $"{prefix}_{generatedName.ToLower()}");
            }
        }

        public CtRpgDefinitionsSubEditor()
        {
            var toolbar = new VisualElement()
            {
                style = { flexDirection = FlexDirection.Row }
            };
            Add(toolbar);

            var generateGameData = new CtImportantButton
            {
                text = "Generate Game Data"
            };
            generateGameData.clicked += _OnGenerateGameData;
            toolbar.Add(generateGameData);

            var renameGameDataAssets = new CtImportantButton
            {
                text = "Rename Game Data Assets"
            };
            renameGameDataAssets.clicked += _OnRenameAssets;
            toolbar.Add(renameGameDataAssets);

            var refresh = new CtImportantButton
            {
                text = "Refresh"
            };
            refresh.clicked += _RefreshViews;
            toolbar.Add(refresh);

            _tabElement = new CtTabElement
            {
                style = { flexGrow = 1f }
            };
            Add(_tabElement);

            _CreateView<CtBattleQuestData>("Battle Quest Definitions");
            _CreateView<CtSquadDefData>("Squad Definitions");
            _CreateView<CtNpcDefData>("Npc Definitions");
            _CreateView<CtMainHandDefData>("Main Hand Definitions");
            _CreateView<CtOffHandDefData>("Off Hand Definitions");
            _CreateView<CtArmorSetDefData>("Armor Definitions");
            _CreateView<CtSkillDefData>("Skill Definitions");
            _CreateView<CtProfessionDefData>("Profession Definitions");
            _CreateView<CtAttributeDefData>("Attribute Definitions");
            _CreateView<CtNpcBehaviorData>("Npc Behavior Definitions");

            _RefreshViews();
        }

        private void _CreateView<T>(string title)
            where T : CtAbstractDefData
        {
            var view = new CtRpgDefintionEditor<T>(title);
            _tabElement.AddTab(title, view);
            _definitionViews.Add(typeof(T), view);
        }

        private CtRpgDefintionEditor<T> _GetView<T>()
            where T : CtAbstractDefData
        {
            return (CtRpgDefintionEditor<T>)_definitionViews[typeof(T)];
        }

        private void _RefreshViews()
        {
            foreach (var view in _definitionViews.Values)
                view.RefreshView();
        }

        private void _OnRenameAssets()
        {
            RenameDefinitions<CtSkillDefData>("skl");
            RenameDefinitions<CtNpcDefData>("npc");
            RenameDefinitions<CtMainHandDefData>("mhw");
            RenameDefinitions<CtOffHandDefData>("ofw");
            RenameDefinitions<CtArmorSetDefData>("arm");
            RenameDefinitions<CtProfessionDefData>("pro");
            RenameDefinitions<CtAttributeDefData>("att");
            RenameDefinitions<CtNpcBehaviorData>("bhv");
            RenameDefinitions<CtNpcBehaviorData>("qst");
            RenameDefinitions<CtSquadDefData>("sqd");
        }

        private void _OnGenerateGameData()
        {
            var gameData = Object.FindFirstObjectByType<CtGameData>();
            if (!gameData)
            {
                return;
            }

            var npcDefLookUp = new Dictionary<CtNpcDefData, CtNpcDef>();
            var behaviors = new Dictionary<CtNpcBehaviorData, CtNpcBehavior>();
            {
                var view = _GetView<CtNpcBehaviorData>();
                // view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var group = gameData.transform.Find("NpcBehaviors");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                foreach (var data in view.Data)
                    behaviors.Add(data, _GenerateNpcBehaviors(group.transform, data));
            }

            {
                var view = _GetView<CtNpcDefData>();
                view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var group = gameData.transform.Find("Npcs");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                foreach (var data in view.Data)
                {
                    var gameObject = new GameObject(data.GenerateName);
                    gameObject.transform.SetParent(group);

                    var npcDef = gameObject.AddComponent<CtNpcDef>();
                    npcDefLookUp.Add(data, npcDef);
                    var so = new SerializedObject(npcDef);

                    so.FindProperty("identifier").intValue = data.identifier;
                    so.FindProperty("displayName").stringValue = data.displayName;
                    so.FindProperty("icon").objectReferenceValue = data.icon;
                    so.FindProperty("characterLevel").intValue = data.characterLevel;
                    so.FindProperty("attributeData").ulongValue = data.professionDataBlock.DataBlock;
                    so.FindProperty("mainHandWeaponData").ulongValue = data.mainHandDataBlock.DataBlock;
                    so.FindProperty("offHandWeaponData").ulongValue = data.offHandDataBlock.DataBlock;
                    so.FindProperty("headSlotData").ulongValue = data.headArmorDataBlock.DataBlock;
                    so.FindProperty("chestSlotData").ulongValue = data.chestArmorDataBlock.DataBlock;
                    so.FindProperty("handsSlotData").ulongValue = data.handsArmorDataBlock.DataBlock;
                    so.FindProperty("legsSlotData").ulongValue = data.legsArmorDataBlock.DataBlock;
                    so.FindProperty("feetSlotData").ulongValue = data.feetArmorDataBlock.DataBlock;
                    so.FindProperty("behavior").objectReferenceValue = data.behavior ? behaviors[data.behavior] : null;
                    so.FindProperty("skillSlot0").intValue =
                        data.skillsBlock.skillDef0 ? data.skillsBlock.skillDef0.identifier : CtConstants.InvalidId;
                    so.FindProperty("skillSlot1").intValue =
                        data.skillsBlock.skillDef1 ? data.skillsBlock.skillDef1.identifier : CtConstants.InvalidId;
                    so.FindProperty("skillSlot2").intValue =
                        data.skillsBlock.skillDef2 ? data.skillsBlock.skillDef2.identifier : CtConstants.InvalidId;
                    so.FindProperty("skillSlot3").intValue =
                        data.skillsBlock.skillDef3 ? data.skillsBlock.skillDef3.identifier : CtConstants.InvalidId;
                    so.FindProperty("skillSlot4").intValue =
                        data.skillsBlock.skillDef4 ? data.skillsBlock.skillDef4.identifier : CtConstants.InvalidId;
                    so.FindProperty("skillSlot5").intValue =
                        data.skillsBlock.skillDef5 ? data.skillsBlock.skillDef5.identifier : CtConstants.InvalidId;
                    so.FindProperty("skillSlot6").intValue =
                        data.skillsBlock.skillDef6 ? data.skillsBlock.skillDef6.identifier : CtConstants.InvalidId;
                    so.FindProperty("skillSlot7").intValue =
                        data.skillsBlock.skillDef7 ? data.skillsBlock.skillDef7.identifier : CtConstants.InvalidId;
                    so.FindProperty("skillSlot8").intValue =
                        data.skillsBlock.skillDef8 ? data.skillsBlock.skillDef8.identifier : CtConstants.InvalidId;
                    so.FindProperty("skillSlot9").intValue =
                        data.skillsBlock.skillDef9 ? data.skillsBlock.skillDef9.identifier : CtConstants.InvalidId;

                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            {
                var view = _GetView<CtMainHandDefData>();
                view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var group = gameData.transform.Find("Weapons/Main-Hand");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                foreach (var data in view.Data)
                {
                    if (!data.attributeType)
                    {
                        Debug.LogWarning("Main-hand did not have attribute type");
                        continue;
                    }

                    var gameObject = new GameObject(data.GenerateName);
                    gameObject.transform.SetParent(group);

                    var weaponDefSo = new SerializedObject(gameObject.AddComponent<CtWeaponDef>());
                    weaponDefSo.FindProperty("identifier").intValue = data.identifier;
                    weaponDefSo.FindProperty("displayName").stringValue = data.displayName;
                    weaponDefSo.FindProperty("icon").objectReferenceValue = data.icon;
                    weaponDefSo.FindProperty("weaponType").enumValueIndex = Convert.ToInt32(data.weaponType);
                    weaponDefSo.FindProperty("attributeType").intValue = data.attributeType.identifier;

                    var prefab = PrefabUtility.InstantiatePrefab(data.userData.userData).GameObject();
                    prefab.transform.SetParent(gameObject.transform);
                    weaponDefSo.FindProperty("userData").objectReferenceValue = prefab;

                    var userData = prefab.GetComponent<CtWeaponAttack>();
                    var userDataSo = new SerializedObject(userData);
                    userDataSo.FindProperty("palette").intValue = data.userData.palette;
                    userDataSo.ApplyModifiedPropertiesWithoutUndo();

                    prefab.GetComponent<MeshRenderer>().material = data.userData.material;

                    weaponDefSo.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            {
                var view = _GetView<CtOffHandDefData>();
                view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var group = gameData.transform.Find("Weapons/Off-Hand");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                foreach (var data in view.Data)
                {
                    if (!data.attributeType)
                    {
                        Debug.LogWarning("Off-hand did not have attribute type");
                        continue;
                    }

                    var gameObject = new GameObject(data.GenerateName);
                    gameObject.transform.SetParent(group);

                    var so = new SerializedObject(gameObject.AddComponent<CtOffHandDef>());

                    so.FindProperty("identifier").intValue = data.identifier;
                    so.FindProperty("displayName").stringValue = data.displayName;
                    so.FindProperty("icon").objectReferenceValue = data.icon;
                    so.FindProperty("attributeType").intValue = data.attributeType.identifier;
                    so.FindProperty("attributeRequirement").intValue = data.attributeRequirement;
                    so.FindProperty("minModifierStat").intValue = data.minModifierStat;
                    so.FindProperty("maxModifierStat").intValue = data.maxModifierStat;
                    so.FindProperty("rarity").enumValueIndex = Convert.ToInt32(data.rarity);

                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            {
                var view = _GetView<CtArmorSetDefData>();
                view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var group = gameData.transform.Find("Armor");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                foreach (var data in view.Data)
                {
                    var gameObject = new GameObject(data.GenerateName);
                    gameObject.transform.SetParent(group);

                    var armorSetDef = gameObject.AddComponent<CtArmorSetDef>();
                    var so = new SerializedObject(armorSetDef);

                    so.FindProperty("identifier").intValue = data.identifier;
                    so.FindProperty("displayName").stringValue = data.displayName;
                    so.FindProperty("rarity").enumValueIndex = Convert.ToInt32(data.rarity);
                    so.FindProperty("headSlot").objectReferenceValue =
                        _GenerateArmorDefs(armorSetDef, data.identifier, data.displayName, data.headSlot);
                    so.FindProperty("chestSlot").objectReferenceValue =
                        _GenerateArmorDefs(armorSetDef, data.identifier, data.displayName, data.chestSlot);
                    so.FindProperty("handsSlot").objectReferenceValue =
                        _GenerateArmorDefs(armorSetDef, data.identifier, data.displayName, data.handsSlot);
                    so.FindProperty("legsSlot").objectReferenceValue =
                        _GenerateArmorDefs(armorSetDef, data.identifier, data.displayName, data.legsSlot);
                    so.FindProperty("feetSlot").objectReferenceValue =
                        _GenerateArmorDefs(armorSetDef, data.identifier, data.displayName, data.feetSlot);

                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            var attributeDefs = new Dictionary<CtAttributeDefData, CtAttributeDef>();
            {
                {
                    var view = _GetView<CtProfessionDefData>();
                    view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                    var group = gameData.transform.Find("Professions");
                    for (int i = group.transform.childCount - 1; i >= 0; i--)
                        Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                    foreach (var data in view.Data)
                    {
                        var gameObject = new GameObject(data.GenerateName);
                        gameObject.transform.SetParent(group);

                        var so = new SerializedObject(gameObject.AddComponent<CtProfessionDef>());

                        so.FindProperty("identifier").intValue = data.identifier;
                        so.FindProperty("displayName").stringValue = data.displayName;
                        so.FindProperty("icon").objectReferenceValue = data.icon;
                        so.FindProperty("theme").colorValue = data.theme;

                        var attributes = data.Attributes;

                        var attributesProp = so.FindProperty("attributes");
                        attributesProp.arraySize = attributes.Length;
                        for (int i = 0; i < attributes.Length; i++)
                        {
                            var arrayIndexProp = attributesProp.GetArrayElementAtIndex(i);
                            var attributeDefData = attributes[i];
                            var attributeDef = _GenerateAttributes(gameObject.transform, attributeDefData);
                            attributeDefs.Add(attributeDefData, attributeDef);
                            arrayIndexProp.objectReferenceValue = attributeDef;
                        }

                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }

                {
                    var view = _GetView<CtSkillDefData>();
                    view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                    foreach (var data in view.Data)
                    {
                        if (!data.attributeType)
                        {
                            Debug.LogWarning("Skill did not have attribute type");
                            continue;
                        }

                        var gameObject = new GameObject(data.GenerateName);
                        gameObject.transform.SetParent(attributeDefs[data.attributeType].transform);

                        var so = new SerializedObject(gameObject.AddComponent(data.script.GetClass()));

                        so.FindProperty("identifier").intValue = data.identifier;
                        so.FindProperty("displayName").stringValue = data.displayName;
                        so.FindProperty("icon").objectReferenceValue = data.icon;
                        so.FindProperty("attributeType").intValue = data.attributeType.identifier;

                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }

            var squadDefLookUp = new Dictionary<CtSquadDefData, CtSquadDef>();
            {
                {
                    var view = _GetView<CtSquadDefData>();
                    view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                    var group = gameData.transform.Find("Squads");
                    for (int i = group.transform.childCount - 1; i >= 0; i--)
                        Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                    foreach (var data in view.Data)
                    {
                        var gameObject = new GameObject(data.GenerateName);
                        gameObject.transform.SetParent(group);

                        var squadDef = gameObject.AddComponent<CtSquadDef>();
                        squadDefLookUp.Add(data, squadDef);
                        var so = new SerializedObject(squadDef);

                        so.FindProperty("identifier").intValue = data.identifier;
                        so.FindProperty("userData").objectReferenceValue = data.userData.userData;

                        var npcDefs = data.npcDataBlock.NpcDefs;

                        var npcDefsProp = so.FindProperty("npcDefs");
                        npcDefsProp.arraySize = npcDefs.Length;
                        for (int i = 0; i < npcDefs.Length; i++)
                        {
                            var arrayIndexProp = npcDefsProp.GetArrayElementAtIndex(i);
                            arrayIndexProp.objectReferenceValue = npcDefLookUp[npcDefs[i]];
                        }

                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }

            {
                {
                    var view = _GetView<CtBattleQuestData>();
                    view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                    var group = gameData.transform.Find("Quests/Battle");
                    for (int i = group.transform.childCount - 1; i >= 0; i--)
                        Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                    foreach (var data in view.Data)
                    {
                        var gameObject = new GameObject(data.GenerateName);
                        gameObject.transform.SetParent(group);

                        var battleQuest = gameObject.AddComponent<CtBattleQuest>();
                        var so = new SerializedObject(battleQuest);

                        so.FindProperty("identifier").intValue = data.identifier;
                        so.FindProperty("displayName").stringValue = data.displayName;
                        so.FindProperty("icon").objectReferenceValue = data.icon;
                        so.FindProperty("levelReq").intValue = data.levelReq;

                        var squadCategories = new List<CtSquadCategory>();
                        var squadCategory = _GenerateSquadCategory(battleQuest, data.squadCategory0, ref squadDefLookUp);
                        if (squadCategory)
                            squadCategories.Add(squadCategory);
                        squadCategory = _GenerateSquadCategory(battleQuest, data.squadCategory1, ref squadDefLookUp);
                        if (squadCategory)
                            squadCategories.Add(squadCategory);
                        squadCategory = _GenerateSquadCategory(battleQuest, data.squadCategory2, ref squadDefLookUp);
                        if (squadCategory)
                            squadCategories.Add(squadCategory);

                        var squadCategoriesProp = so.FindProperty("squadCategories");
                        squadCategoriesProp.arraySize = squadCategories.Count;
                        for (int i = 0; i < squadCategories.Count; i++)
                        {
                            var arrayIndexProp = squadCategoriesProp.GetArrayElementAtIndex(i);
                            arrayIndexProp.objectReferenceValue = squadCategories[i];
                        }

                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }
        }

        private CtSquadCategory _GenerateSquadCategory(CtBattleQuest battleQuest, CtSquadCategoryDataBlock data,
            ref Dictionary<CtSquadDefData, CtSquadDef> squadDefLookUp)
        {
            var squadDefs = data.SquadDefs;
            if (squadDefs.Length == 0)
                return null;

            var squadCategory = battleQuest.gameObject.AddComponent<CtSquadCategory>();
            var so = new SerializedObject(squadCategory);

            var squadDefsProp = so.FindProperty("squadDefs");
            squadDefsProp.arraySize = squadDefs.Length;
            for (int i = 0; i < squadDefs.Length; i++)
            {
                var arrayIndexProp = squadDefsProp.GetArrayElementAtIndex(i);
                arrayIndexProp.objectReferenceValue = squadDefLookUp[squadDefs[i]];
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            return squadCategory;
        }

        private CtArmorSlotDef _GenerateArmorDefs(
            CtArmorSetDef armorSetDef, ushort identifier, string displayName, CtArmorSlotData data)
        {
            var def = armorSetDef.gameObject.AddComponent<CtArmorSlotDef>();
            var so = new SerializedObject(def);

            so.FindProperty("identifier").intValue = identifier;
            so.FindProperty("displayName").stringValue = 
                string.IsNullOrEmpty(data.suffix) ? displayName : $"{displayName} {data.suffix}";
            so.FindProperty("icon").objectReferenceValue = data.icon;
            so.FindProperty("armorRating").intValue = data.armorRating;

            so.FindProperty("armorSet").objectReferenceValue = armorSetDef;

            so.ApplyModifiedPropertiesWithoutUndo();

            return def;
        }

        private CtNpcBehavior _GenerateNpcBehaviors(Transform group, CtNpcBehaviorData data)
        {
            var gameObject = new GameObject(data.GenerateName);
            gameObject.transform.SetParent(group);
        
            var def = gameObject.AddComponent<CtNpcBehavior>();
            var so = new SerializedObject(def);
        
            so.FindProperty("selfHealingThreshold").floatValue = data.selfHealingThreshold;
            so.FindProperty("defensiveWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("supportWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("supportCoolDownWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("healingWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("healingCoolDownWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("offensiveWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("useSkillWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("useSkillCoolDownWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("buffingWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("deBuffingWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("conditionsWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("damageWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("attackWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("attackCoolDownWeight").floatValue = data.selfHealingThreshold;
            so.FindProperty("focusTargetWeight").floatValue = data.selfHealingThreshold;
        
            so.ApplyModifiedPropertiesWithoutUndo();
        
            return def;
        }

        private CtAttributeDef _GenerateAttributes(Transform group, CtAttributeDefData data)
        {
            var gameObject = new GameObject(data.GenerateName);
            gameObject.transform.SetParent(group);

            var attributeDef = gameObject.AddComponent<CtAttributeDef>();
            var so = new SerializedObject(attributeDef);

            so.FindProperty("identifier").intValue = data.identifier;
            so.FindProperty("displayName").stringValue = data.displayName;
            so.FindProperty("icon").objectReferenceValue = data.icon;

            so.ApplyModifiedPropertiesWithoutUndo();

            return attributeDef;
        }
    }
}