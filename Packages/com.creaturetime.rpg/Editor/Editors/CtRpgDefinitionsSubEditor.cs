
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
    [Serializable]
    public class CtRpgDefAsset : ScriptableObject
    {
        public List<CtAttributeDefData> attributeDefs = new List<CtAttributeDefData>();
        public List<CtProfessionDefData> professionDefs = new List<CtProfessionDefData>();
        public List<CtMainHandDefData> mainHandDefs = new List<CtMainHandDefData>();
        public List<CtOffHandDefData> offHandDefs = new List<CtOffHandDefData>();
        public List<CtNpcDefData> npcDefs = new List<CtNpcDefData>();
    }

    public class CtRpgDefinitionsSubEditor : CtCreatureTimeSubEditor
    {
        private const string RpgDefsLocation = "CreatureTime/Editor/Data/Rpg";
        private const string RpgDefsFileName = "RpgDefs.asset";
        private const string RpgDefsFullPath = "Assets/" + RpgDefsLocation + "/" + RpgDefsFileName;

        public override string Name => "RPG Def Editor";

        private CtRpgDefAsset _data;

        private MultiColumnListView _npcDefsView;
        private MultiColumnListView _mainHandDefsView;
        private MultiColumnListView _offHandDefsView;
        private MultiColumnListView _professionDefsView;
        private MultiColumnListView _attributeDefsView;

        public CtRpgDefinitionsSubEditor()
        {
            _data = ScriptableObject.CreateInstance<CtRpgDefAsset>();

            // var assets = AssetDatabase.FindAssets($"t:{nameof(CtNpcDefData)}", 
            //     new string[] { "Assets/CreatureTime/Editor/Data/Rpg" });
            // foreach (var asset in assets)
            // {
            //     var assetPath = AssetDatabase.GUIDToAssetPath(asset);
            //     _data.npcDefs.Add(AssetDatabase.LoadAssetAtPath<CtNpcDefData>(assetPath));
            // }
            //
            // assets = AssetDatabase.FindAssets($"t:{nameof(CtMainHandDefData)}", 
            //     new string[] { "Assets/CreatureTime/Editor/Data/Rpg" });
            // foreach (var asset in assets)
            // {
            //     var assetPath = AssetDatabase.GUIDToAssetPath(asset);
            //     _data.mainHandDefs.Add(AssetDatabase.LoadAssetAtPath<CtMainHandDefData>(assetPath));
            // }
            //
            // assets = AssetDatabase.FindAssets($"t:{nameof(CtOffHandDefData)}", 
            //     new string[] { "Assets/CreatureTime/Editor/Data/Rpg" });
            // foreach (var asset in assets)
            // {
            //     var assetPath = AssetDatabase.GUIDToAssetPath(asset);
            //     _data.offHandDefs.Add(AssetDatabase.LoadAssetAtPath<CtOffHandDefData>(assetPath));
            // }
            //
            // assets = AssetDatabase.FindAssets($"t:{nameof(CtOffHandDefData)}", 
            //     new string[] { "Assets/CreatureTime/Editor/Data/Rpg" });
            // foreach (var asset in assets)
            // {
            //     var assetPath = AssetDatabase.GUIDToAssetPath(asset);
            //     _data.offHandDefs.Add(AssetDatabase.LoadAssetAtPath<CtOffHandDefData>(assetPath));
            // }

            _FindDefinitionAssets(ref _data.attributeDefs);
            _FindDefinitionAssets(ref _data.professionDefs);
            _FindDefinitionAssets(ref _data.mainHandDefs);
            _FindDefinitionAssets(ref _data.offHandDefs);
            _FindDefinitionAssets(ref _data.npcDefs);

            var so = new SerializedObject(_data);

            // _CreateCateogry("Npc Definitions");
            //
            // // Provide the list view with an explicit height for every row
            // // so it can calculate how many items to actually display
            // _npcDefsView = new MultiColumnListView
            // {
            //     showBoundCollectionSize = false,
            //     virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            //     itemsSource = _data.npcDefs,
            //     // Enables multiple selection using shift or ctrl/cmd keys.
            //     selectionType = SelectionType.Multiple,
            //     style = { minWidth = 256, minHeight = 128 }
            // };
            //
            // {
            //     var subPropPath = nameof(_data.npcDefs);
            //     var type = typeof(CtNpcDefData);
            //     _npcDefsView.columns.Add(new Column
            //     {
            //         minWidth = 32, maxWidth = 32,
            //         makeCell = () => new Label(),
            //         bindCell = (element, i) => { }
            //     });
            //     int variant = 0;
            //     _PopulateRow(_npcDefsView, so, type, subPropPath, ref variant);
            //     // foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            //     //     _GenerateColumn(_npcDefsView, field.FieldType, 
            //     //         ObjectNames.NicifyVariableName(field.GetNiceName()), so, subPropPath, field.Name);
            // }
            //
            // Add(_npcDefsView);

            // _FindDefinitionAssets(ref _data.npcDefs);

            _CreateDefinitionsView(
                ref _npcDefsView, so, typeof(CtNpcDefData), "Npc Definitions", _data.npcDefs, 
                nameof(_data.npcDefs));

            // _FindDefinitionAssets(ref _data.mainHandDefs);

            _CreateDefinitionsView(
                ref _mainHandDefsView, so, typeof(CtMainHandDefData), "Main Hand Definitions", _data.mainHandDefs, 
                nameof(_data.mainHandDefs));

            // _CreateCateogry("Main Hand Definitions");
            //
            // // Provide the list view with an explicit height for every row
            // // so it can calculate how many items to actually display
            // _mainHandDefsView = new MultiColumnListView
            // {
            //     showBoundCollectionSize = false,
            //     virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            //     itemsSource = _data.mainHandDefs,
            //     // Enables multiple selection using shift or ctrl/cmd keys.
            //     selectionType = SelectionType.Multiple,
            //     style = { minWidth = 256, minHeight = 128 }
            // };
            //
            // {
            //     var subPropPath = nameof(_data.mainHandDefs);
            //     var type = typeof(CtMainHandDefData);
            //     _mainHandDefsView.columns.Add(new Column
            //     {
            //         minWidth = 32, maxWidth = 32,
            //         makeCell = () => new Label(),
            //         bindCell = (element, i) => { }
            //     });
            //     int variant = 0;
            //     _PopulateRow(_mainHandDefsView, so, type, subPropPath, ref variant);
            //     // foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            //     //     _GenerateColumn(_mainHandDefsView, field.FieldType, 
            //     //         ObjectNames.NicifyVariableName(field.GetNiceName()), so, subPropPath, field.Name, 0);
            // }
            //
            // Add(_mainHandDefsView);

            // _FindDefinitionAssets(ref _data.offHandDefs);

            _CreateDefinitionsView(
                ref _offHandDefsView, so, typeof(CtOffHandDefData), "Off Hand Definitions", _data.offHandDefs, 
                nameof(_data.offHandDefs));

            _CreateDefinitionsView(
                ref _professionDefsView, so, typeof(CtProfessionDefData), "Profession Definitions", _data.professionDefs, 
                nameof(_data.professionDefs));

            _CreateDefinitionsView(
                ref _attributeDefsView, so, typeof(CtAttributeDefData), "Attribute Definitions", _data.attributeDefs, 
                nameof(_data.attributeDefs));

            // _FindDefinitionAssets(_data.offHandDefs);
            //
            // _CreateDefinitionsView(
            //     ref _offHandDefsView, so, typeof(CtOffHandDefData), "Off Hand Definitions", _data.offHandDefs, 
            //     nameof(_data.offHandDefs));

            // _CreateCateogry("Off Hand Definitions");
            //
            // // Provide the list view with an explicit height for every row
            // // so it can calculate how many items to actually display
            // _offHandDefsView = new MultiColumnListView
            // {
            //     showBoundCollectionSize = false,
            //     virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            //     itemsSource = _data.offHandDefs,
            //     // Enables multiple selection using shift or ctrl/cmd keys.
            //     selectionType = SelectionType.Multiple,
            //     style = { minWidth = 256, minHeight = 128 }
            // };
            //
            // {
            //     var subPropPath = nameof(_data.offHandDefs);
            //     var type = typeof(CtOffHandDefData);
            //     _offHandDefsView.columns.Add(new Column
            //     {
            //         minWidth = 32, maxWidth = 32,
            //         makeCell = () => new Label(),
            //         bindCell = (element, i) => { }
            //     });
            //     int variant = 0;
            //     _PopulateRow(_offHandDefsView, so, type, subPropPath, ref variant);
            //     // foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            //     //     _GenerateColumn(_offHandDefsView, field.FieldType, 
            //     //         ObjectNames.NicifyVariableName(field.GetNiceName()), so, subPropPath, field.Name, 0);
            // }
            //
            // Add(_offHandDefsView);

            var footer = new Toolbar();

            var generateGameData = new Button
            {
                text = "Generate Game Data"
            };
            generateGameData.clicked += _OnGenerateGameData;
            footer.Add(generateGameData);

            Add(footer);
        }

        private void _FindDefinitionAssets<T>(ref List<T> data)
            where T : ScriptableObject
        {
            data.Clear();
        
            Type type = typeof(T);
            var assets = AssetDatabase.FindAssets($"t:{type.Name}", 
                new string[] { "Assets/CreatureTime/Editor/Data/Rpg" });
            foreach (var asset in assets)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(asset);
                data.Add(AssetDatabase.LoadAssetAtPath<T>(assetPath));
            }
        }

        private void _CreateDefinitionsView<T>(ref MultiColumnListView view, SerializedObject serializedObject, Type type, 
            string title, List<T> data, string subPropPath)
        {
            _CreateCateogry(title);

            view = new MultiColumnListView
            {
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                itemsSource = data,
                selectionType = SelectionType.Multiple,
                style = { minWidth = 256, minHeight = 128 }
            };

            view.columns.Add(new Column
            {
                minWidth = 32, maxWidth = 32,
                makeCell = () => new Label(),
                bindCell = (element, i) => { }
            });

            int variant = 0;
            _PopulateRow(view, serializedObject, type, subPropPath, ref variant);

            Add(view);
        }

        private void _PopulateRow(MultiColumnListView view, SerializedObject so, Type type, string subPropPath, ref int variant, string parentProp = "")
        {
            int levelVariant = variant;
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var propPath = string.IsNullOrEmpty(parentProp) ? field.Name : $"{parentProp}.{field.Name}";
                // if (field.FieldType.IsArray)
                // {
                //     variant += 1;
                //     var elementType = field.FieldType.GetElementType();
                //     _PopulateRow(view, so, elementType, subPropPath, ref variant, propPath);
                // }
                if (field.FieldType.IsStruct() && field.FieldType != typeof(Color))
                {
                    variant += 1;
                    _PopulateRow(view, so, field.FieldType, subPropPath, ref variant, propPath);
                }
                else
                {
                    _GenerateColumn(view, field.FieldType, 
                        ObjectNames.NicifyVariableName(field.GetNiceName()), so, subPropPath, propPath, levelVariant);
                }
            }
        }

        private void _OnGenerateGameData()
        {
            var gameData = Object.FindFirstObjectByType<CtGameData>();
            if (!gameData)
            {
                return;
            }

            {
                _data.npcDefs.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var group = gameData.transform.Find("Npcs/Test");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                foreach (var data in _data.npcDefs)
                {
                    var generatedName = $"{data.identifier:00000}_{data.displayName.Replace(' ', '-')}";

                    string assetPath = AssetDatabase.GetAssetPath(data);
                    AssetDatabase.RenameAsset(assetPath, $"npc_{generatedName.ToLower()}");

                    var gameObject = new GameObject(generatedName);
                    gameObject.transform.SetParent(group);

                    var so = new SerializedObject(gameObject.AddComponent<CtNpcDef>());

                    so.FindProperty("identifier").intValue = data.identifier;
                    so.FindProperty("displayName").stringValue = data.displayName;
                    so.FindProperty("icon").objectReferenceValue = data.icon;
                    so.FindProperty("mainHandWeaponData").ulongValue = data.mainHandDataBlock.DataBlock;
                    so.FindProperty("offHandWeaponData").ulongValue = data.offHandDataBlock.DataBlock;

                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            {
                _data.mainHandDefs.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var group = gameData.transform.Find("Weapons/Main-Hand/Test");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                foreach (var data in _data.mainHandDefs)
                {
                    var generatedName = $"{data.identifier:00000}_{data.displayName.Replace(' ', '-')}";

                    string assetPath = AssetDatabase.GetAssetPath(data);
                    AssetDatabase.RenameAsset(assetPath, $"mainhand_{generatedName.ToLower()}");

                    var gameObject = new GameObject(generatedName);
                    gameObject.transform.SetParent(group);

                    var weaponDefSo = new SerializedObject(gameObject.AddComponent<CtWeaponDef>());
                    weaponDefSo.FindProperty("identifier").intValue = data.identifier;
                    weaponDefSo.FindProperty("displayName").stringValue = data.displayName;
                    weaponDefSo.FindProperty("icon").objectReferenceValue = data.icon;
                    weaponDefSo.FindProperty("attributeType").intValue = data.attributeType;

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
                _data.offHandDefs.Sort((a, b) => a.identifier.CompareTo(b.identifier));

                var group = gameData.transform.Find("Weapons/Off-Hand/Test");
                for (int i = group.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(group.transform.GetChild(i).gameObject);

                foreach (var data in _data.offHandDefs)
                {
                    var generatedName = $"{data.identifier:00000}_{data.displayName.Replace(' ', '-')}";

                    string assetPath = AssetDatabase.GetAssetPath(data);
                    AssetDatabase.RenameAsset(assetPath, $"offhand_{generatedName.ToLower()}");

                    var gameObject = new GameObject(generatedName);
                    gameObject.transform.SetParent(group);

                    var so = new SerializedObject(gameObject.AddComponent<CtOffHandDef>());

                    so.FindProperty("identifier").intValue = data.identifier;
                    so.FindProperty("displayName").stringValue = data.displayName;
                    so.FindProperty("icon").objectReferenceValue = data.icon;
                    so.FindProperty("attributeType").intValue = data.attributeType;
                    so.FindProperty("attributeRequirement").intValue = data.attributeRequirement;
                    so.FindProperty("minModifierStat").intValue = data.minModifierStat;
                    so.FindProperty("maxModifierStat").intValue = data.maxModifierStat;
                    so.FindProperty("rarity").enumValueIndex = Convert.ToInt32(data.rarity);

                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private void _CreateCateogry(string text)
        {
            var toolbar = new Toolbar();
            Add(toolbar);

            var header = new Label
            {
                text = text,
                style =
                {
                    backgroundColor = Color.black,
                    color = Color.white,
                    fontSize = 14
                }
            };
            toolbar.Add(header);
        }

        private static void _GenerateColumn(MultiColumnListView view, Type type, string title, 
            SerializedObject serializedObject, string subPropPath, string bindingPath, int variant)
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
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
                };
            }
            else if (type.IsSubclassOf(typeof(Object)))
            {
                bindCellFunc = (element, i) =>
                {
                    var field = (ObjectField)element;
                    field.bindingPath = bindingPath;
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
                };
            }
            else
            {
                var typeToBindCell = new Dictionary<Type, Action<VisualElement, int>>();
                typeToBindCell.Add(typeof(Color), (element, i) =>
                {
                    var field = (ColorField)element;
                    field.bindingPath = bindingPath;
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
                });
                typeToBindCell.Add(typeof(string), (element, i) =>
                {
                    var field = (TextField)element;
                    field.bindingPath = bindingPath;
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
                });
                typeToBindCell.Add(typeof(bool), (element, i) =>
                {
                    var field = (Toggle)element;
                    field.bindingPath = bindingPath;
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
                });
                typeToBindCell.Add(typeof(ushort), (element, i) =>
                {
                    var field = (UnsignedIntegerField)element;
                    field.bindingPath = bindingPath;
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
                });
                typeToBindCell.Add(typeof(uint), (element, i) =>
                {
                    var field = (UnsignedIntegerField)element;
                    field.bindingPath = bindingPath;
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
                });
                typeToBindCell.Add(typeof(ulong), (element, i) =>
                {
                    var field = (UnsignedIntegerField)element;
                    field.bindingPath = bindingPath;
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
                });
                typeToBindCell.Add(typeof(short), (element, i) =>
                {
                    var field = (IntegerField)element;
                    field.bindingPath = bindingPath;
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
                });
                typeToBindCell.Add(typeof(int), (element, i) =>
                {
                    var field = (IntegerField)element;
                    field.bindingPath = bindingPath;
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
                });
                typeToBindCell.Add(typeof(long), (element, i) =>
                {
                    var field = (IntegerField)element;
                    field.bindingPath = bindingPath;
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
                });
                typeToBindCell.Add(typeof(float), (element, i) =>
                {
                    var field = (FloatField)element;
                    field.bindingPath = bindingPath;
                    var so = new SerializedObject(
                        serializedObject.FindProperty(subPropPath).GetArrayElementAtIndex(i).objectReferenceValue);
                    field.Bind(so);
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
            view.columns.Add(column);
        }
    }
}