using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;

namespace Watermelon
{
    [CustomEditor(typeof(CoreOrganizer))]
    public class CoreOrganizerEditor : Editor
    {
        private const string CORE_SETTINGS_PROPERTY_PATH = "coreSettings";
        private const string NAME_PROPERTY_PATH = "name";
        private const string DIRECTORY_PROPERTY_PATH = "directory";
        private const string DIRECTORIES_PROPERTY_PATH = "directories";
        private const string EXIST_PROPERTY_PATH = "isDirectoryExist";
        private const string STATUS_PROPERTY_PATH = "status";
        private const string INCLUDE_PROPERTY_PATH = "include";

        private const string LIST_HEADER = "Core Settings"; 
        private const string UPDATE_STATUS_LABEL = "Update status"; 
        private const string ORGANIZE_LABEL = "Organize";
        private const string SEPARATOR = " | ";
        private const string EXISTS_STATUS = "Exists";
        private const string TO_DELETE_STATUS = "To Delete";
        private const string REMOVED_STATUS = "Removed";
        private const string MISSING_STATUS = "Missing";

        private const int DEFAULT_PADDING = 8;
        private const int SMALL_PADDING = 2;
        private const string PATH_SEPARATOR = "/";
        private const string META_FILE_SUFFIX = ".meta";
        private const int ICON_BUTTON_WIDTH = 24;
        private readonly float EXPANDED_ELEMENT_HEIGHT = (EditorGUIUtility.singleLineHeight + SMALL_PADDING) * 4;
        SerializedProperty coreSettingsProperty;
        SerializedProperty nameProperty;
        SerializedProperty directoryProperty;
        SerializedProperty directoriesProperty;
        SerializedProperty existProperty;
        SerializedProperty statusProperty;
        SerializedProperty includeProperty;
        SerializedProperty currentElementProperty;

        SerializedProperty currentDirectory;
        ReorderableList list;
        Rect workRect;
        Rect fieldRect;
        GUIStyle greenStyle;
        GUIStyle yellowStyle;
        GUIStyle redStyle;
        GUIStyle labelStyle;
        Color backupColor;

        //Colors to change
        Color greenBackroundColor = new Color(0.466f, 0.729f, 0.6f, 1f);
        Color yellowBackroundColor = new Color(0.906f, 0.663f, 0.467f, 1f);
        Color redBackroundColor = new Color(0.666f, 0.266f, 0.396f, 1f);
        Color defaultTextColor = Color.black;
        Color activeTextColor = Color.white;
        Color foldoutColor = Color.black;

        GUIContent iconAdd;
        GUIContent iconRemove;
        GUIContent iconUp;
        GUIContent iconDown;
        private Rect moveUpRect;
        private Rect moveDownRect;
        private Rect removeRect;

        protected void OnEnable()
        {
            coreSettingsProperty = serializedObject.FindProperty(CORE_SETTINGS_PROPERTY_PATH);

            list = new ReorderableList(serializedObject, coreSettingsProperty, true, true, true, true);
            
            list.drawHeaderCallback += DrawHeaderCallback;
            list.drawElementCallback += DrawElementCallback;
            list.elementHeightCallback += ElementHeightCallback;
            list.drawElementBackgroundCallback += DrawElementBackgroundCallback;
            list.onAddCallback += OnAddCallback;

            HandleObsoleteField();
            PrepareStyles();
        }

