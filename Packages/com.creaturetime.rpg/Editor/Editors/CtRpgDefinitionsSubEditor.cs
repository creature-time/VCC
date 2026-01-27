
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CreatureTime.RpgGame;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
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

            var splitPanel = new TwoPaneSplitView(1, 350, TwoPaneSplitViewOrientation.Horizontal);
            Add(splitPanel);

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

            splitPanel.Add(_view);

            var propertyEditor = new ScrollView();
            splitPanel.Add(propertyEditor);

            _view.selectionChanged += objects =>
            {
                propertyEditor.Clear();
                _serializedObject.Clear();

                foreach (var obj in objects)
                {
                    var scriptableObject = (ScriptableObject)obj;
                    Debug.Log(scriptableObject);
                    _serializedObject.Add(scriptableObject);
                }

                if (_serializedObject.Count == 0) return;

                var fallbackEditorType =
                    typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GenericInspector");
                var editor = UnityEditor.Editor.CreateEditor(_serializedObject.ToArray(), fallbackEditorType);
                var imgui = new IMGUIContainer(() =>
                {
                    editor.OnInspectorGUI();
                });

                propertyEditor.Add(imgui);
            };
        }

        private List<Object> _serializedObject = new List<Object>();

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
                // TODO: Reverse these checks and check if type is found otherwise check if is class.
                bool isStruct = field.FieldType.IsValueType && !field.FieldType.IsPrimitive && !field.FieldType.IsEnum;
                if (isStruct && field.FieldType != typeof(Color))
                {
                    variant += 1;
                    _PopulateRow(field.FieldType, ref variant, propPath);
                }
                else
                {
                    _GenerateColumn(field.FieldType, ObjectNames.NicifyVariableName(field.Name), propPath, levelVariant);
                }
            }
        }

        private void _GenerateColumn(Type type, string title, string bindingPath, int variant)
        {
            Func<VisualElement> makeCellFunc;
            if (type.IsEnum)
            {
                if (type.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    makeCellFunc = () =>
                    {
                        var field = new EnumFlagsField();
                        return field;
                    };
                }
                else
                {
                    makeCellFunc = () =>
                    {
                        var field = new EnumField();
                        return field;
                    };
                }
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
                    VisualElement field;
                    if (type.GetCustomAttribute<FlagsAttribute>() != null)
                    {
                        var enumField = (EnumFlagsField)element;
                        enumField.bindingPath = bindingPath;
                        field = enumField;
                    }
                    else
                    {
                        var enumField = (EnumField)element;
                        enumField.bindingPath = bindingPath;
                        field = enumField;
                    }

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
                new string[] { "Assets/CreatureTime/Worlds/RpgGame/Editor/RpgData" });
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
        private const string RpgEditorDirectory = "Assets/CreatureTime/Worlds/RpgGame/Editor/Settings";

        public override string Name => "RPG Def Editor";

        private List<CtRpgDefinition> _definitionViews = new List<CtRpgDefinition>();

        private DropdownField _editorSelect;
        private ScrollView _scrollView;

        private SerializedObject _serializedObject;

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

        private CtRpgDefinitionsSubEditorSettings _GetOrCreateSettings()
        {
            var info = AssetDatabase.LoadAssetAtPath<CtRpgDefinitionsSubEditorSettings>($"{RpgEditorDirectory}/rpg-definitions-sub-editor.asset");
            if (!info)
            {
                info = ScriptableObject.CreateInstance<CtRpgDefinitionsSubEditorSettings>();
                Directory.CreateDirectory(RpgEditorDirectory);
                AssetDatabase.CreateAsset(info, $"{RpgEditorDirectory}/rpg-definitions-sub-editor.asset");
                AssetDatabase.Refresh();
            }

            return info;
        }

        public CtRpgDefinitionsSubEditor()
        {
            _serializedObject = new SerializedObject(_GetOrCreateSettings());

            var toolbar = new VisualElement()
            {
                style = { flexDirection = FlexDirection.Row }
            };
            Add(toolbar);
 
            _editorSelect = new DropdownField();
            _editorSelect.style.width = 256;
            _editorSelect.bindingPath = "selectedView";
            _editorSelect.Bind(_serializedObject);
            toolbar.Add(_editorSelect);

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

            _editorSelect.RegisterValueChangedCallback(evt =>
            {
                _scrollView.contentViewport.Clear();
                var index = _editorSelect.choices.IndexOf(_editorSelect.value);
                if (index == -1) return;

                _scrollView.contentViewport.Add(_definitionViews[index]);
            });

            _scrollView = new ScrollView();
            _scrollView.style.flexGrow = 1;
            Add(_scrollView);

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
            _CreateView<CtNpcTypeDefData>("Npc Type Definitions");

            _RefreshViews();
        }

        private void _CreateView<T>(string title)
            where T : CtAbstractDefData
        {
            var view = new CtRpgDefintionEditor<T>(title);
            view.style.flexGrow = 1;
            _editorSelect.choices.Add(title);
            _definitionViews.Add(view);
        }

        private CtRpgDefintionEditor<T> _GetView<T>()
            where T : CtAbstractDefData
        {
            var type = typeof(CtRpgDefintionEditor<T>);
            foreach (var view in _definitionViews)
                if (view.GetType() == type)
                    return (CtRpgDefintionEditor<T>)view;
            return null;
        }

        private void _RefreshViews()
        {
            foreach (var view in _definitionViews)
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
            RenameDefinitions<CtBattleQuestData>("qst");
            RenameDefinitions<CtSquadDefData>("sqd");
            RenameDefinitions<CtNpcTypeDefData>("typ");
        }

        private T AddUdonSharpComponentWithUdonBehavior<T>(GameObject gameObject)
            where T : UdonSharpBehaviour
        {
            return (T)AddUdonSharpComponentWithUdonBehavior(gameObject, typeof(T));
        }

        private T AddUdonSharpComponentWithUdonBehavior<T>(GameObject gameObject, Type type)
            where T : UdonSharpBehaviour
        {
            return (T)AddUdonSharpComponentWithUdonBehavior(gameObject, type);
        }

        private UdonSharpBehaviour AddUdonSharpComponentWithUdonBehavior(GameObject gameObject, Type type)
        {
            return gameObject.AddUdonSharpComponent(type);
        }

        private void _OnGenerateGameData()
        {
            var gameData = Object.FindFirstObjectByType<CtGameData>();
            if (!gameData)
                return;

            var userDataPool = gameData.transform.Find("UserDataPool");
            if (!userDataPool)
                return;

            {
                for (int i = userDataPool.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(userDataPool.GetChild(i).gameObject);
            }

            var soGameData = new SerializedObject(gameData);

            var npcTypeLookUp = new Dictionary<CtNpcTypeDefData, CtNpcTypeDef>();
            {
                var group = gameData.transform.Find("NpcTypes");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                var view = _GetView<CtNpcTypeDefData>();
                foreach (var data in view.Data)
                {
                    var gameObject = new GameObject(data.GenerateName);
                    gameObject.transform.SetParent(group);

                    var def = AddUdonSharpComponentWithUdonBehavior<CtNpcTypeDef>(gameObject);
                    var so = new SerializedObject(def);

                    so.FindProperty("identifier").intValue = data.identifier;
                    so.FindProperty("displayName").stringValue = data.displayName;

                    so.ApplyModifiedPropertiesWithoutUndo();

                    npcTypeLookUp.Add(data, def);
                }
            }

            var behaviors = new Dictionary<CtNpcBehaviorData, CtNpcBehavior>();
            {
                var group = gameData.transform.Find("NpcBehaviors");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                var view = _GetView<CtNpcBehaviorData>();

                foreach (var data in view.Data)
                    behaviors.Add(data, _GenerateNpcBehaviors(group.transform, data));
            }

            var npcDefLookUp = new Dictionary<CtNpcDefData, CtNpcDef>();
            {
                var group = gameData.transform.Find("Npcs");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                var view = _GetView<CtNpcDefData>();
                view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var dataProp = soGameData.FindProperty("npcDefinitions");
                dataProp.arraySize = view.Data.Count;

                var npcPool = new Dictionary<GameObject, GameObject>();
                for (int i = 0; i < view.Data.Count; i++)
                {
                    var data = view.Data[i];

                    var gameObject = new GameObject(data.GenerateName);
                    gameObject.transform.SetParent(group);

                    var npcDef = AddUdonSharpComponentWithUdonBehavior<CtNpcDef>(gameObject);
                    npcDefLookUp.Add(data, npcDef);
 
                    var so = new SerializedObject(npcDef);
 
                    so.FindProperty("identifier").intValue = data.identifier;
                    so.FindProperty("displayName").stringValue = data.displayName;
                    so.FindProperty("icon").objectReferenceValue = data.icon;
                    so.FindProperty("characterLevel").intValue = data.characterLevel;
                    so.FindProperty("attributeData").stringValue = CtDataBlock.Serialize(data.professionDataBlock.DataBlock);
                    so.FindProperty("mainHandWeaponData").stringValue = CtDataBlock.Serialize(data.mainHandDataBlock.DataBlock);
                    so.FindProperty("offHandWeaponData").stringValue = CtDataBlock.Serialize(data.offHandDataBlock.DataBlock);
                    so.FindProperty("headSlotData").stringValue = CtDataBlock.Serialize(data.headArmorDataBlock.CreateDataBlock(EArmorSlot.Head));
                    so.FindProperty("chestSlotData").stringValue = CtDataBlock.Serialize(data.chestArmorDataBlock.CreateDataBlock(EArmorSlot.Chest));
                    so.FindProperty("handsSlotData").stringValue = CtDataBlock.Serialize(data.handsArmorDataBlock.CreateDataBlock(EArmorSlot.Hands));
                    so.FindProperty("legsSlotData").stringValue = CtDataBlock.Serialize(data.legsArmorDataBlock.CreateDataBlock(EArmorSlot.Legs));
                    so.FindProperty("feetSlotData").stringValue = CtDataBlock.Serialize(data.feetArmorDataBlock.CreateDataBlock(EArmorSlot.Feet));
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
                    behaviors.TryGetValue(data.behavior, out var behavior);
                    so.FindProperty("behavior").objectReferenceValue = behavior;
                    npcTypeLookUp.TryGetValue(data.npcType, out var npcTypeDef);
                    so.FindProperty("npcType").objectReferenceValue = npcTypeDef;

                    if (data.userData)
                    {
                        GameObject prefabInstance;
                        if (npcPool.TryGetValue(data.userData, out var value))
                        {
                            prefabInstance = value;
                        }
                        else
                        {
                            prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(data.userData);
                            prefabInstance.transform.SetParent(userDataPool);
                            prefabInstance.SetActive(false);

                            npcPool[data.userData] = prefabInstance;
                        }

                        so.FindProperty("userData").objectReferenceValue = prefabInstance;
                    }
                    else
                    {
                        Debug.LogWarning("Npc did not have user data defined.");
                    }

                    so.ApplyModifiedPropertiesWithoutUndo();

                    dataProp.GetArrayElementAtIndex(i).objectReferenceValue = npcDef;
                }
            }

            {
                var group = gameData.transform.Find("Weapons/Main-Hand");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                var view = _GetView<CtMainHandDefData>();
                view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var definitions = new List<CtWeaponDef>();
                var weaponPool = new Dictionary<GameObject, GameObject>();
                for (int i = 0; i < view.Data.Count; i++)
                {
                    var data = view.Data[i];
                    if (!data.attributeType)
                    {
                        Debug.LogWarning("Main-hand did not have attribute type");
                        continue;
                    }

                    if (!data.userData.userData)
                    {
                        Debug.LogWarning($"Failed to find user data for main hand (identifier={data.identifier}).");
                        continue;
                    }

                    var gameObject = new GameObject(data.GenerateName);
                    gameObject.transform.SetParent(group);

                    var def = AddUdonSharpComponentWithUdonBehavior<CtWeaponDef>(gameObject);
                    var weaponDefSo = new SerializedObject(def);

                    weaponDefSo.FindProperty("identifier").intValue = data.identifier;
                    weaponDefSo.FindProperty("displayName").stringValue = data.displayName;
                    weaponDefSo.FindProperty("icon").objectReferenceValue = data.icon;
                    weaponDefSo.FindProperty("weaponType").enumValueIndex = Convert.ToInt32(data.weaponType);
                    weaponDefSo.FindProperty("attackType").enumValueIndex = Convert.ToInt32(data.attackType);
                    weaponDefSo.FindProperty("attributeType").intValue = data.attributeType.identifier;
                    weaponDefSo.FindProperty("damageType").enumValueIndex = Convert.ToInt32(data.damageType);

                    GameObject prefabInstance;
                    if (!weaponPool.TryGetValue(data.userData.userData, out var value))
                    {
                        prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(data.userData.userData);
                        prefabInstance.transform.SetParent(userDataPool);

                        var userData = prefabInstance.GetComponent<CtWeaponAttack>();
                        var userDataSo = new SerializedObject(userData);
                        userDataSo.FindProperty("palette").intValue = data.userData.palette;
                        userDataSo.ApplyModifiedPropertiesWithoutUndo();

                        prefabInstance.GetComponent<MeshRenderer>().material = data.userData.material;

                        weaponPool[data.userData.userData] = prefabInstance;
                    }
                    else
                    {
                        prefabInstance = value;
                    }

                    weaponDefSo.FindProperty("userData").objectReferenceValue = prefabInstance;

                    weaponDefSo.ApplyModifiedPropertiesWithoutUndo();

                    definitions.Add(def);
                }

                var dataProp = soGameData.FindProperty("weaponDefinitions");
                dataProp.arraySize = definitions.Count;
                for (int i = 0; i < definitions.Count; i++)
                    dataProp.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
            }

            {
                var view = _GetView<CtOffHandDefData>();
                view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var group = gameData.transform.Find("Weapons/Off-Hand");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                var definitions = new List<CtOffHandDef>();
                for (int i = 0; i < view.Data.Count; i++)
                {
                    var data = view.Data[i];
                    if (!data.attributeType)
                    {
                        Debug.LogWarning("Off-hand did not have attribute type");
                        continue;
                    }

                    var gameObject = new GameObject(data.GenerateName);
                    gameObject.transform.SetParent(group);

                    var def = AddUdonSharpComponentWithUdonBehavior<CtOffHandDef>(gameObject);
                    var so = new SerializedObject(def);

                    so.FindProperty("identifier").intValue = data.identifier;
                    so.FindProperty("displayName").stringValue = data.displayName;
                    so.FindProperty("icon").objectReferenceValue = data.icon;
                    so.FindProperty("offHandType").enumValueIndex = Convert.ToInt32(data.offHandType);
                    so.FindProperty("attributeType").intValue = data.attributeType.identifier;
                    so.FindProperty("attributeRequirement").intValue = data.attributeRequirement;
                    so.FindProperty("minModifierStat").intValue = data.minModifierStat;
                    so.FindProperty("maxModifierStat").intValue = data.maxModifierStat;
                    so.FindProperty("rarity").enumValueIndex = Convert.ToInt32(data.rarity);

                    so.ApplyModifiedPropertiesWithoutUndo();

                    definitions.Add(def);
                }

                var dataProp = soGameData.FindProperty("offHandDefinitions");
                dataProp.arraySize = definitions.Count;
                for (int i = 0; i < definitions.Count; i++)
                    dataProp.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
            }

            {
                var view = _GetView<CtArmorSetDefData>();
                view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var group = gameData.transform.Find("Armor");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                var dataProp = soGameData.FindProperty("armorDefinitions");
                dataProp.arraySize = view.Data.Count;

                for (int i = 0; i < view.Data.Count; i++)
                {
                    var data = view.Data[i];
                    var gameObject = new GameObject(data.GenerateName);
                    gameObject.transform.SetParent(group);

                    var def = AddUdonSharpComponentWithUdonBehavior<CtArmorSetDef>(gameObject);
                    var so = new SerializedObject(def);

                    so.FindProperty("identifier").intValue = data.identifier;
                    so.FindProperty("displayName").stringValue = data.displayName;
                    so.FindProperty("allowedProfessionFlags").intValue = Convert.ToInt32(data.allowedProfessionFlags);
                    so.FindProperty("rarity").enumValueIndex = Convert.ToInt32(data.rarity);
                    so.FindProperty("headSlot").objectReferenceValue =
                        _GenerateArmorDefs(def, data.identifier, data.displayName, data.headSlot);
                    so.FindProperty("chestSlot").objectReferenceValue =
                        _GenerateArmorDefs(def, data.identifier, data.displayName, data.chestSlot);
                    so.FindProperty("handsSlot").objectReferenceValue =
                        _GenerateArmorDefs(def, data.identifier, data.displayName, data.handsSlot);
                    so.FindProperty("legsSlot").objectReferenceValue =
                        _GenerateArmorDefs(def, data.identifier, data.displayName, data.legsSlot);
                    so.FindProperty("feetSlot").objectReferenceValue =
                        _GenerateArmorDefs(def, data.identifier, data.displayName, data.feetSlot);

                    so.ApplyModifiedPropertiesWithoutUndo();

                    dataProp.GetArrayElementAtIndex(i).objectReferenceValue = def;
                }
            }

            var attributeDefs = new Dictionary<CtAttributeDefData, CtAttributeDef>();
            {
                {
                    var group = gameData.transform.Find("Professions");
                    for (int i = group.transform.childCount - 1; i >= 0; i--)
                        Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                    var view = _GetView<CtProfessionDefData>();
                    view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                    var dataProp = soGameData.FindProperty("professionDefinitions");
                    dataProp.arraySize = view.Data.Count;

                    var attrDataProp = soGameData.FindProperty("attributeDefinitions");
                    attrDataProp.arraySize = 0;

                    for (int i = 0; i < view.Data.Count; i++)
                    {
                        var data = view.Data[i];
                        var gameObject = new GameObject(data.GenerateName);
                        gameObject.transform.SetParent(group);

                        var def = AddUdonSharpComponentWithUdonBehavior<CtProfessionDef>(gameObject);
                        var so = new SerializedObject(def);

                        so.FindProperty("identifier").intValue = data.identifier;
                        so.FindProperty("displayName").stringValue = data.displayName;
                        so.FindProperty("icon").objectReferenceValue = data.icon;
                        so.FindProperty("theme").colorValue = data.theme;

                        var attributes = data.Attributes;
                        int start = attrDataProp.arraySize;
                        attrDataProp.arraySize += attributes.Length;

                        var attributesProp = so.FindProperty("attributes");
                        attributesProp.arraySize = attributes.Length;
                        for (int j = 0; j < attributes.Length; j++)
                        {
                            var arrayIndexProp = attributesProp.GetArrayElementAtIndex(j);
                            var attributeDefData = attributes[j];
                            var attributeDef = _GenerateAttributes(gameObject.transform, attributeDefData);
                            attributeDefs.Add(attributeDefData, attributeDef);
                            arrayIndexProp.objectReferenceValue = attributeDef;

                            attrDataProp.GetArrayElementAtIndex(start + j).objectReferenceValue = attributeDef;
                        }

                        so.ApplyModifiedPropertiesWithoutUndo();

                        dataProp.GetArrayElementAtIndex(i).objectReferenceValue = def;
                    }
                }

                {
                    var view = _GetView<CtSkillDefData>();
                    view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                    var definitions = new List<CtSkillDef>();
                    for (int i = 0; i < view.Data.Count; i++)
                    {
                        var data = view.Data[i];
                        if (!data.attributeType)
                        {
                            Debug.LogWarning("Skill did not have attribute type.");
                            continue;
                        }

                        if (!data.script)
                        {
                            Debug.LogWarning("Skill did not have script type.");
                            continue;
                        }

                        var gameObject = new GameObject(data.GenerateName);
                        gameObject.transform.SetParent(attributeDefs[data.attributeType].transform);

                        var def = AddUdonSharpComponentWithUdonBehavior<CtSkillDef>(gameObject, data.script.GetClass());
                        var so = new SerializedObject(def);

                        so.FindProperty("identifier").intValue = data.identifier;
                        so.FindProperty("displayName").stringValue = data.displayName;
                        so.FindProperty("icon").objectReferenceValue = data.icon;
                        so.FindProperty("isWeaponSkill").boolValue = data.isWeaponSkill;
                        so.FindProperty("attributeType").intValue = data.attributeType.identifier;
                        so.FindProperty("targetType").intValue = Convert.ToInt32(data.targetType);
                        so.FindProperty("subType").enumValueIndex = Convert.ToInt32(data.subType);
                        so.FindProperty("isBeneficial").boolValue = data.isBeneficial;
                        so.FindProperty("skillType").intValue = Convert.ToInt32(data.skillType);
                        so.FindProperty("cost").intValue = data.cost;
                        so.FindProperty("rechargeTime").intValue = data.rechargeTime;

                        so.ApplyModifiedPropertiesWithoutUndo();

                        definitions.Add(def);
                    }

                    var conditionsGroup = gameData.transform.Find("Conditions");
                    foreach (var condition in conditionsGroup.GetComponentsInChildren<CtSkillDef>(true))
                        definitions.Add(condition);

                    var dataProp = soGameData.FindProperty("skillDefinitions");
                    dataProp.arraySize = definitions.Count;
                    for (int i = 0; i < definitions.Count; i++)
                        dataProp.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];

                    CtSkillDefFuncs.AssignSkillFlags();
                }
            }

            var squadDefLookUp = new Dictionary<CtSquadDefData, CtSquadDef>();
            {
                {
                    var group = gameData.transform.Find("Squads");
                    for (int i = group.transform.childCount - 1; i >= 0; i--)
                        Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                    var view = _GetView<CtSquadDefData>();
                    view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                    var definitions = new List<CtSquadDef>();
                    var squadUserDataPool = new Dictionary<GameObject, GameObject>();
                    for (int i = 0; i < view.Data.Count; i++)
                    {
                        var data = view.Data[i];
                        var gameObject = new GameObject(data.GenerateName);
                        gameObject.transform.SetParent(group);

                        var def = AddUdonSharpComponentWithUdonBehavior<CtSquadDef>(gameObject);
                        squadDefLookUp.Add(data, def);
                        var so = new SerializedObject(def);

                        so.FindProperty("identifier").intValue = data.identifier;

                        if (data.userData.userData)
                        {
                            GameObject prefabInstance;
                            if (!squadUserDataPool.ContainsKey(data.userData.userData))
                            {
                                prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(data.userData.userData);
                                prefabInstance.transform.SetParent(userDataPool);
                                squadUserDataPool[data.userData.userData] = prefabInstance;
                            }
                            else
                            {
                                prefabInstance = squadUserDataPool[data.userData.userData];
                            }

                            so.FindProperty("userData").objectReferenceValue = prefabInstance;
                        }
                        else
                        {
                            Debug.LogWarning("Squad did not have user data defined.");
                        }

                        var npcDefs = data.npcDataBlock.NpcDefs;

                        var npcDefsProp = so.FindProperty("npcDefs");
                        npcDefsProp.arraySize = npcDefs.Length;
                        for (int j = 0; j < npcDefs.Length; j++)
                        {
                            var arrayIndexProp = npcDefsProp.GetArrayElementAtIndex(j);
                            arrayIndexProp.objectReferenceValue = npcDefLookUp[npcDefs[j]];
                        }

                        so.ApplyModifiedPropertiesWithoutUndo();

                        definitions.Add(def);
                    }

                    var dataProp = soGameData.FindProperty("squadDefinitions");
                    dataProp.arraySize = definitions.Count;
                    for (int i = 0; i < definitions.Count; i++)
                        dataProp.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
                }
            }

            {
                {
                    var group = gameData.transform.Find("Quests/Battle");
                    for (int i = group.transform.childCount - 1; i >= 0; i--)
                        Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                    var view = _GetView<CtBattleQuestData>();
                    view.Data.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                    var dataProp = soGameData.FindProperty("questDefinitions");
                    dataProp.arraySize = view.Data.Count;

                    for (int i = 0; i < view.Data.Count; i++)
                    {
                        var data = view.Data[i];
                        var gameObject = new GameObject(data.GenerateName);
                        gameObject.transform.SetParent(group);

                        var def = AddUdonSharpComponentWithUdonBehavior<CtBattleQuest>(gameObject);
                        var so = new SerializedObject(def);

                        so.FindProperty("identifier").intValue = data.identifier;
                        so.FindProperty("displayName").stringValue = data.displayName;
                        so.FindProperty("icon").objectReferenceValue = data.icon;
                        so.FindProperty("levelReq").intValue = data.levelReq;

                        var squadCategories = new List<CtSquadCategory>();
                        var squadCategory = _GenerateSquadCategory(def, data.squadCategory0, ref squadDefLookUp);
                        if (squadCategory)
                            squadCategories.Add(squadCategory);
                        squadCategory = _GenerateSquadCategory(def, data.squadCategory1, ref squadDefLookUp);
                        if (squadCategory)
                            squadCategories.Add(squadCategory);
                        squadCategory = _GenerateSquadCategory(def, data.squadCategory2, ref squadDefLookUp);
                        if (squadCategory)
                            squadCategories.Add(squadCategory);

                        var squadCategoriesProp = so.FindProperty("squadCategories");
                        squadCategoriesProp.arraySize = squadCategories.Count;
                        for (int j = 0; j < squadCategories.Count; j++)
                        {
                            var arrayIndexProp = squadCategoriesProp.GetArrayElementAtIndex(j);
                            arrayIndexProp.objectReferenceValue = squadCategories[j];
                        }

                        so.ApplyModifiedPropertiesWithoutUndo();

                        dataProp.GetArrayElementAtIndex(i).objectReferenceValue = def;
                    }
                }
            }

            soGameData.ApplyModifiedProperties();

            CtSingletonEditor.AssignSingletons(CtSingletonEditor.GetCurrentSingletonTypes(), gameData.gameObject);
        }

        private CtSquadCategory _GenerateSquadCategory(CtBattleQuest battleQuest, CtSquadCategoryDataBlock data,
            ref Dictionary<CtSquadDefData, CtSquadDef> squadDefLookUp)
        {
            var squadDefs = data.SquadDefs;
            if (squadDefs.Length == 0)
                return null;

            var squadCategory = AddUdonSharpComponentWithUdonBehavior<CtSquadCategory>(battleQuest.gameObject);
            var so = new SerializedObject(squadCategory);

            var squadDefsProp = so.FindProperty("squadDefs");
            squadDefsProp.arraySize = squadDefs.Length;
            for (int i = 0; i < squadDefs.Length; i++)
            {
                var arrayIndexProp = squadDefsProp.GetArrayElementAtIndex(i);
                if (squadDefLookUp.TryGetValue(squadDefs[i], out var value))
                    arrayIndexProp.objectReferenceValue = value;
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            return squadCategory;
        }

        private CtArmorSlotDef _GenerateArmorDefs(
            CtArmorSetDef armorSetDef, ushort identifier, string displayName, CtArmorSlotData data)
        {
            var def = AddUdonSharpComponentWithUdonBehavior<CtArmorSlotDef>(armorSetDef.gameObject);
            var so = new SerializedObject(def);

            so.FindProperty("identifier").intValue = identifier;
            so.FindProperty("displayName").stringValue = 
                string.IsNullOrEmpty(data.suffix) ? displayName : $"{displayName} {data.suffix}";
            so.FindProperty("icon").objectReferenceValue = data.icon;
            so.FindProperty("armorRating").intValue = data.armorRating;
            so.FindProperty("armorRatingBonus").intValue = data.armorRatingBonus;
            so.FindProperty("armorRatingBonusType").enumValueIndex = Convert.ToInt32(data.armorRatingBonusType);
            so.FindProperty("energyRegenerationBonus").intValue = data.energyRegenerationBonus;
            so.FindProperty("energyIncreaseBonus").intValue = data.energyIncreaseBonus;
            so.FindProperty("healthIncreaseBonus").intValue = data.healthIncreaseBonus;

            so.FindProperty("armorSet").objectReferenceValue = armorSetDef;

            so.ApplyModifiedPropertiesWithoutUndo();

            return def;
        }

        private CtNpcBehavior _GenerateNpcBehaviors(Transform group, CtNpcBehaviorData data)
        {
            var gameObject = new GameObject(data.GenerateName);
            gameObject.transform.SetParent(group);

            var def = AddUdonSharpComponentWithUdonBehavior<CtNpcBehavior>(gameObject);
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

            var attributeDef = AddUdonSharpComponentWithUdonBehavior<CtAttributeDef>(gameObject);
            var so = new SerializedObject(attributeDef);

            so.FindProperty("identifier").intValue = data.identifier;
            so.FindProperty("displayName").stringValue = data.displayName;
            so.FindProperty("icon").objectReferenceValue = data.icon;

            so.ApplyModifiedPropertiesWithoutUndo();

            return attributeDef;
        }
    }
}