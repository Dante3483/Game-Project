using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class AtlasEditorWindow : TwoPaneEditorWindow
{
    private enum SearchMode
    {
        Global = 0,
        Collections = 1,
        Collection = 2,
    }

    #region Private fields
    private List<AtlasSO> _atlases;
    private Dictionary<AtlasSO, List<SerializedProperty>> _collectionsPropertiesByAtlas;
    private Dictionary<string, Type> _collectionElementTypeByCollectionName;
    private Dictionary<string, List<Object>> _collectionObjectsByCollectionName;

    private AtlasSO _currentAtlas;
    private SerializedProperty _currentCollectionProperty;
    private List<Object> _currentCollectionObjects;
    private List<Object> _collectionObjectsAfterSearch;
    private int _selectedIndex;

    private ScriptableObject _newScriptableObject;
    private string _newObjectName;
    private Type _newObjectType;
    private string _pathToSaveNewObject;
    private SearchMode _currentSearchMode;
    private bool _isSearching;

    private ToolbarSearchField _searchField;
    private ListView _listView;
    private Button _deleteObjectButton;
    private VisualElement _searchToolbar;
    private VisualElement _backToolbar;
    private VisualElement _deleteObjectToolbar;

    private static readonly string _styleResource = StaticInfo.StyleSheetsDirectory + "AtlasEditorWindowStyleSheet";
    private static readonly string _ussAtlasWindow = "atlas-window";
    private static readonly string _ussToolbar = _ussAtlasWindow + "__toolbar";
    private static readonly string _ussSearchToolbar = _ussAtlasWindow + "__toolbar-search";
    private static readonly string _ussBackToolbar = _ussAtlasWindow + "__toolbar-back";
    private static readonly string _ussDeleteObjectToolbar = _ussAtlasWindow + "__toolbar-delete-object";
    private static readonly string _ussLeftPane = _ussAtlasWindow + "__left-pane";
    private static readonly string _ussRightPane = _ussAtlasWindow + "__right-pane";
    private static readonly string _ussSearchField = _ussAtlasWindow + "__search-field";
    private static readonly string _ussListView = _ussAtlasWindow + "__list-view";
    private static readonly string _ussToolbarButton = _ussAtlasWindow + "__toolbar-button";
    private static readonly string _ussContentButton = _ussAtlasWindow + "__content-button";
    #endregion

    #region Public fields

    #endregion

    #region Properties

    #endregion

    #region Methods
    private void OnDestroy()
    {
        if (_newScriptableObject != null)
        {
            DestroyImmediate(_newScriptableObject);
        }
    }

    [MenuItem("Utils/Atlases")]
    public static void OpenWindow()
    {
        AtlasEditorWindow window = GetWindow<AtlasEditorWindow>();
        window.titleContent = new GUIContent("Atlases");
        window.minSize = new Vector2(700, 500);
        window.Show();
    }

    private void Update()
    {
        _searchToolbar.style.width = _leftPane.style.width;
    }

    public override void InitializeEditorWindow()
    {
        base.InitializeEditorWindow();
        _rightPane = new ScrollView(ScrollViewMode.Vertical);
        _searchField = new ToolbarSearchField();
        _listView = new ListView();
        _searchToolbar = new VisualElement();
        _backToolbar = new VisualElement();
        _deleteObjectToolbar = new VisualElement();

        _root.styleSheets.Add(Resources.Load<StyleSheet>(_styleResource));
        _toolbar.AddToClassList(_ussToolbar);
        _searchToolbar.AddToClassList(_ussSearchToolbar);
        _backToolbar.AddToClassList(_ussBackToolbar);
        _deleteObjectToolbar.AddToClassList(_ussDeleteObjectToolbar);
        _splitView.AddToClassList(_ussAtlasWindow);
        _leftPane.AddToClassList(_ussLeftPane);
        _rightPane.AddToClassList(_ussRightPane);
        _searchField.AddToClassList(_ussSearchField);
        _listView.AddToClassList(_ussListView);

        _atlases = new List<AtlasSO>();
        _collectionsPropertiesByAtlas = new Dictionary<AtlasSO, List<SerializedProperty>>();
        _collectionObjectsByCollectionName = new Dictionary<string, List<Object>>();
        _collectionElementTypeByCollectionName = new Dictionary<string, Type>();
        _collectionObjectsAfterSearch = new List<Object>();

        AddSearchObjectsField();
        _toolbar.Add(_backToolbar);
        AddDeleteObjectButton();
    }

    public override void ComposeLeftPane()
    {
        Button button = new Button(() =>
        {
            _rightPane.Clear();
            string path = AssetDatabase.GetAssetPath(_currentCollectionObjects[0]);
            path = Path.GetDirectoryName(path) + '\\';
            Debug.Log(path);
            _newScriptableObject = CreateInstance<SolidBlockSO>();
            _newScriptableObject.name = "NewBlock";
            InspectorElement inspectorElement = new InspectorElement(_newScriptableObject);
            _rightPane.Add(inspectorElement);
            Button create = new Button(() =>
            {
                string[] existingAssets = AssetDatabase.FindAssets(_newScriptableObject.name, new[] { path });
                if (existingAssets.Length > 0)
                {
                    _newScriptableObject.name += existingAssets.Length;
                }
                AssetDatabase.CreateAsset(_newScriptableObject, path + _newScriptableObject.name + ".asset");
                AssetDatabase.SaveAssets();
                //_leftPane.Q<ListView>()?.Rebuild();

                SerializedObject serializedObject = new SerializedObject(_atlases[0]);
                SerializedProperty collection = serializedObject.FindProperty("_solidBlocks");
                collection.InsertArrayElementAtIndex(collection.arraySize);
                collection.GetArrayElementAtIndex(collection.arraySize - 1).objectReferenceValue = _newScriptableObject;
                serializedObject.ApplyModifiedProperties();
                _currentCollectionObjects.Add(_newScriptableObject);
                //_leftPane.Q<ListView>()?.Rebuild();
                _newScriptableObject = CreateInstance<SolidBlockSO>();
                _newScriptableObject.name = "NewBlock";
            });
            create.text = "Create";
            _rightPane.Add(create);
        });
        button.text = "Create";
        //_root.Add(button);
        GetAllData();
        DisplayAllAtlases();
    }

    private void ClearContent()
    {
        _leftPane.Clear();
        _rightPane.Clear();
        _newScriptableObject = null;
    }

    private void ResetSearch()
    {
        _isSearching = false;
        _searchField.SetValueWithoutNotify("");
        _listView.itemsSource = _currentCollectionObjects;
        _listView.RefreshItems();
    }

    private void GetAllData()
    {
        string[] allAtlasesGuids = AssetDatabase.FindAssets("t:AtlasSO");
        foreach (var atlasGuid in allAtlasesGuids)
        {
            AtlasSO atlas = AssetDatabase.LoadAssetAtPath<AtlasSO>(AssetDatabase.GUIDToAssetPath(atlasGuid));
            _atlases.Add(atlas);
            GetAllAtlasCollections(atlas);
        }
    }

    private void GetAllAtlasCollections(AtlasSO atlas)
    {
        _collectionsPropertiesByAtlas.Add(atlas, new List<SerializedProperty>());
        SerializedProperty iterator = new SerializedObject(atlas).GetIterator();
        iterator.NextVisible(true);
        while (iterator.NextVisible(false))
        {
            if (iterator.isArray && iterator.propertyType == SerializedPropertyType.Generic)
            {
                SerializedProperty collectionProperty = iterator.Copy();
                _collectionsPropertiesByAtlas[atlas].Add(collectionProperty);
                GetAllCollectionObjects(iterator);
            }
        }
    }

    private void GetAllCollectionObjects(SerializedProperty collectionProperty)
    {
        _collectionObjectsByCollectionName.Add(collectionProperty.name, new List<Object>());
        _collectionElementTypeByCollectionName.Add(collectionProperty.name, GetCollectionElementType(collectionProperty));
        for (int i = 0; i < collectionProperty.arraySize; i++)
        {
            Object collectionObject = collectionProperty.GetArrayElementAtIndex(i).objectReferenceValue;
            if (collectionObject == null)
            {
                collectionProperty.DeleteArrayElementAtIndex(i--);
                collectionProperty.serializedObject.ApplyModifiedProperties();
                continue;
            }
            _collectionObjectsByCollectionName[collectionProperty.name].Add(collectionObject);
        }
    }
    
    private void DisplayAllAtlases()
    {
        ClearContent();
        _currentSearchMode = SearchMode.Global;
        foreach (AtlasSO atlas in _atlases)
        {
            AddAtlasButton(atlas);
        }
    }

    private void DisplayAtlas(AtlasSO atlas)
    {
        ClearContent();
        _currentSearchMode = SearchMode.Collections;
        List<SerializedProperty> collectionsProperties = _collectionsPropertiesByAtlas[atlas];
        foreach (SerializedProperty collectionProperty in collectionsProperties)
        {
            AddCollectionButton(atlas, collectionProperty);
        }

        InspectorElement atlasInspectorElement = new InspectorElement(atlas);
        _rightPane.Add(atlasInspectorElement);
    }

    private void DisplayCollection(SerializedProperty collectionProperty)
    {
        ClearContent();
        _currentSearchMode = SearchMode.Collection;
        _currentCollectionObjects = _collectionObjectsByCollectionName[collectionProperty.name];

        AddListViewForCollection(_currentCollectionObjects);
        AddCreateObjectButton();
    }

    private void DisplaySearchResult()
    {
        ClearContent();
        AddListViewForCollection(_collectionObjectsAfterSearch);
    }

    private void AddAtlasButton(AtlasSO atlas)
    {
        Button atlasButton = CreateButton(GetCorrectNameByObject(atlas), _ussContentButton);
        atlasButton.clicked += () =>
        {
            AddBackToAtlasesButton();

            _currentAtlas = atlas;
            DisplayAtlas(atlas);
        };
        _leftPane.Add(atlasButton);
    }

    private void AddCollectionButton(AtlasSO atlas, SerializedProperty collectionProperty)
    {
        Button collectionButton = CreateButton(GetCorrectNameByObject(collectionProperty.displayName), _ussContentButton);
        collectionButton.clicked += () =>
        {
            AddBackToAtlasButton(atlas);

            _currentCollectionProperty = collectionProperty;
            DisplayCollection(collectionProperty);
        };
        _leftPane.Add(collectionButton);
    }

    private void AddBackToAtlasesButton()
    {
        Button backToAtlasesButton = CreateButton("Back to atlases", _ussToolbarButton);
        backToAtlasesButton.clicked += () =>
        {
            DisplayAllAtlases();
            ResetSearch();
            _backToolbar.Clear();
        };
        _backToolbar.Add(backToAtlasesButton);
    }

    private void AddBackToAtlasButton(AtlasSO atlas)
    {
        Button backToAtlasButton = CreateButton($"Back to {GetCorrectNameByObject(atlas).ToLower()}", _ussToolbarButton);
        backToAtlasButton.clicked += () =>
        {
            DisplayAtlas(atlas);
            ResetSearch();
            _backToolbar.Remove(backToAtlasButton);
        };
        _backToolbar.Add(backToAtlasButton);
    }

    private void AddSearchObjectsField()
    {
        _searchField.RegisterValueChangedCallback(evt =>
        {
            string searchString = evt.newValue.ToLower();
            switch (_currentSearchMode)
            {
                case SearchMode.Global:
                    {
                        SearchGloabal(searchString);
                    }
                    break;
                case SearchMode.Collections:
                    {
                        SearchCollections(searchString);
                    }
                    break;
                case SearchMode.Collection:
                    {
                        SearchCollection(searchString);
                    }
                    break;
                default:
                    break;
            }
        });
        _searchField.RegisterCallback<FocusInEvent>(evt =>
        {
            if (_searchField.value == "")
            {
                switch (_currentSearchMode)
                {
                    case SearchMode.Global:
                        {
                            if (!_isSearching)
                            {
                                _isSearching = true;
                                DisplaySearchResult();
                                AddBackToAtlasesButton();
                            }
                            SearchGloabal("");
                        }
                        break;
                    case SearchMode.Collections:
                        {
                            if (!_isSearching)
                            {
                                _isSearching = true;
                                DisplaySearchResult();
                                AddBackToAtlasButton(_currentAtlas);
                            }
                            SearchCollections("");
                        }
                        break;
                    case SearchMode.Collection:
                        {
                            if (!_isSearching)
                            {
                                _isSearching = true;
                                _listView.itemsSource = _collectionObjectsAfterSearch;
                            }
                            SearchCollection("");
                        }
                        break;
                    default:
                        break;
                }
            }
        });
        _toolbar.Add(_searchToolbar);
        _searchToolbar.Add(_searchField);
    }

    private void SearchGloabal(string searchString)
    {
        _collectionObjectsAfterSearch.Clear();
        foreach (List<Object> collectionObjects in _collectionObjectsByCollectionName.Values)
        {
            foreach (Object collectionObject in collectionObjects)
            {
                if (GetCorrectNameByObject(collectionObject).ToLower().Contains(searchString))
                {
                    _collectionObjectsAfterSearch.Add(collectionObject);
                }
            }
        }
        _listView.RefreshItems();
    }

    private void SearchCollections(string searchString)
    {
        _collectionObjectsAfterSearch.Clear();
        foreach (SerializedProperty collectionProperty in _collectionsPropertiesByAtlas[_currentAtlas])
        {
            foreach (Object collectionObject in _collectionObjectsByCollectionName[collectionProperty.name])
            {
                if (GetCorrectNameByObject(collectionObject).ToLower().Contains(searchString))
                {
                    _collectionObjectsAfterSearch.Add(collectionObject);
                }
            }
        }
        _listView.RefreshItems();
    }

    private void SearchCollection(string searchString)
    {
        _collectionObjectsAfterSearch.Clear();
        foreach (Object collectionObject in _currentCollectionObjects)
        {
            if (GetCorrectNameByObject(collectionObject).ToLower().Contains(searchString))
            {
                _collectionObjectsAfterSearch.Add(collectionObject);
            }
        }
        _listView.RefreshItems();
    }

    private void AddDeleteObjectButton()
    {
        _deleteObjectButton = CreateButton("Delete object", _ussToolbarButton);
        _deleteObjectButton.SetEnabled(false);
        _toolbar.Add(_deleteObjectToolbar);
    }

    private void AddListViewForCollection(List<Object> itemSource)
    {
        _listView.itemsSource = itemSource;
        _listView.fixedItemHeight = 30;
        _listView.makeItem += OnMakeObject;
        _listView.bindItem += OnBindObject;
        _listView.selectionChanged += OnObjectChanged;
        _listView.selectedIndex = _selectedIndex;
        _listView.selectionChanged += (items) => { _selectedIndex = _listView.selectedIndex; };
        _listView.ClearSelection();
        _listView.AddToSelection(0);
        _leftPane.Add(_listView);
    }

    private void AddCreateObjectButton()
    {
        _pathToSaveNewObject = _currentAtlas.AtlasDataPath;
        _newObjectType = _collectionElementTypeByCollectionName[_currentCollectionProperty.name];
        Button createObjectButton = CreateButton("Create new object", _ussContentButton);
        createObjectButton.clicked += OnStartCreateNewObject;
        _leftPane.Add(createObjectButton);
        DisableCreateObjectButton(createObjectButton);
    }

    private void DisableCreateObjectButton(Button createObjectButton)
    {
        if (string.IsNullOrEmpty(_pathToSaveNewObject))
        {
            createObjectButton.SetEnabled(false);
        }

        if (!_newObjectType.IsSubclassOf(typeof(ScriptableObject)))
        {
            createObjectButton.SetEnabled(false);
        }
    }

    private string GetCorrectNameByObject(Object obj)
    {
        return GetCorrectNameByObject(obj.name);
    }

    private string GetCorrectNameByObject(string name)
    {
        name = Regex.Replace(name, @"(\p{Ll}) (\P{Ll})", m => m.Groups[1].Value + ' ' + m.Groups[2].Value.ToLower());
        return Regex.Replace(name, @"(\p{Ll})(\P{Ll})", m => m.Groups[1].Value + ' ' + m.Groups[2].Value.ToLower());
    }

    private Sprite GetIconByObject(Object obj)
    {
        return obj switch
        {
            BlockSO block => block.Sprites.Count != 0 ? block.Sprites[0] : null,
            Tree tree => tree.GetComponent<SpriteRenderer>()?.sprite,
            PickUpItem pickUpItem => pickUpItem.GetComponent<SpriteRenderer>()?.sprite,
            _ => null
        };
    }

    private Type GetCollectionElementType(SerializedProperty collectionProperty)
    {
        Type parentType = collectionProperty.serializedObject.targetObject.GetType();
        BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
        FieldInfo field = parentType.GetField(collectionProperty.propertyPath, fieldFlags);
        Type type = field.FieldType.GetElementType();
        return type;
    }

    private Button CreateButton(string name, string className)
    {
        Button button = new Button();
        button.AddToClassList(className);
        button.text = name;
        return button;
    }

    private VisualElement OnMakeObject()
    {
        return new ListItem();
    }

    private void OnBindObject(VisualElement item, int index)
    {
        ListItem cell = item as ListItem;
        Object obj = _listView.itemsSource[index] as Object;
        if (obj == null)
        {
            cell.RemoveFromHierarchy();
            return;
        }
        cell.SetName(GetCorrectNameByObject(obj));
        cell.SetIcon(GetIconByObject(obj));
    }

    private void OnObjectChanged(IEnumerable<object> selectedItems)
    {
        Object obj = selectedItems.FirstOrDefault() as Object;
        if (obj == null)
        {
            return;
        }
        _rightPane.Clear();
        InspectorElement inspectorElement = new InspectorElement(obj);
        inspectorElement.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
        {
            _listView.RefreshItem(_selectedIndex);
        });
        _rightPane.Add(inspectorElement);
    }

    private void OnStartCreateNewObject()
    {
        _rightPane.Clear();
        ResetSearch();

        if (_newScriptableObject == null)
        {
            _newScriptableObject = CreateInstance(_newObjectType);
        }

        TextField objectNameTextField = new TextField();
        objectNameTextField.RegisterValueChangedCallback(evt =>
        {
            _newObjectName = evt.newValue;
        });
        _rightPane.Add(objectNameTextField);

        Button createObject = new Button();
        createObject.text = "Create";
        createObject.clicked += OnCreateNewObject;
        _rightPane.Add(createObject);

        InspectorElement inspectorElement = new InspectorElement(_newScriptableObject);
        _rightPane.Add(inspectorElement);
    }

    private void OnCreateNewObject()
    {
        if (!string.IsNullOrEmpty(_newObjectName))
        {
            SaveItem();
            AddItemToCurrentCollection(_newScriptableObject);
            _newScriptableObject = null;
        }
    }

    private void SaveItem()
    {
        AssetDatabase.CreateAsset(_newScriptableObject, _pathToSaveNewObject + _newObjectName + ".asset");
        AssetDatabase.SaveAssets();
    }

    private void AddItemToCurrentCollection(Object item)
    {
        int arraySize = _currentCollectionProperty.arraySize;
        _currentCollectionProperty.InsertArrayElementAtIndex(arraySize);
        _currentCollectionProperty.GetArrayElementAtIndex(arraySize).objectReferenceValue = item;
        _currentCollectionProperty.serializedObject.ApplyModifiedProperties();
        _currentCollectionObjects.Add(item);
        _listView.RefreshItems();
    }
    #endregion
}