        private void HandleObsoleteField()
        {
            for (int i = 0; i < coreSettingsProperty.arraySize; i++)
            {
                OpenElement(i);

                if(directoryProperty.stringValue.Length > 0)
                {
                    directoriesProperty.arraySize++;
                    directoriesProperty.GetArrayElementAtIndex(0).stringValue = directoryProperty.stringValue;
                    directoryProperty.stringValue = string.Empty;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected void PrepareStyles()
        {
            greenStyle = new GUIStyle();
            greenStyle.onNormal.background = MakeColoredTexture(2, 2, greenBackroundColor);

            yellowStyle = new GUIStyle();
            yellowStyle.onNormal.background = MakeColoredTexture(2, 2, yellowBackroundColor);

            redStyle = new GUIStyle();
            redStyle.onNormal.background = MakeColoredTexture(2, 2, redBackroundColor);

            labelStyle = new GUIStyle(WatermelonEditor.Styles.Skin.label);
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.normal.textColor = defaultTextColor;
            labelStyle.focused.textColor = activeTextColor;
            labelStyle.active.textColor = activeTextColor;
            labelStyle.hover.textColor = activeTextColor;

            iconAdd = new GUIContent(WatermelonEditor.Styles.GetIcon("icon_add"));
            iconRemove = new GUIContent(WatermelonEditor.Styles.GetIcon("icon_close"));
            iconUp = new GUIContent(WatermelonEditor.Styles.GetIcon("icon_arrow_up"));
            iconDown = new GUIContent(WatermelonEditor.Styles.GetIcon("icon_arrow_down"));
        }

        private Texture2D MakeColoredTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = color;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, LIST_HEADER);
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            OpenElement(index);

            workRect = new Rect(rect);
            workRect.height = EditorGUIUtility.singleLineHeight + SMALL_PADDING;
            workRect.xMin += DEFAULT_PADDING;

            backupColor = GUI.backgroundColor;
            GUI.backgroundColor = foldoutColor;
            currentElementProperty.isExpanded = EditorGUI.Foldout(workRect, currentElementProperty.isExpanded, GUIContent.none);
            GUI.backgroundColor = backupColor;

            workRect.xMin += DEFAULT_PADDING;

            EditorGUI.BeginChangeCheck();
            includeProperty.boolValue = EditorGUI.ToggleLeft(workRect, GetElementHeaderLabel(), includeProperty.boolValue, labelStyle);

            if (EditorGUI.EndChangeCheck())
            {
                UpdateStatus();
            }


            if (currentElementProperty.isExpanded)
            {
                workRect = new Rect(rect);
                workRect.height = EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                workRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING + SMALL_PADDING; //header
                workRect.xMin += DEFAULT_PADDING * 3;
                fieldRect = new Rect(workRect);
                fieldRect.xMin += EditorGUIUtility.labelWidth;

                EditorGUI.PrefixLabel(workRect, new GUIContent(nameProperty.displayName), labelStyle);
                nameProperty.stringValue = EditorGUI.TextField(fieldRect, GUIContent.none, nameProperty.stringValue);

                

                workRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                fieldRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING + SMALL_PADDING;
                EditorGUI.PrefixLabel(workRect, new GUIContent(directoriesProperty.displayName), labelStyle);

                fieldRect.height = EditorGUIUtility.singleLineHeight - SMALL_PADDING;

                if (GUI.Button(fieldRect, "add directory"))
                {
                    directoriesProperty.arraySize++;
                    directoriesProperty.GetArrayElementAtIndex(directoriesProperty.arraySize - 1).stringValue = string.Empty;
                }

                workRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                fieldRect = new Rect(workRect);
                fieldRect.height = EditorGUIUtility.singleLineHeight;
                fieldRect.xMax -= 84;
                moveUpRect = new Rect(fieldRect.xMax + DEFAULT_PADDING, fieldRect.y + SMALL_PADDING, ICON_BUTTON_WIDTH, EditorGUIUtility.singleLineHeight - SMALL_PADDING);
                moveDownRect = new Rect(moveUpRect);
                moveDownRect.x += ICON_BUTTON_WIDTH + SMALL_PADDING;
                removeRect = new Rect(moveUpRect);
                removeRect.x += ICON_BUTTON_WIDTH + SMALL_PADDING + ICON_BUTTON_WIDTH + SMALL_PADDING;

                for (int i = 0; i < directoriesProperty.arraySize; i++)
                {
                    currentDirectory = directoriesProperty.GetArrayElementAtIndex(i);

                    if (statusProperty.stringValue.Equals(MISSING_STATUS) && (directoriesProperty.arraySize == existProperty.arraySize) && (!existProperty.GetArrayElementAtIndex(i).boolValue))
                    {
                        backupColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.red;
                        currentDirectory.stringValue = EditorGUI.TextField(fieldRect, GUIContent.none, currentDirectory.stringValue);
                        GUI.backgroundColor = backupColor;
                    }
                    else
                    {
                        currentDirectory.stringValue = EditorGUI.TextField(fieldRect, GUIContent.none, currentDirectory.stringValue);
                    }

                    EditorGUI.BeginDisabledGroup(i == 0);

                    if (GUI.Button(moveUpRect, iconUp))
                    {
                        directoriesProperty.MoveArrayElement(i, i - 1);
                        EditorGUI.FocusTextInControl(null);
                    }

                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(i == directoriesProperty.arraySize - 1);

                    if (GUI.Button(moveDownRect, iconDown))
                    {
                        directoriesProperty.MoveArrayElement(i, i + 1);
                        EditorGUI.FocusTextInControl(null);
                    }

                    EditorGUI.EndDisabledGroup();

                    if (GUI.Button(removeRect, iconRemove))
                    {
                        directoriesProperty.DeleteArrayElementAtIndex(i);
                        EditorGUI.FocusTextInControl(null);
                    }


                    workRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                    fieldRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                    moveUpRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                    moveDownRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                    removeRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                }


                for (int i = 0; i < directoriesProperty.arraySize; i++)
                {
                    currentDirectory = directoriesProperty.GetArrayElementAtIndex(i);

                    if (currentDirectory.stringValue.StartsWith("Assets/"))
                    {
                        currentDirectory.stringValue = currentDirectory.stringValue.Substring(7);
                    }
                }
            }
        }

        private string GetElementHeaderLabel()
        {
            return SEPARATOR + statusProperty.stringValue + SEPARATOR + nameProperty.stringValue;
        }

        private float ElementHeightCallback(int index)
        {
            if (coreSettingsProperty.GetArrayElementAtIndex(index).isExpanded)
            {
                return (EditorGUIUtility.singleLineHeight + SMALL_PADDING) * (3 + coreSettingsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(DIRECTORIES_PROPERTY_PATH).arraySize);
            }
            else
            {
                return EditorGUIUtility.singleLineHeight + SMALL_PADDING;
            }
        }

        private void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if(Event.current.type == EventType.Repaint)
            {
                if(index == -1)
                {
                    return;
                }

                OpenElement(index);

                if (statusProperty.stringValue.Equals(TO_DELETE_STATUS))
                {
                    yellowStyle.Draw(rect, false, false, true, false);
                }
                else if (statusProperty.stringValue.Equals(MISSING_STATUS))
                {
                    redStyle.Draw(rect, false, false, true, false);
                }
                else
                {
                    greenStyle.Draw(rect, false, false, true, false);
                }
            }
        }

