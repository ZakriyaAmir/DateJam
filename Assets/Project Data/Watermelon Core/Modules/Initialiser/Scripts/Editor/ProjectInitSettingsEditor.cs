using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Watermelon
{
    [CustomEditor(typeof(ProjectInitSettings))]
    public class ProjectInitSettingsEditor : Editor
    {
        private readonly string CORE_MODULES_PROPERTY_NAME = "coreModules";
        private readonly string MODULES_PROPERTY_NAME = "modules";

        private readonly string MODULE_NAME_PROPERTY_NAME = "moduleName";

        private static string DEFAULT_PROJECT_INIT_SETTINGS_PATH = "Assets/Project Data/Content/Settings/Project Init Settings.asset";

        private SerializedProperty coreModulesProperty;
        private SerializedProperty modulesProperty;
        private List<InitModuleContainer> initModulesEditors;

        private GUIContent addButton;
        private GUIContent arrowDownContent;
        private GUIContent arrowUpContent;

        private GUIStyle arrowButtonStyle;

        private ProjectInitSettings projectInitSettings;
        private IEnumerable<Type> registeredTypes;
        private GenericMenu modulesGenericMenu;

        [MenuItem("Tools/Editor/Project Init Settings")]
        public static void SelectProjectInitSettings()
        {
            UnityEngine.Object selectedObject = AssetDatabase.LoadAssetAtPath(DEFAULT_PROJECT_INIT_SETTINGS_PATH, typeof(ProjectInitSettings));

            if(selectedObject == null)
            {
                selectedObject = EditorUtils.GetAsset<ProjectInitSettings>();

                if(selectedObject == null)
                {
                    Debug.LogError("Asset with type \"ProjectInitSettings\" don`t exist.");
                }
                else
                {
                    Selection.activeObject = selectedObject;
                    Debug.LogWarning($"Asset with type \"ProjectInitSettings\" is misplaced. Expected path: {DEFAULT_PROJECT_INIT_SETTINGS_PATH} .Actual path: {AssetDatabase.GetAssetPath(selectedObject)}");

                }
            }
            else
            {
                Selection.activeObject = selectedObject;
            }
        }

        protected void OnEnable()
        {
            projectInitSettings = (ProjectInitSettings)target;

            coreModulesProperty = serializedObject.FindProperty(CORE_MODULES_PROPERTY_NAME);
            modulesProperty = serializedObject.FindProperty(MODULES_PROPERTY_NAME);

            // Get all registered init modules
            registeredTypes = Assembly.GetAssembly(typeof(InitModule)).GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(InitModule)));

            InitGenericMenu();
            InitCoreModules();
            LoadEditorsList();

            addButton = new GUIContent("", WatermelonEditor.Styles.GetIcon("icon_add"));

            arrowDownContent = new GUIContent(WatermelonEditor.Styles.GetIcon("icon_arrow_down"));
            arrowUpContent = new GUIContent(WatermelonEditor.Styles.GetIcon("icon_arrow_up"));

            arrowButtonStyle = new GUIStyle(WatermelonEditor.Styles.padding00);

            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private void InitCoreModules()
        {
            //Get current core modules
            InitModule[] coreModules = projectInitSettings.CoreModules;

            bool newModuleCreated = false;
            foreach (Type type in registeredTypes)
            {
                RegisterModuleAttribute[] defineAttributes = (RegisterModuleAttribute[])Attribute.GetCustomAttributes(type, typeof(RegisterModuleAttribute));
                for (int m = 0; m < defineAttributes.Length; m++)
                {
                    if (defineAttributes[m].Core)
                    {
                        bool isExists = coreModules != null && coreModules.Any(x => x.GetType() == type);
                        if (!isExists)
                        {
                            if (EditorUtility.DisplayDialog("New core module is found!", string.Format("Module {0} is required for propper work of the game. Do you want to add it to modules list?", type.FullName), "Add", "Skip"))
                            {
                                AddCoreModule(type, false);

                                newModuleCreated = true;
                            }
                        }
                    }
                }
            }

            // If new module was created, update file system
            if(newModuleCreated)
            {
                LoadEditorsList();

                EditorUtility.SetDirty(target);

                AssetDatabase.SaveAssets();
            }
        }

        private void InitGenericMenu()
        {
            modulesGenericMenu = new GenericMenu();

            //Load all modules
            InitModule[] initModules = projectInitSettings.Modules;

            foreach(Type type in registeredTypes)
            {
                RegisterModuleAttribute[] defineAttributes = (RegisterModuleAttribute[])Attribute.GetCustomAttributes(type, typeof(RegisterModuleAttribute));
                for (int m = 0; m < defineAttributes.Length; m++)
                {
                    if (!defineAttributes[m].Core)
                    {
                        bool isAlreadyActive = initModules != null && initModules.Any(x => x.GetType() == type);
                        if (isAlreadyActive)
                        {
                            modulesGenericMenu.AddDisabledItem(new GUIContent(defineAttributes[m].Path), false);
                        }
                        else
                        {
                            modulesGenericMenu.AddItem(new GUIContent(defineAttributes[m].Path), false, delegate
                            {
                                AddModule(type);

                                InitGenericMenu();
                            });
                        }
                    }
                }
            }
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= LogPlayModeState;
        }

        private void LogPlayModeState(PlayModeStateChange obj)
        {
            if (Selection.activeObject == target)
                Selection.activeObject = null;
        }

        private void LoadEditorsList()
        {
            ClearInitModules();
            SerializedProperty initModule;
            SerializedObject initModuleSerializedObject;

            for (int i = 0; i < coreModulesProperty.arraySize; i++)
            {
                initModule = coreModulesProperty.GetArrayElementAtIndex(i);

                if (initModule.objectReferenceValue != null)
                {
                    initModuleSerializedObject = new SerializedObject(initModule.objectReferenceValue);

                    initModulesEditors.Add(new InitModuleContainer(initModule.objectReferenceValue.GetType(), initModuleSerializedObject, Editor.CreateEditor(initModuleSerializedObject.targetObject)));
                }
            }

            for (int i = 0; i < modulesProperty.arraySize; i++)
            {
                initModule = modulesProperty.GetArrayElementAtIndex(i);

                if(initModule.objectReferenceValue != null)
                {
                    initModuleSerializedObject = new SerializedObject(initModule.objectReferenceValue);

                    initModulesEditors.Add(new InitModuleContainer(initModule.objectReferenceValue.GetType(), initModuleSerializedObject, Editor.CreateEditor(initModuleSerializedObject.targetObject)));
                }
            }
        }

        private InitModuleContainer GetEditor(Type type)
        {
            for(int i = 0; i < initModulesEditors.Count; i++)
            {
                if (initModulesEditors[i].type == type)
                    return initModulesEditors[i];
            }

            return null;
        }

        private void OnDestroy()
        {
            ClearInitModules();
        }

        private void ClearInitModules()
        {
            if (initModulesEditors != null)
            {
                // Destroy old editors
                for (int i = 0; i < initModulesEditors.Count; i++)
                {
                    if (initModulesEditors[i] != null && initModulesEditors[i].editor != null)
                    {
                        DestroyImmediate(initModulesEditors[i].editor);
                    }
                }

                initModulesEditors.Clear();
            }
            else
            {
                initModulesEditors = new List<InitModuleContainer>();
            }
        }

        private void DrawInitModule(SerializedProperty arrayProperty, string title, bool buttons)
        {
            EditorGUILayoutCustom.Header(title);

            if (arrayProperty.arraySize > 0)
            {
                for (int i = 0; i < arrayProperty.arraySize; i++)
                {
                    SerializedProperty initModule = arrayProperty.GetArrayElementAtIndex(i);

                    if (initModule.objectReferenceValue != null)
                    {
                        SerializedObject moduleSeializedObject = new SerializedObject(initModule.objectReferenceValue);

                        moduleSeializedObject.Update();

                        SerializedProperty moduleNameProperty = moduleSeializedObject.FindProperty(MODULE_NAME_PROPERTY_NAME);

                        EditorGUILayout.BeginVertical(WatermelonEditor.Styles.box);

                        Rect moduleRect = EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(moduleNameProperty.stringValue, WatermelonEditor.Styles.label_medium);
                        EditorGUILayout.EndHorizontal();

                        if (initModule.isExpanded)
                        {
                            InitModuleContainer moduleContainer = GetEditor(initModule.objectReferenceValue.GetType());
                            if (moduleContainer != null && moduleContainer.editor != null)
                            {
                                moduleContainer.editor.OnInspectorGUI();
                            }

                            GUILayout.Space(10);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();

                            if (moduleContainer.isModuleInitEditor)
                            {
                                moduleContainer.initModuleEditor.Buttons();
                            }

                            if(buttons)
                            {
                                if (GUILayout.Button("Remove", GUILayout.Width(90)))
                                {
                                    if (EditorUtility.DisplayDialog("This object will be removed!", "Are you sure?", "Remove", "Cancel"))
                                    {
                                        UnityEngine.Object removedObject = initModule.objectReferenceValue;
                                        initModule.isExpanded = false;
                                        arrayProperty.RemoveFromVariableArrayAt(i);

                                        LoadEditorsList();
                                        AssetDatabase.RemoveObjectFromAsset(removedObject);

                                        DestroyImmediate(removedObject, true);

                                        EditorUtility.SetDirty(target);

                                        AssetDatabase.SaveAssets();

                                        return;
                                    }
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();

                        if (GUI.Button(new Rect(moduleRect.x + moduleRect.width - 15, moduleRect.y, 12, 12), arrowUpContent, arrowButtonStyle))
                        {
                            if (i > 0)
                            {
                                bool expandState = arrayProperty.GetArrayElementAtIndex(i - 1).isExpanded;

                                arrayProperty.MoveArrayElement(i, i - 1);

                                arrayProperty.GetArrayElementAtIndex(i - 1).isExpanded = initModule.isExpanded;
                                arrayProperty.GetArrayElementAtIndex(i).isExpanded = expandState;
                                serializedObject.ApplyModifiedProperties();

                                GUIUtility.ExitGUI();
                                Event.current.Use();

                                return;
                            }
                        }
                        if (GUI.Button(new Rect(moduleRect.x + moduleRect.width - 15, moduleRect.y + 12, 12, 12), arrowDownContent, arrowButtonStyle))
                        {
                            if (i + 1 < arrayProperty.arraySize)
                            {
                                bool expandState = arrayProperty.GetArrayElementAtIndex(i + 1).isExpanded;

                                arrayProperty.MoveArrayElement(i, i + 1);

                                arrayProperty.GetArrayElementAtIndex(i + 1).isExpanded = initModule.isExpanded;
                                arrayProperty.GetArrayElementAtIndex(i).isExpanded = expandState;

                                serializedObject.ApplyModifiedProperties();

                                GUIUtility.ExitGUI();
                                Event.current.Use();

                                return;
                            }
                        }

                        if (GUI.Button(moduleRect, GUIContent.none, GUIStyle.none))
                        {
                            initModule.isExpanded = !initModule.isExpanded;
                        }

                        moduleSeializedObject.ApplyModifiedProperties();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal(WatermelonEditor.Styles.box);
                        EditorGUILayout.BeginHorizontal(WatermelonEditor.Styles.padding00);
                        EditorGUILayout.LabelField(EditorGUIUtility.IconContent("console.warnicon"), WatermelonEditor.Styles.padding00, GUILayout.Width(16), GUILayout.Height(16));
                        EditorGUILayout.LabelField("Object referenct is null");
                        if (GUILayout.Button("Remove", EditorStyles.miniButton))
                        {
                            arrayProperty.RemoveFromVariableArrayAt(i);

                            InitGenericMenu();

                            GUIUtility.ExitGUI();
                            Event.current.Use();

                            return;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Modules list is empty!", MessageType.Info);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(WatermelonEditor.Styles.Skin.box);

            DrawInitModule(coreModulesProperty, "CORE MODULES", false);

            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            Rect projectModulesRect = EditorGUILayout.BeginVertical(WatermelonEditor.Styles.Skin.box);

            DrawInitModule(modulesProperty, "MODULES", true);

            // Buttons panel
            Rect buttonsPanelRect = new Rect(projectModulesRect.x + projectModulesRect.width - 40, projectModulesRect.y + projectModulesRect.height, 30, 20);
            Rect addButtonRect = new Rect(buttonsPanelRect.x + 5, buttonsPanelRect.y, 20, 20);

            GUI.Box(buttonsPanelRect, "", WatermelonEditor.Styles.panelBottom);
            GUI.Label(addButtonRect, addButton, WatermelonEditor.Styles.labelCentered);

            if (GUI.Button(buttonsPanelRect, GUIContent.none, GUIStyle.none))
            {
                modulesGenericMenu.ShowAsContext();
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(90);
        }

        public void AddModule(Type moduleType)
        {
            if(!moduleType.IsSubclassOf(typeof(InitModule)))
            {
                Debug.LogError("[Initialiser]: Module type should be subclass of InitModule class!");

                return;
            }

            Undo.RecordObject(target, "Add module");

            modulesProperty = serializedObject.FindProperty(MODULES_PROPERTY_NAME);

            serializedObject.Update();

            modulesProperty.arraySize++;

            InitModule testInitModule = (InitModule)ScriptableObject.CreateInstance(moduleType);
            testInitModule.name = moduleType.ToString();

            AssetDatabase.AddObjectToAsset(testInitModule, target);

            modulesProperty.GetArrayElementAtIndex(modulesProperty.arraySize - 1).objectReferenceValue = testInitModule;

            serializedObject.ApplyModifiedProperties();
            LoadEditorsList();

            EditorUtility.SetDirty(target);

            AssetDatabase.SaveAssets();
        }

        public void AddCoreModule(Type moduleType, bool updateFileSystem = false)
        {
            if (!moduleType.IsSubclassOf(typeof(InitModule)))
            {
                Debug.LogError("[Initialiser]: Module type should be subclass of InitModule class!");

                return;
            }

            Undo.RecordObject(target, "Add core module");

            coreModulesProperty = serializedObject.FindProperty(CORE_MODULES_PROPERTY_NAME);

            serializedObject.Update();

            coreModulesProperty.arraySize++;

            InitModule testInitModule = (InitModule)ScriptableObject.CreateInstance(moduleType);
            testInitModule.name = moduleType.ToString();

            AssetDatabase.AddObjectToAsset(testInitModule, target);

            coreModulesProperty.GetArrayElementAtIndex(coreModulesProperty.arraySize - 1).objectReferenceValue = testInitModule;

            serializedObject.ApplyModifiedProperties();

            if(updateFileSystem)
            {
                LoadEditorsList();

                EditorUtility.SetDirty(target);

                AssetDatabase.SaveAssets();
            }
        }

        private class InitModuleContainer
        {
            public Type type;
            public SerializedObject serializedObject;
            public Editor editor;

            public bool isModuleInitEditor;
            public InitModuleEditor initModuleEditor;

            public InitModuleContainer(Type type, SerializedObject serializedObject, Editor editor)
            {
                this.type = type;
                this.serializedObject = serializedObject;
                this.editor = editor;

                initModuleEditor = editor as InitModuleEditor;
                isModuleInitEditor = initModuleEditor != null;
            }
        }
    }
}