        private void OnAddCallback(ReorderableList list)
        {
            coreSettingsProperty.arraySize++;
            OpenElement(coreSettingsProperty.arraySize - 1);
            coreSettingsProperty.GetArrayElementAtIndex(coreSettingsProperty.arraySize - 1).ClearProperty();
            coreSettingsProperty.GetArrayElementAtIndex(coreSettingsProperty.arraySize - 1).isExpanded = true;
            directoriesProperty.arraySize = 1;
            includeProperty.boolValue = true;
        }

        
        private void OpenElement(int index)
        {
            currentElementProperty = coreSettingsProperty.GetArrayElementAtIndex(index);
            nameProperty = currentElementProperty.FindPropertyRelative(NAME_PROPERTY_PATH);
            directoryProperty = currentElementProperty.FindPropertyRelative(DIRECTORY_PROPERTY_PATH);
            directoriesProperty = currentElementProperty.FindPropertyRelative(DIRECTORIES_PROPERTY_PATH);
            existProperty = currentElementProperty.FindPropertyRelative(EXIST_PROPERTY_PATH);
            statusProperty = currentElementProperty.FindPropertyRelative(STATUS_PROPERTY_PATH);
            includeProperty = currentElementProperty.FindPropertyRelative(INCLUDE_PROPERTY_PATH);
        }


        public override void OnInspectorGUI()
        {
            list.DoLayoutList();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(UPDATE_STATUS_LABEL, WatermelonEditor.Styles.button_02_large))
            {
                for (int i = 0; i < coreSettingsProperty.arraySize; i++)
                {
                    OpenElement(i);
                    UpdateStatus();
                }
            }

            if (GUILayout.Button(ORGANIZE_LABEL, WatermelonEditor.Styles.button_03_large))
            {
                for (int i = 0; i < coreSettingsProperty.arraySize; i++)
                {
                    OpenElement(i);

                    if (statusProperty.stringValue.Equals(TO_DELETE_STATUS))
                    {
                        RemoveFolders();
                        UpdateStatus();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateStatus()
        {
            existProperty.arraySize = directoriesProperty.arraySize;
            string globalDirectory;
            int counter = 0;
            bool exists;

            for (int i = 0; i < directoriesProperty.arraySize; i++)
            {
                globalDirectory = Application.dataPath + PATH_SEPARATOR + directoriesProperty.GetArrayElementAtIndex(i).stringValue;
                exists = (Directory.Exists(globalDirectory) || File.Exists(globalDirectory));
                existProperty.GetArrayElementAtIndex(i).boolValue = exists;

                if (exists)
                {
                    counter++;
                }
            }

            if (includeProperty.boolValue)
            {
                if(counter == directoriesProperty.arraySize)
                {
                    statusProperty.stringValue = EXISTS_STATUS;
                }
                else
                {
                    statusProperty.stringValue = MISSING_STATUS;
                }
            }
            else
            {
                if(counter > 0)
                {
                    statusProperty.stringValue = TO_DELETE_STATUS;
                }
                else
                {
                    statusProperty.stringValue = REMOVED_STATUS;
                }
            }
        }

        private void RemoveFolders()
        {
            string globalDirectory;

            for (int i = 0; i < directoriesProperty.arraySize; i++)
            {
                globalDirectory = Application.dataPath + PATH_SEPARATOR + directoriesProperty.GetArrayElementAtIndex(i).stringValue;

                if (Directory.Exists(globalDirectory) || File.Exists(globalDirectory))
                {
                    FileUtil.DeleteFileOrDirectory(globalDirectory);
                    FileUtil.DeleteFileOrDirectory(globalDirectory + META_FILE_SUFFIX);
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
