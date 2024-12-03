#pragma warning disable 649

using UnityEngine;
using UnityEditor;
using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;

namespace Watermelon.BusStop
{
    public class LevelEditorWindow : LevelEditorBase
    {

        //used variables
        private const string LEVELS_PROPERTY_NAME = "levels";
        private const string LEVEL_ELEMENTS_PROPERTY_NAME = "levelElements";
        private const string EDITOR_TYPE_COLORS_PROPERTY_NAME = "editorTypeColors";
        private const string EDITOR_SPAWNER_TEXTURE_PROPERTY_NAME = "editorSpawnerTexure";
        private SerializedProperty levelsSerializedProperty;
        private SerializedProperty levelElementsSerializedProperty;
        private SerializedProperty editorTypeColorsSerializedProperty;
        private SerializedProperty editorSpawnerTexureSerializedProperty;
        private LevelRepresentation selectedLevelRepresentation;
        private LevelsHandler levelsHandler;
        private CellTypesHandler gridHandler;

        //sidebar
        private const int SIDEBAR_WIDTH = 320;

        //PlayerPrefs
        private const string PREFS_LEVEL = "editor_level_index";
        private const string PREFS_WIDTH = "editor_sidebar_width";

        //instructions
        private const string LEVEL_INSTRUCTION = "Use left click or hold left button to fill cells with selected cell type.";
        private const string RIGHT_CLICK_INSTRUCTION = "Use right click to set hidden state.";
        private const string LEVEL_PASSED_VALIDATION = "Level passed validation.";
        private const int INFO_HEIGH = 78; //found out using Debug.Log(infoRect) on worst case scenario
        private const string ELEMENT_TYPE_PROPERTY_NAME = "elementType";
        private const string EDITOR_COLOR_PROPERTY_NAME = "editorColor";
        private const string TEST_LEVEL = "Test level";
        private const string GENERATE_LEVEL = "Generate";
        private const string MENU_ITEM_PATH = "Edit/Play";
        private const string SAVE_NAME = "level";
        private const string CLEAR = "Clear";
        private Rect infoRect;

        //level drawing
        private Rect drawRect;
        private float xSize;
        private float ySize;
        private float elementSize;
        private Event currentEvent;
        private Vector2 elementUnderMouseIndex;
        private Vector2Int elementPosition;
        private int invertedY;
        private float buttonRectX;
        private float buttonRectY;
        private Rect buttonRect;

        //Menu
        private int menuIndex1;
        private int menuIndex2;

        private List<LevelElement.Type> cellTypes;
        private List<LevelData.SpecialEffectType> extraPropsTypes;
        TabHandler tabHandler;
        private SerializedProperty colorElementProperty;
        private string tempFieldLabel;
        private Color tempFieldColor;

        //creation mode
        private bool isCreationModeEnabled;
        private int cellCount = 0;
        private int maxCellCount = 3;
        private bool[,] placements;
        private List<int> creationModeBlockTypes;
        private SpawnerRepresentation tempSpawnerData;
        private Matrix4x4 matrixBackup;
        private Color defaultColor;
        private bool isTutuorialSetupModeEnabled;
        private string cellTutorialStepLabel;
        private int currentSideBarWidth;
        private bool lastActiveLevelOpened;
        private Rect separatorRect;
        private bool separatorIsDragged;
        private Rect queueDrawRect;
        private float posX;
        private float posY;
        private int queueElementSize = 32;
        private int itemsPerRow;
        private int rowsCount;

        protected override WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder)
        {
            return builder.SetWindowMinSize(new Vector2(930, 670)).Build();
        }

        protected override Type GetLevelsDatabaseType()
        {
            return typeof(LevelDatabase);
        }

        public override Type GetLevelType()
        {
            return typeof(LevelData);
        }


        protected override void ReadLevelDatabaseFields()
        {
            levelsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVELS_PROPERTY_NAME);
            levelElementsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVEL_ELEMENTS_PROPERTY_NAME);
            editorTypeColorsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(EDITOR_TYPE_COLORS_PROPERTY_NAME);
            editorSpawnerTexureSerializedProperty = levelsDatabaseSerializedObject.FindProperty(EDITOR_SPAWNER_TEXTURE_PROPERTY_NAME);
        }

        protected override void InitialiseVariables()
        {
            LevelRepresentation.levelsSerializedProperty = levelsSerializedProperty;

            gridHandler = new CellTypesHandler();

            cellTypes = new List<LevelElement.Type>();
            cellTypes.AddRange((LevelElement.Type[])Enum.GetValues(typeof(LevelElement.Type)));
            creationModeBlockTypes = new List<int>();

            for (int i = 0; i < cellTypes.Count; i++)
            {
                gridHandler.AddCellType(new CellTypesHandler.CellType(i, cellTypes[i].ToString(), GetColor(i, cellTypes[i]), IsExtraPropAvailable(cellTypes[i])));

                if (cellTypes[i].ToString().Contains("Block_"))
                {
                    creationModeBlockTypes.Add(i);
                }
            }

            extraPropsTypes = new List<LevelData.SpecialEffectType>();
            extraPropsTypes.AddRange((LevelData.SpecialEffectType[])Enum.GetValues(typeof(LevelData.SpecialEffectType)));

            for (int i = 0; i < extraPropsTypes.Count; i++)
            {
                gridHandler.AddExtraProp(new CellTypesHandler.ExtraProp(i, extraPropsTypes[i].ToString(), i != 0));
            }

            levelsHandler = new LevelsHandler(levelsDatabaseSerializedObject, levelsSerializedProperty);

            tabHandler = new TabHandler();
            tabHandler.AddTab(new TabHandler.Tab("Levels", DrawLevelsTab, UpdateCellColors));
            tabHandler.AddTab(new TabHandler.Tab("Level Items", DrawLevelItemsTab, CheckForEmptyLevelItems));

            currentSideBarWidth = PlayerPrefs.GetInt(PREFS_WIDTH, SIDEBAR_WIDTH);
        }



        private void OpenLastActiveLevel()
        {
            if (!lastActiveLevelOpened)
            {
                if ((levelsSerializedProperty.arraySize > 0) && PlayerPrefs.HasKey(PREFS_LEVEL))
                {
                    int levelIndex = Mathf.Clamp(PlayerPrefs.GetInt(PREFS_LEVEL, 0), 0, levelsSerializedProperty.arraySize - 1);
                    levelsHandler.CustomList.SelectedIndex = levelIndex;
                    levelsHandler.OpenLevel(levelIndex);
                }

                lastActiveLevelOpened = true;
            }
        }

        private void UpdateCellColors()
        {
            for (int i = 0; i < gridHandler.cellTypes.Count; i++)
            {
                gridHandler.cellTypes[i].color = GetColor(i, cellTypes[i]);
            }
        }

        private bool IsExtraPropAvailable(LevelElement.Type type)
        {
            return true;
        }

        private Color GetColor(int index, LevelElement.Type typeName)
        {
            //try to get color from LevelElement
            int value = (int)typeName;
            SerializedProperty elementProperty;

            for (int i = 0; i < editorTypeColorsSerializedProperty.arraySize; i++)
            {
                elementProperty = editorTypeColorsSerializedProperty.GetArrayElementAtIndex(i);

                if (elementProperty.FindPropertyRelative(ELEMENT_TYPE_PROPERTY_NAME).intValue == value)
                {
                    return elementProperty.FindPropertyRelative(EDITOR_COLOR_PROPERTY_NAME).colorValue;
                }
            }

            //getting random color from name
            MD5 textHash = MD5.Create();
            byte[] hashBytes = textHash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(index.ToString() + typeName.ToString()));
            Color newColor = new Color32(hashBytes[index % 3], hashBytes[index % 3 + 1], hashBytes[index % 3 + 2], byte.MaxValue); //we spread colors

            editorTypeColorsSerializedProperty.arraySize++;
            elementProperty = editorTypeColorsSerializedProperty.GetArrayElementAtIndex(editorTypeColorsSerializedProperty.arraySize - 1);
            elementProperty.FindPropertyRelative(ELEMENT_TYPE_PROPERTY_NAME).intValue = value;
            elementProperty.FindPropertyRelative(EDITOR_COLOR_PROPERTY_NAME).colorValue = newColor;

            return newColor;
        }

        protected override void Styles()
        {
            if (gridHandler != null)
            {
                gridHandler.SetDefaultLabelStyle();
            }

            if (tabHandler != null)
            {
                tabHandler.SetDefaultToolbarStyle();
            }
        }

        public override void OpenLevel(UnityEngine.Object levelObject, int index)
        {
            PlayerPrefs.SetInt(PREFS_LEVEL, index);
            PlayerPrefs.Save();
            selectedLevelRepresentation = new LevelRepresentation(levelObject);

            if (!selectedLevelRepresentation.NullLevel)
            {
                selectedLevelRepresentation.Validate(gridHandler, creationModeBlockTypes, cellTypes);
            }

        }

        public override string GetLevelLabel(UnityEngine.Object levelObject, int index)
        {
            LevelRepresentation levelRepresentation = new LevelRepresentation(levelObject);

            if (!levelRepresentation.NullLevel)
            {
                levelRepresentation.Validate(gridHandler, creationModeBlockTypes, cellTypes);
                levelRepresentation.UpdateNote();
                levelRepresentation.ApplyChanges();
            }

            return levelRepresentation.GetLevelLabel(index, stringBuilder);
        }

        public override void ClearLevel(UnityEngine.Object levelObject)
        {
            new LevelRepresentation(levelObject).Clear();
        }
        public override void LogErrorsForGlobalValidation(UnityEngine.Object levelObject, int index)
        {
            LevelRepresentation level = new LevelRepresentation(levelObject);
            level.ValidateLevel();

            if (!level.IsLevelCorrect)
            {
                Debug.Log("Logging validation errors for level #" + (index + 1) + " :");

                foreach (string error in level.errorLabels)
                {
                    Debug.LogWarning(error);
                }
            }
            else
            {
                Debug.Log($"Level # {(index + 1)} passed validation.");
            }
        }

        protected override void DrawContent()
        {
            tabHandler.DisplayTab();
        }

        private void DrawLevelsTab()
        {
            OpenLastActiveLevel();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DisplayListArea();
            HandleChangingSideBar();
            DisplayMainArea();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void HandleChangingSideBar()
        {
            separatorRect = EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.MinWidth(8), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.AddCursorRect(separatorRect, MouseCursor.ResizeHorizontal);


            if (separatorRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    separatorIsDragged = true;
                    levelsHandler.IgnoreDragEvents = true;
                    Event.current.Use();
                }
            }

            if (separatorIsDragged)
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    separatorIsDragged = false;
                    levelsHandler.IgnoreDragEvents = false;
                    PlayerPrefs.SetInt(PREFS_WIDTH, currentSideBarWidth);
                    PlayerPrefs.Save();
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    currentSideBarWidth = Mathf.RoundToInt(Event.current.delta.x) + currentSideBarWidth;
                    Event.current.Use();
                }
            }
        }

        private void CheckForEmptyLevelItems()
        {
            int enumValueIndex;

            for (int i = editorTypeColorsSerializedProperty.arraySize - 1; i >= 0; i--)
            {
                colorElementProperty = editorTypeColorsSerializedProperty.GetArrayElementAtIndex(i);
                enumValueIndex = colorElementProperty.FindPropertyRelative(ELEMENT_TYPE_PROPERTY_NAME).enumValueIndex;

                if (enumValueIndex == -1)
                {
                    editorTypeColorsSerializedProperty.DeleteArrayElementAtIndex(i);
                }
            }
        }

        private void DrawLevelItemsTab()
        {
            EditorGUILayout.PropertyField(levelElementsSerializedProperty);
            EditorGUILayout.PropertyField(editorSpawnerTexureSerializedProperty);

            EditorGUILayout.LabelField("Type colors:");
            int enumValueIndex;

            for (int i = 0; i < editorTypeColorsSerializedProperty.arraySize; i++)
            {
                EditorGUILayout.Space();
                colorElementProperty = editorTypeColorsSerializedProperty.GetArrayElementAtIndex(i);
                enumValueIndex = colorElementProperty.FindPropertyRelative(ELEMENT_TYPE_PROPERTY_NAME).enumValueIndex;
                tempFieldLabel = colorElementProperty.FindPropertyRelative(ELEMENT_TYPE_PROPERTY_NAME).enumDisplayNames[enumValueIndex];
                tempFieldColor = colorElementProperty.FindPropertyRelative(EDITOR_COLOR_PROPERTY_NAME).colorValue;
                colorElementProperty.FindPropertyRelative(EDITOR_COLOR_PROPERTY_NAME).colorValue = EditorGUILayout.ColorField(tempFieldLabel, tempFieldColor);
            }
        }

        private void DisplayListArea()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(currentSideBarWidth));
            levelsHandler.DisplayReordableList();
            levelsHandler.DrawRenameLevelsButton();
            levelsHandler.DrawGlobalValidationButton();
            gridHandler.DrawCellButtons();
            EditorGUILayout.EndVertical();
        }

        private void DisplayMainArea()
        {
            if (levelsHandler.SelectedLevelIndex == -1)
            {
                return;
            }

            if (selectedLevelRepresentation.NullLevel)
            {
                EditorGUILayout.BeginVertical();

                if (IsPropertyChanged(levelsHandler.SelectedLevelProperty, new GUIContent("File")))
                {
                    levelsHandler.ReopenLevel();
                }

                EditorGUILayout.EndVertical();

                return;
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical();

            if (IsPropertyChanged(levelsHandler.SelectedLevelProperty, new GUIContent("File")))
            {
                levelsHandler.ReopenLevel();
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(selectedLevelRepresentation.widthProperty);
            EditorGUILayout.PropertyField(selectedLevelRepresentation.heightProperty);

            if (EditorGUI.EndChangeCheck())
            {
                selectedLevelRepresentation.HandleSizePropertyChange();
            }

            EditorGUILayout.PropertyField(selectedLevelRepresentation.coinsRewardProperty);
            EditorGUILayout.PropertyField(selectedLevelRepresentation.useInRandomizerProperty);
            DrawBusSpawnQueue();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(TEST_LEVEL))
            {
                TestLevel();
            }

            if (GUILayout.Button(CLEAR))
            {
                selectedLevelRepresentation.ClearField(creationModeBlockTypes);


                if (isCreationModeEnabled)
                {
                    cellCount = 0;
                    UpdatePlacements();

                    if (creationModeBlockTypes.IndexOf(gridHandler.selectedCellTypeValue) == -1)//selecting random
                    {
                        AutoSelectRandomBlock();
                    }
                }

            }

            EditorGUILayout.EndHorizontal();

            if (isCreationModeEnabled)
            {
                HandleCreationMode();
            }
            else if (isTutuorialSetupModeEnabled)
            {
                HandleTutorialSetupMode();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                HandleCreationMode();
                HandleTutorialSetupMode();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            DrawLevel();
            selectedLevelRepresentation.Validate(gridHandler, creationModeBlockTypes, cellTypes);
            selectedLevelRepresentation.UpdateNote();
            levelsHandler.UpdateCurrentLevelLabel(selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder));
            selectedLevelRepresentation.ApplyChanges();

            DrawTipsAndWarnings();

            EditorGUILayout.EndVertical();
        }

        

        private void HandleTutorialSetupMode()
        {
            if (!isTutuorialSetupModeEnabled)
            {
                if (GUILayout.Button("Enable tutorial setup mode"))
                {
                    isTutuorialSetupModeEnabled = true;
                }
            }
            else
            {

                if (GUILayout.Button("Disable tutorial setup mode"))
                {
                    isTutuorialSetupModeEnabled = false;
                }

                if (GUILayout.Button("Clear tutorial list"))
                {
                    selectedLevelRepresentation.ClearTutorialList();
                }

                EditorGUILayout.HelpBox("Use left click to add position to tutorial array. Use right click to remove last entry of position from array.", MessageType.Info);
            }
        }

        #region Creation mode

        private void HandleCreationMode()
        {
            if (!isCreationModeEnabled)
            {
                if (GUILayout.Button("Enable creation mode"))
                {
                    isCreationModeEnabled = true;
                    placements = new bool[selectedLevelRepresentation.widthProperty.intValue, selectedLevelRepresentation.heightProperty.intValue];

                    if (creationModeBlockTypes.IndexOf(gridHandler.selectedCellTypeValue) == -1)//selecting random
                    {
                        AutoSelectRandomBlock();
                    }

                    cellCount = 0;
                    UpdatePlacements();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Disable creation mode"))
                {
                    isCreationModeEnabled = false;
                }

                EditorGUILayout.LabelField($"{cellCount} / {maxCellCount}");

                EditorGUILayout.EndHorizontal();
            }
        }




        private void AutoSelectRandomBlock()
        {
            gridHandler.selectedCellTypeValue = creationModeBlockTypes[UnityEngine.Random.Range(0, creationModeBlockTypes.Count)];
        }

        private void UpdatePlacements()
        {
            for (int y = 0; y < selectedLevelRepresentation.heightProperty.intValue; y++)
            {
                for (int x = 0; x < selectedLevelRepresentation.widthProperty.intValue; x++)
                {
                    if (selectedLevelRepresentation.GetItemsValue(x, y) == 0)
                    {
                        if (y == 0)
                        {
                            placements[x, y] = true;
                        }
                        else if (placements[x, y - 1]) //check up
                        {
                            placements[x, y] = true;

                            //check right for previous elements
                            if ((x > 0) && (!placements[x - 1, y]))
                            {
                                for (int j = x - 1; j >= 0; j--)
                                {
                                    if (selectedLevelRepresentation.GetItemsValue(j, y) == 0)
                                    {
                                        placements[j, y] = true;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                        }
                        else if ((x > 0) && (placements[x - 1, y])) //check left
                        {
                            placements[x, y] = true;
                        }
                        else
                        {
                            placements[x, y] = false;
                        }
                    }
                    else
                    {
                        placements[x, y] = false;
                    }
                }
            }

        }


        #endregion

        private void TestLevel()
        {
            GlobalSave tempSave = SaveController.GetGlobalSave();
            LevelSave levelSave = tempSave.GetSaveObject<LevelSave>(SAVE_NAME);

            levelSave.RealLevelNumber = levelsHandler.SelectedLevelIndex;
            levelSave.DisplayLevelNumber = levelsHandler.SelectedLevelIndex;

            SaveController.SaveCustom(tempSave);
            EditorApplication.ExecuteMenuItem(MENU_ITEM_PATH);
        }


        private void DrawBusSpawnQueue()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginVertical();
            selectedLevelRepresentation.busSpawnQueueProperty.arraySize = EditorGUILayout.IntField(selectedLevelRepresentation.busSpawnQueueProperty.displayName, selectedLevelRepresentation.busSpawnQueueProperty.arraySize);
            EditorGUILayout.EndVertical();

            queueDrawRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));

            posX = queueDrawRect.x;
            posY = queueDrawRect.y;
            CellTypesHandler.CellType cellType;

            itemsPerRow = Mathf.FloorToInt((Screen.width - currentSideBarWidth - 20) / (queueElementSize));
            rowsCount = Mathf.CeilToInt(((selectedLevelRepresentation.busSpawnQueueProperty.arraySize + 1) * 1f) / itemsPerRow);
            GUILayout.Space(rowsCount * queueElementSize);

            //+button
            buttonRect.Set(posX, posY, queueElementSize, queueElementSize);
            DrawColorRect(buttonRect, Color.gray);
            GUI.Label(buttonRect, "+", gridHandler.GetLabelStyle(Color.gray));

            if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
            {
                selectedLevelRepresentation.AddItemToBusSpawnQueue(gridHandler.selectedCellTypeValue);
            }

            posX += queueElementSize;


            for (int i = 0; i < selectedLevelRepresentation.busSpawnQueueProperty.arraySize; i++)
            {
                cellType = gridHandler.GetCellType(selectedLevelRepresentation.busSpawnQueueProperty.GetArrayElementAtIndex(i).enumValueIndex);
                buttonRect.Set(posX, posY, queueElementSize, queueElementSize);
                DrawColorRect(buttonRect, cellType.color);
                GUI.Label(buttonRect, (i + 1).ToString(), gridHandler.GetLabelStyle(cellType.color));

                if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
                {
                    selectedLevelRepresentation.busSpawnQueueProperty.GetArrayElementAtIndex(i).enumValueIndex = gridHandler.selectedCellTypeValue;
                }

                posX += queueElementSize;

                if (posX + queueElementSize > queueDrawRect.xMax)
                {
                    posX = queueDrawRect.x;
                    posY += queueElementSize;
                }

            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawLevel()
        {
            drawRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            xSize = Mathf.Floor(drawRect.width / selectedLevelRepresentation.widthProperty.intValue);
            ySize = Mathf.Floor(drawRect.height / selectedLevelRepresentation.heightProperty.intValue);
            elementSize = Mathf.Min(xSize, ySize);
            currentEvent = Event.current;
            CellTypesHandler.CellType cellType;
            CellTypesHandler.ExtraProp extraProp;


            if (isTutuorialSetupModeEnabled)
            {
                ////Handle click
                if ((currentEvent.type == EventType.MouseDown))
                {
                    elementUnderMouseIndex = (currentEvent.mousePosition - drawRect.position) / (elementSize);

                    elementPosition = new Vector2Int(Mathf.FloorToInt(elementUnderMouseIndex.x), Mathf.FloorToInt(elementUnderMouseIndex.y));

                    if ((elementPosition.x >= 0) && (elementPosition.x < selectedLevelRepresentation.widthProperty.intValue) && (elementPosition.y >= 0) && (elementPosition.y < selectedLevelRepresentation.heightProperty.intValue))
                    {
                        if (currentEvent.button == 0)
                        {
                            selectedLevelRepresentation.AddTutorialStep(elementPosition.x, elementPosition.y);
                            currentEvent.Use();

                        }
                        else if (currentEvent.button == 1)
                        {
                            selectedLevelRepresentation.RemoveLastTutorialStep(elementPosition.x, elementPosition.y);
                            currentEvent.Use();
                        }
                    }
                }
            }
            else if (isCreationModeEnabled)
            {
                ////Handle click
                if ((currentEvent.type == EventType.MouseDown))
                {
                    elementUnderMouseIndex = (currentEvent.mousePosition - drawRect.position) / (elementSize);

                    elementPosition = new Vector2Int(Mathf.FloorToInt(elementUnderMouseIndex.x), Mathf.FloorToInt(elementUnderMouseIndex.y));

                    if ((elementPosition.x >= 0) && (elementPosition.x < selectedLevelRepresentation.widthProperty.intValue) && (elementPosition.y >= 0) && (elementPosition.y < selectedLevelRepresentation.heightProperty.intValue))
                    {
                        if (currentEvent.button == 0)
                        {
                            cellType = gridHandler.GetCellType(selectedLevelRepresentation.GetItemsValue(elementPosition.x, elementPosition.y));

                            if (cellTypes[gridHandler.selectedCellTypeValue] == LevelElement.Type.Spawner)
                            {
                                if (cellTypes[cellType.value] != LevelElement.Type.Spawner)
                                {
                                    selectedLevelRepresentation.SetItemsValue(elementPosition.x, elementPosition.y, gridHandler.selectedCellTypeValue);
                                    selectedLevelRepresentation.AddSpawner(elementPosition.x, elementPosition.y);
                                }
                            }
                            else if (cellTypes[cellType.value] == LevelElement.Type.Spawner)
                            {
                                if ((cellTypes[gridHandler.selectedCellTypeValue] == LevelElement.Type.Wall) || (cellTypes[gridHandler.selectedCellTypeValue] == LevelElement.Type.Empty))
                                {
                                    selectedLevelRepresentation.RemoveSpawner(elementPosition.x, elementPosition.y);
                                    selectedLevelRepresentation.SetItemsValue(elementPosition.x, elementPosition.y, gridHandler.selectedCellTypeValue);
                                }
                                else
                                {
                                    selectedLevelRepresentation.AddBlockToSpawner(elementPosition.x, elementPosition.y, gridHandler.selectedCellTypeValue);
                                    cellCount++;

                                    if (cellCount == maxCellCount)
                                    {
                                        cellCount = 0;
                                        selectedLevelRepresentation.AddItemToBusSpawnQueue(gridHandler.selectedCellTypeValue);
                                        AutoSelectRandomBlock();
                                    }

                                    UpdatePlacements();
                                }
                            }
                            else if (placements[elementPosition.x, elementPosition.y])
                            {
                                selectedLevelRepresentation.SetItemsValue(elementPosition.x, elementPosition.y, gridHandler.selectedCellTypeValue);
                                cellCount++;

                                if (cellCount == maxCellCount)
                                {
                                    cellCount = 0;
                                    selectedLevelRepresentation.AddItemToBusSpawnQueue(gridHandler.selectedCellTypeValue);
                                    AutoSelectRandomBlock();
                                }

                                UpdatePlacements();
                            }

                            currentEvent.Use();
                        }
                        else if (currentEvent.button == 1)
                        {
                            cellType = gridHandler.GetCellType(selectedLevelRepresentation.GetItemsValue(elementPosition.x, elementPosition.y));
                            extraProp = gridHandler.GetExtraProp(selectedLevelRepresentation.GetExtraPropsValue(elementPosition.x, elementPosition.y));
                            GenericMenu menu = new GenericMenu();

                            menuIndex1 = elementPosition.x;
                            menuIndex2 = elementPosition.y;

                            if (cellTypes[cellType.value] == LevelElement.Type.Spawner)
                            {
                                menu.AddItem(new GUIContent(SpawnerMenuOption.TurnSpawnerUp.ToString()), false, SpawnerMenu, SpawnerMenuOption.TurnSpawnerUp);
                                menu.AddItem(new GUIContent(SpawnerMenuOption.TurnSpawnerRight.ToString()), false, SpawnerMenu, SpawnerMenuOption.TurnSpawnerRight);
                                menu.AddItem(new GUIContent(SpawnerMenuOption.TurnSpawnerDown.ToString()), false, SpawnerMenu, SpawnerMenuOption.TurnSpawnerDown);
                                menu.AddItem(new GUIContent(SpawnerMenuOption.TurnSpawnerLeft.ToString()), false, SpawnerMenu, SpawnerMenuOption.TurnSpawnerLeft);
                                menu.AddSeparator(string.Empty);
                                menu.AddItem(new GUIContent(SpawnerMenuOption.ClearSpawnerQueue.ToString()), false, SpawnerMenu, SpawnerMenuOption.ClearSpawnerQueue);
                                menu.AddSeparator(string.Empty);
                                menu.AddItem(new GUIContent(SpawnerMenuOption.RemoveSpawner.ToString()), false, SpawnerMenu, SpawnerMenuOption.RemoveSpawner);
                            }
                            else
                            {
                                foreach (CellTypesHandler.ExtraProp el in gridHandler.extraProps)
                                {
                                    menu.AddItem(new GUIContent(el.label), el.value == extraProp.value, () => selectedLevelRepresentation.SetExtraPropsValue(menuIndex1, menuIndex2, el.value));
                                }
                            }

                            menu.ShowAsContext();
                        }
                    }
                }
            }
            else
            {
                ////Handle drag and click
                if ((currentEvent.type == EventType.MouseDrag) || (currentEvent.type == EventType.MouseDown))
                {
                    elementUnderMouseIndex = (currentEvent.mousePosition - drawRect.position) / (elementSize);

                    elementPosition = new Vector2Int(Mathf.FloorToInt(elementUnderMouseIndex.x), Mathf.FloorToInt(elementUnderMouseIndex.y));

                    if ((elementPosition.x >= 0) && (elementPosition.x < selectedLevelRepresentation.widthProperty.intValue) && (elementPosition.y >= 0) && (elementPosition.y < selectedLevelRepresentation.heightProperty.intValue))
                    {
                        if (currentEvent.button == 0)
                        {
                            cellType = gridHandler.GetCellType(selectedLevelRepresentation.GetItemsValue(elementPosition.x, elementPosition.y));

                            if (cellTypes[gridHandler.selectedCellTypeValue] == LevelElement.Type.Spawner)
                            {
                                if (cellTypes[cellType.value] != LevelElement.Type.Spawner)
                                {
                                    selectedLevelRepresentation.SetItemsValue(elementPosition.x, elementPosition.y, gridHandler.selectedCellTypeValue);
                                    selectedLevelRepresentation.AddSpawner(elementPosition.x, elementPosition.y);
                                }
                            }
                            else if (cellTypes[cellType.value] == LevelElement.Type.Spawner)
                            {
                                if ((cellTypes[gridHandler.selectedCellTypeValue] == LevelElement.Type.Wall) || (cellTypes[gridHandler.selectedCellTypeValue] == LevelElement.Type.Empty))
                                {
                                    selectedLevelRepresentation.RemoveSpawner(elementPosition.x, elementPosition.y);
                                    selectedLevelRepresentation.SetItemsValue(elementPosition.x, elementPosition.y, gridHandler.selectedCellTypeValue);
                                }
                                else
                                {
                                    selectedLevelRepresentation.AddBlockToSpawner(elementPosition.x, elementPosition.y, gridHandler.selectedCellTypeValue);
                                }
                            }
                            else
                            {
                                selectedLevelRepresentation.SetItemsValue(elementPosition.x, elementPosition.y, gridHandler.selectedCellTypeValue);
                            }

                            currentEvent.Use();
                        }
                        else if ((currentEvent.button == 1) && (currentEvent.type == EventType.MouseDown))
                        {
                            cellType = gridHandler.GetCellType(selectedLevelRepresentation.GetItemsValue(elementPosition.x, elementPosition.y));
                            extraProp = gridHandler.GetExtraProp(selectedLevelRepresentation.GetExtraPropsValue(elementPosition.x, elementPosition.y));
                            GenericMenu menu = new GenericMenu();

                            menuIndex1 = elementPosition.x;
                            menuIndex2 = elementPosition.y;

                            if (cellTypes[cellType.value] == LevelElement.Type.Spawner)
                            {
                                menu.AddItem(new GUIContent(SpawnerMenuOption.TurnSpawnerUp.ToString()), false, SpawnerMenu, SpawnerMenuOption.TurnSpawnerUp);
                                menu.AddItem(new GUIContent(SpawnerMenuOption.TurnSpawnerRight.ToString()), false, SpawnerMenu, SpawnerMenuOption.TurnSpawnerRight);
                                menu.AddItem(new GUIContent(SpawnerMenuOption.TurnSpawnerDown.ToString()), false, SpawnerMenu, SpawnerMenuOption.TurnSpawnerDown);
                                menu.AddItem(new GUIContent(SpawnerMenuOption.TurnSpawnerLeft.ToString()), false, SpawnerMenu, SpawnerMenuOption.TurnSpawnerLeft);
                                menu.AddSeparator(string.Empty);
                                menu.AddItem(new GUIContent(SpawnerMenuOption.ClearSpawnerQueue.ToString()), false, SpawnerMenu, SpawnerMenuOption.ClearSpawnerQueue);
                                menu.AddSeparator(string.Empty);
                                menu.AddItem(new GUIContent(SpawnerMenuOption.RemoveSpawner.ToString()), false, SpawnerMenu, SpawnerMenuOption.RemoveSpawner);
                            }
                            else
                            {
                                foreach (CellTypesHandler.ExtraProp el in gridHandler.extraProps)
                                {
                                    menu.AddItem(new GUIContent(el.label), el.value == extraProp.value, () => selectedLevelRepresentation.SetExtraPropsValue(menuIndex1, menuIndex2, el.value));
                                }
                            }

                            menu.ShowAsContext();
                        }
                    }
                }
            }


            //draw

            for (int y = 0; y < selectedLevelRepresentation.heightProperty.intValue; y++)
            {
                for (int x = 0; x < selectedLevelRepresentation.widthProperty.intValue; x++)
                {
                    cellType = gridHandler.GetCellType(selectedLevelRepresentation.GetItemsValue(x, y));
                    extraProp = gridHandler.GetExtraProp(selectedLevelRepresentation.GetExtraPropsValue(x, y));
                    buttonRectX = drawRect.position.x + x * elementSize;
                    buttonRectY = drawRect.position.y + y * elementSize;
                    buttonRect = new Rect(buttonRectX, buttonRectY, elementSize, elementSize);

                    if (cellTypes[cellType.value] == LevelElement.Type.Spawner)
                    {
                        tempSpawnerData = selectedLevelRepresentation.GetSpawnerData(x, y);
                        DrawSpawner(tempSpawnerData, buttonRect, cellType.color);
                    }
                    else if ((isCreationModeEnabled) && (placements[x, y]))
                    {
                        DrawColorRect(buttonRect, Color.white);
                    }
                    else
                    {
                        DrawColorRect(buttonRect, cellType.color);
                    }

                    if (isTutuorialSetupModeEnabled)
                    {
                        cellTutorialStepLabel = selectedLevelRepresentation.GetTutorialStepPositionLabel(x, y, stringBuilder);

                        if (cellTutorialStepLabel.Length != 0)
                        {
                            GUI.Label(buttonRect, cellTutorialStepLabel, gridHandler.GetLabelStyle(cellType.color));
                        }
                    }
                    else if (extraProp.value != 0)
                    {
                        GUI.Label(buttonRect, extraProp.label, gridHandler.GetLabelStyle(cellType.color));
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void SpawnerMenu(object dataObject)
        {
            SpawnerMenuOption menuOption = (SpawnerMenuOption)dataObject;
            if ((int)menuOption < 4) //rotating
            {
                selectedLevelRepresentation.RotateSpawner(menuIndex1, menuIndex2, (int)menuOption);
            }
            else if (menuOption == SpawnerMenuOption.ClearSpawnerQueue)
            {
                selectedLevelRepresentation.ClearSpawnerQueue(menuIndex1, menuIndex2);
            }
            else if (menuOption == SpawnerMenuOption.RemoveSpawner)
            {
                selectedLevelRepresentation.RemoveSpawner(menuIndex1, menuIndex2);
            }

        }

        private void DrawSpawner(SpawnerRepresentation tempSpawnerData, Rect buttonRect, Color color)
        {
            float rectSize;
            Rect workRect = new Rect(buttonRect);

            if (tempSpawnerData.blocksEnumValueIndex.Length <= 4)
            {
                rectSize = buttonRect.width / 2f;
                workRect.width = rectSize;
                workRect.height = rectSize;

                for (int i = 0; i < tempSpawnerData.blocksEnumValueIndex.Length; i++)
                {
                    DrawColorRect(workRect, gridHandler.GetCellType(tempSpawnerData.blocksEnumValueIndex[i]).color);

                    if (i == 1)
                    {
                        workRect.y = buttonRect.y + rectSize;
                        workRect.x = buttonRect.x;
                    }
                    else
                    {
                        workRect.x += rectSize;
                    }
                }

                workRect.Set(buttonRect.x + rectSize - 1, buttonRect.y, 2, buttonRect.height);
                DrawColorRect(workRect, color);
                workRect.Set(buttonRect.x, buttonRect.y + rectSize - 1, buttonRect.width, 2);
                DrawColorRect(workRect, color);

            }
            else if (tempSpawnerData.blocksEnumValueIndex.Length <= 9)
            {
                rectSize = buttonRect.width / 3f;
                workRect.width = rectSize;
                workRect.height = rectSize;

                for (int i = 0; i < tempSpawnerData.blocksEnumValueIndex.Length; i++)
                {
                    DrawColorRect(workRect, gridHandler.GetCellType(tempSpawnerData.blocksEnumValueIndex[i]).color);

                    if (i == 2)
                    {
                        workRect.y = buttonRect.y + rectSize;
                        workRect.x = buttonRect.x;
                    }
                    else if (i == 5)
                    {
                        workRect.y = buttonRect.y + rectSize + rectSize;
                        workRect.x = buttonRect.x;
                    }
                    else
                    {
                        workRect.x += rectSize;
                    }
                }

                workRect.Set(buttonRect.x + rectSize - 1, buttonRect.y, 2, buttonRect.height);
                DrawColorRect(workRect, color);
                workRect.Set(buttonRect.x + rectSize + rectSize - 1, buttonRect.y, 2, buttonRect.height);
                DrawColorRect(workRect, color);
                workRect.Set(buttonRect.x, buttonRect.y + rectSize - 1, buttonRect.width, 2);
                DrawColorRect(workRect, color);
                workRect.Set(buttonRect.x, buttonRect.y + rectSize + rectSize - 1, buttonRect.width, 2);
                DrawColorRect(workRect, color);
            }

            defaultColor = GUI.color;
            GUI.color = color;

            if (tempSpawnerData.directionEnumValueIndex == 0)// for top no rotation nessesary
            {
                GUI.DrawTexture(buttonRect, (Texture2D)editorSpawnerTexureSerializedProperty.objectReferenceValue);
            }
            else
            {
                matrixBackup = GUI.matrix;
                GUIUtility.RotateAroundPivot(90 * tempSpawnerData.directionEnumValueIndex, buttonRect.center);
                GUI.DrawTexture(buttonRect, (Texture2D)editorSpawnerTexureSerializedProperty.objectReferenceValue);
                GUI.matrix = matrixBackup;
            }

            GUI.color = defaultColor;
        }

        private void DrawTipsAndWarnings()
        {
            infoRect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(INFO_HEIGH));

            if (selectedLevelRepresentation.IsLevelCorrect)
            {
                EditorGUILayout.HelpBox(LEVEL_PASSED_VALIDATION, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(selectedLevelRepresentation.errorLabels[0], MessageType.Error);
            }

            EditorGUILayout.HelpBox(LEVEL_INSTRUCTION + '\n' + RIGHT_CLICK_INSTRUCTION, MessageType.Info);
            EditorGUILayout.EndVertical();
            //Debug.Log(infoRect.height);
        }

        public override void OnBeforeAssemblyReload()
        {
            lastActiveLevelOpened = false;
        }


        public override bool WindowClosedInPlaymode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                lastActiveLevelOpened = false;
            }

            return false;
        }

        internal class SpawnerRepresentation
        {
            public int directionEnumValueIndex;
            public int[] blocksEnumValueIndex;
        }

        private enum SpawnerMenuOption
        {
            TurnSpawnerUp,
            TurnSpawnerRight,
            TurnSpawnerDown,
            TurnSpawnerLeft,
            ClearSpawnerQueue,
            RemoveSpawner
        }

        protected class LevelRepresentation : LevelRepresentationBase
        {
            private const string ELEMENTS_DATA_PROPERTY_NAME = "elementsData";
            private const string WIDTH_PROPERTY_NAME = "width";
            private const string HEIGHT_PROPERTY_NAME = "height";
            private const string NOTE_PROPERTY_NAME = "note";
            private const string SPAWNER_DATA_PROPERTY_NAME = "spawnerData";
            private const string TUTORIAL_STEPS_PROPERTY_NAME = "tutorialSteps";
            private const string BUS_SPAWN_QUEUE_PROPERTY_NAME = "busSpawnQueue";
            private const string COINS_REWARD_PROPERTY_NAME = "coinsReward";
            private const string USE_IN_RANDOMIZER_PROPERTY_NAME = "useInRandomizer";

            public SerializedProperty elementsDataProperty;
            public SerializedProperty widthProperty;
            public SerializedProperty heightProperty;
            public SerializedProperty spawnerDataProperty;
            public SerializedProperty noteProperty;
            public SerializedProperty tutorialStepsProperty;
            public SerializedProperty busSpawnQueueProperty;
            public SerializedProperty coinsRewardProperty;
            public SerializedProperty useInRandomizerProperty;


            private const string ELEMENT_TYPE_PROPERTY_NAME = "elementType";
            private const string ELEMENT_POSITION_PROPERTY_NAME = "elementPosition";
            private const string SPECIAL_EFFECT_TYPE_PROPERTY_NAME = "specialEffectType";
            private const string X_PROPERTY_NAME = "x";
            private const string Y_PROPERTY_NAME = "y";
            private const string DIRECTION_PROPERTY_NAME = "direction";
            private const string SPAWN_QUEUE_PROPERTY_NAME = "spawnQueue";
            private bool useWidthForIndexes;

            public static SerializedProperty levelsSerializedProperty;
            public int currentLevelIndex;
            public int currentStageIndex;

            protected override bool LEVEL_CHECK_ENABLED => true;

            public LevelRepresentation(UnityEngine.Object levelObject) : base(levelObject)
            {
            }

            protected override void ReadFields()
            {
                elementsDataProperty = serializedLevelObject.FindProperty(ELEMENTS_DATA_PROPERTY_NAME);
                spawnerDataProperty = serializedLevelObject.FindProperty(SPAWNER_DATA_PROPERTY_NAME);
                widthProperty = serializedLevelObject.FindProperty(WIDTH_PROPERTY_NAME);
                heightProperty = serializedLevelObject.FindProperty(HEIGHT_PROPERTY_NAME);
                noteProperty = serializedLevelObject.FindProperty(NOTE_PROPERTY_NAME);
                tutorialStepsProperty = serializedLevelObject.FindProperty(TUTORIAL_STEPS_PROPERTY_NAME);
                busSpawnQueueProperty = serializedLevelObject.FindProperty(BUS_SPAWN_QUEUE_PROPERTY_NAME);
                coinsRewardProperty = serializedLevelObject.FindProperty(COINS_REWARD_PROPERTY_NAME);
                useInRandomizerProperty = serializedLevelObject.FindProperty(USE_IN_RANDOMIZER_PROPERTY_NAME);

                useWidthForIndexes = (widthProperty.intValue >= heightProperty.intValue);
            }

            public override void Clear()
            {
                elementsDataProperty.arraySize = 0;
                spawnerDataProperty.arraySize = 0;
                widthProperty.intValue = 0;
                heightProperty.intValue = 0;
                noteProperty.stringValue = string.Empty;
                tutorialStepsProperty.arraySize = 0;
                busSpawnQueueProperty.arraySize = 0;
                ApplyChanges();
            }

            public int GetItemsValue(int x, int y)
            {
                return GetItemProperty(x, y).FindPropertyRelative(ELEMENT_TYPE_PROPERTY_NAME).enumValueIndex;
            }

            public void SetItemsValue(int x, int y, int newValue)
            {
                GetItemProperty(x, y).FindPropertyRelative(ELEMENT_TYPE_PROPERTY_NAME).enumValueIndex = newValue;
            }

            public void AddItemToBusSpawnQueue(int selectedCellTypeValue)
            {
                busSpawnQueueProperty.InsertArrayElementAtIndex(0);
                busSpawnQueueProperty.GetArrayElementAtIndex(0).enumValueIndex = selectedCellTypeValue;
            }

            public SerializedProperty GetItemProperty(int x, int y)
            {
                if (useWidthForIndexes)
                {
                    return elementsDataProperty.GetArrayElementAtIndex(y * widthProperty.intValue + x);
                }
                else
                {
                    return elementsDataProperty.GetArrayElementAtIndex(x * heightProperty.intValue + y);
                }
            }

            public int GetExtraPropsValue(int x, int y)
            {
                return GetItemProperty(x, y).FindPropertyRelative(SPECIAL_EFFECT_TYPE_PROPERTY_NAME).enumValueIndex;
            }

            public void SetExtraPropsValue(int x, int y, int newValue)
            {
                GetItemProperty(x, y).FindPropertyRelative(SPECIAL_EFFECT_TYPE_PROPERTY_NAME).enumValueIndex = newValue;
            }

            public void HandleSizePropertyChange()
            {
                if (widthProperty.intValue < 2)
                {
                    widthProperty.intValue = 2;
                }

                if (heightProperty.intValue < 2)
                {
                    heightProperty.intValue = 2;
                }

                elementsDataProperty.arraySize = widthProperty.intValue * heightProperty.intValue;
                useWidthForIndexes = (widthProperty.intValue >= heightProperty.intValue);

                SerializedProperty positionProperty;

                for (int y = 0; y < heightProperty.intValue; y++)
                {
                    for (int x = 0; x < widthProperty.intValue; x++)
                    {
                        positionProperty = GetItemProperty(x, y).FindPropertyRelative(ELEMENT_POSITION_PROPERTY_NAME);
                        positionProperty.FindPropertyRelative(X_PROPERTY_NAME).intValue = x;
                        positionProperty.FindPropertyRelative(Y_PROPERTY_NAME).intValue = y;
                    }
                }
            }

            public void AddEmptyRow()
            {
                //backup
                int[,] fieldsValue = new int[widthProperty.intValue, heightProperty.intValue];
                int[,] extraFieldsValue = new int[widthProperty.intValue, heightProperty.intValue];

                for (int y = 0; y < heightProperty.intValue; y++)
                {
                    for (int x = 0; x < widthProperty.intValue; x++)
                    {
                        fieldsValue[x, y] = GetItemsValue(x, y);
                        extraFieldsValue[x, y] = GetExtraPropsValue(x, y);
                    }
                }

                //change size
                heightProperty.intValue++;
                HandleSizePropertyChange();

                //recover
                for (int y = 0; y < heightProperty.intValue - 1; y++)
                {
                    for (int x = 0; x < widthProperty.intValue; x++)
                    {
                        SetItemsValue(x, y + 1, fieldsValue[x, y]);
                        SetExtraPropsValue(x, y + 1, extraFieldsValue[x, y]);
                    }
                }

                //fill empty
                for (int x = 0; x < widthProperty.intValue; x++)
                {
                    SetItemsValue(x, 0, 0);
                }

                //Update spawners position
                SerializedProperty elementProperty;
                SerializedProperty positionProperty;

                for (int i = 0; i < spawnerDataProperty.arraySize; i++)
                {
                    elementProperty = spawnerDataProperty.GetArrayElementAtIndex(i);
                    positionProperty = elementProperty.FindPropertyRelative(ELEMENT_POSITION_PROPERTY_NAME);
                    positionProperty.FindPropertyRelative(Y_PROPERTY_NAME).intValue++;
                }
            }

            public override string GetLevelLabel(int index, StringBuilder stringBuilder)
            {
                if (NullLevel || (!IsLevelCorrect))
                {
                    return base.GetLevelLabel(index, stringBuilder);
                }
                else
                {
                    return base.GetLevelLabel(index, stringBuilder) + SEPARATOR +  noteProperty.stringValue;
                }
            }

            public void AddSpawner(int x, int y)
            {
                spawnerDataProperty.arraySize++;
                SerializedProperty elementProperty = spawnerDataProperty.GetArrayElementAtIndex(spawnerDataProperty.arraySize - 1);
                SerializedProperty positionProperty = elementProperty.FindPropertyRelative(ELEMENT_POSITION_PROPERTY_NAME);
                positionProperty.FindPropertyRelative(X_PROPERTY_NAME).intValue = x;
                positionProperty.FindPropertyRelative(Y_PROPERTY_NAME).intValue = y;

                elementProperty.FindPropertyRelative(DIRECTION_PROPERTY_NAME).enumValueIndex = 0;
                elementProperty.FindPropertyRelative(SPAWN_QUEUE_PROPERTY_NAME).arraySize = 0;
            }

            public void AddBlockToSpawner(int x, int y, int selectedCellTypeValue)
            {
                SerializedProperty elementProperty = GetSpawnerElementProperty(x, y);
                SerializedProperty queueProperty = elementProperty.FindPropertyRelative(SPAWN_QUEUE_PROPERTY_NAME);
                queueProperty.arraySize++;
                queueProperty.GetArrayElementAtIndex(queueProperty.arraySize - 1).enumValueIndex = selectedCellTypeValue;
            }

            public void RotateSpawner(int x, int y, int directionEnumValueIndex)
            {
                SerializedProperty elementProperty = GetSpawnerElementProperty(x, y);
                elementProperty.FindPropertyRelative(DIRECTION_PROPERTY_NAME).enumValueIndex = directionEnumValueIndex;
            }

            public void ClearSpawnerQueue(int x, int y)
            {
                SerializedProperty elementProperty = GetSpawnerElementProperty(x, y);
                elementProperty.FindPropertyRelative(SPAWN_QUEUE_PROPERTY_NAME).arraySize = 0;
            }

            public void ClearField(List<int> creationModeBlockTypes)
            {
                //Clear spawner 
                for (int i = 0; i < spawnerDataProperty.arraySize; i++)
                {
                    spawnerDataProperty.GetArrayElementAtIndex(i).FindPropertyRelative(SPAWN_QUEUE_PROPERTY_NAME).arraySize = 0;
                }

                int currentValue;

                for (int x = 0; x < widthProperty.intValue; x++)
                {
                    for (int y = 0; y < heightProperty.intValue; y++)
                    {
                        currentValue = GetItemsValue(x, y);

                        if (creationModeBlockTypes.Contains(currentValue))
                        {
                            SetItemsValue(x, y, 0);
                        }
                    }
                }

                busSpawnQueueProperty.arraySize = 0;
            }

            internal SpawnerRepresentation GetSpawnerData(int x, int y)
            {
                SerializedProperty elementProperty = GetSpawnerElementProperty(x, y);
                SpawnerRepresentation spawnerRepresentation = new SpawnerRepresentation();
                spawnerRepresentation.directionEnumValueIndex = elementProperty.FindPropertyRelative(DIRECTION_PROPERTY_NAME).enumValueIndex;
                SerializedProperty queueProperty = elementProperty.FindPropertyRelative(SPAWN_QUEUE_PROPERTY_NAME);
                spawnerRepresentation.blocksEnumValueIndex = new int[queueProperty.arraySize];

                for (int i = 0; i < spawnerRepresentation.blocksEnumValueIndex.Length; i++)
                {
                    spawnerRepresentation.blocksEnumValueIndex[i] = queueProperty.GetArrayElementAtIndex(i).enumValueIndex;
                }

                return spawnerRepresentation;
            }

            public void RemoveSpawner(int x, int y)
            {
                SerializedProperty elementProperty;
                SerializedProperty positionProperty;

                for (int i = 0; i < spawnerDataProperty.arraySize; i++)
                {
                    elementProperty = spawnerDataProperty.GetArrayElementAtIndex(i);
                    positionProperty = elementProperty.FindPropertyRelative(ELEMENT_POSITION_PROPERTY_NAME);

                    if ((positionProperty.FindPropertyRelative(X_PROPERTY_NAME).intValue == x) && (positionProperty.FindPropertyRelative(Y_PROPERTY_NAME).intValue == y))
                    {
                        spawnerDataProperty.DeleteArrayElementAtIndex(i);
                        SetItemsValue(x, y, 0);
                        return;
                    }
                }
            }

            private SerializedProperty GetSpawnerElementProperty(int x, int y)
            {
                SerializedProperty elementProperty;
                SerializedProperty positionProperty;

                for (int i = 0; i < spawnerDataProperty.arraySize; i++)
                {
                    elementProperty = spawnerDataProperty.GetArrayElementAtIndex(i);
                    positionProperty = elementProperty.FindPropertyRelative(ELEMENT_POSITION_PROPERTY_NAME);

                    if ((positionProperty.FindPropertyRelative(X_PROPERTY_NAME).intValue == x) && (positionProperty.FindPropertyRelative(Y_PROPERTY_NAME).intValue == y))
                    {
                        return elementProperty;
                    }
                }

                throw new Exception($"SpawnerElement not found for x:{x},y:{y}");
            }
            public void ClearTutorialList()
            {
                tutorialStepsProperty.arraySize = 0;
            }

            public string GetTutorialStepPositionLabel(int x, int y, StringBuilder stringBuilder)
            {
                stringBuilder.Clear();
                SerializedProperty elementProperty;
                SerializedProperty positionProperty;

                for (int i = 0; i < tutorialStepsProperty.arraySize; i++)
                {
                    elementProperty = tutorialStepsProperty.GetArrayElementAtIndex(i);
                    positionProperty = elementProperty.FindPropertyRelative(ELEMENT_POSITION_PROPERTY_NAME);

                    if ((positionProperty.FindPropertyRelative(X_PROPERTY_NAME).intValue == x) && (positionProperty.FindPropertyRelative(Y_PROPERTY_NAME).intValue == y))
                    {
                        if (stringBuilder.Length != 0)
                        {
                            stringBuilder.Append(", ");
                        }

                        stringBuilder.Append(i);
                    }
                }

                return stringBuilder.ToString();
            }

            public void RemoveLastTutorialStep(int x, int y)
            {
                SerializedProperty elementProperty;
                SerializedProperty positionProperty;

                for (int i = tutorialStepsProperty.arraySize - 1; i >= 0; i--)
                {
                    elementProperty = tutorialStepsProperty.GetArrayElementAtIndex(i);
                    positionProperty = elementProperty.FindPropertyRelative(ELEMENT_POSITION_PROPERTY_NAME);

                    if ((positionProperty.FindPropertyRelative(X_PROPERTY_NAME).intValue == x) && (positionProperty.FindPropertyRelative(Y_PROPERTY_NAME).intValue == y))
                    {
                        tutorialStepsProperty.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }

            public void AddTutorialStep(int x, int y)
            {
                SerializedProperty elementProperty;
                SerializedProperty positionProperty;
                tutorialStepsProperty.arraySize++;
                elementProperty = tutorialStepsProperty.GetArrayElementAtIndex(tutorialStepsProperty.arraySize - 1);
                positionProperty = elementProperty.FindPropertyRelative(ELEMENT_POSITION_PROPERTY_NAME);

                positionProperty.FindPropertyRelative(X_PROPERTY_NAME).intValue = x;
                positionProperty.FindPropertyRelative(Y_PROPERTY_NAME).intValue = y;
            }

            public void Validate(CellTypesHandler gridHandler, List<int> creationModeBlockTypes, List<LevelElement.Type> cellTypes)
            {
                errorLabels.Clear();
                int[] counter = new int[creationModeBlockTypes.Count];
                CellTypesHandler.CellType cellType;
                SpawnerRepresentation tempSpawnerData;
                int index;

                for (int y = 0; y < heightProperty.intValue; y++)
                {
                    for (int x = 0; x < widthProperty.intValue; x++)
                    {
                        cellType = gridHandler.GetCellType(GetItemsValue(x, y));
                        index = creationModeBlockTypes.IndexOf(cellType.value);

                        if (cellTypes[cellType.value] == LevelElement.Type.Spawner)
                        {
                            tempSpawnerData = GetSpawnerData(x, y);

                            for (int i = 0; i < tempSpawnerData.blocksEnumValueIndex.Length; i++)
                            {
                                index = creationModeBlockTypes.IndexOf(tempSpawnerData.blocksEnumValueIndex[i]);

                                if (index != -1)
                                {
                                    counter[index]++;
                                }
                            }

                        }
                        else if (index != -1)
                        {
                            counter[index]++;
                        }
                    }
                }

                if(heightProperty.intValue > 10)
                {
                    errorLabels.Add($"Level too tall. Max allowed height is 10.");
                }


                for (int i = 0; i < counter.Length; i++)
                {
                    if (counter[i] % 3 == 1)
                    {
                        errorLabels.Add($"Incorrect {cellTypes[creationModeBlockTypes[i]]} amount. Need 2 more to create a full set.");
                    }
                    else if(counter[i] % 3 == 2)
                    {
                        errorLabels.Add($"Incorrect {cellTypes[creationModeBlockTypes[i]]} amount. Need 1 more to create a full set.");
                    }
                }

                //collecting data for a list
                List<int> busSpawnQueueList = new List<int>();

                for (int i = 0; i < busSpawnQueueProperty.arraySize; i++)
                {
                    busSpawnQueueList.Add(busSpawnQueueProperty.GetArrayElementAtIndex(i).enumValueIndex);
                }


                if(busSpawnQueueList.Count == 0)
                {
                    errorLabels.Add($"BusSpawnQueue is empty.");
                }

                int numberToCheck;
                int quantity;

                for (int i = 0; i < counter.Length; i++)
                {
                    if (counter[i] == 0)
                    {
                        continue;
                    }

                    quantity = counter[i] / 3;
                    numberToCheck = creationModeBlockTypes[i];

                    for (int j = 0; j < quantity; j++)
                    {
                        if (!busSpawnQueueList.Remove(numberToCheck))
                        {
                            errorLabels.Add($"There is not enough {cellTypes[creationModeBlockTypes[i]]}  elements in  busSpawnQueue.");
                        }
                    }
                }

                if (busSpawnQueueList.Count != 0)
                {
                    for (int i = 0; i < busSpawnQueueList.Count; i++)
                    {
                        errorLabels.Add($"There is extra {cellTypes[busSpawnQueueList[i]]}  element in  busSpawnQueue.");
                    }
                }

            }

            public void UpdateNote()
            {
                LevelData level = serializedLevelObject.targetObject as LevelData;

                string note = string.Empty;

                if(level.BusSpawnQueue != null)
                {
                    note += level.BusSpawnQueue.Length;
                }

                int spanwers = level.GetSpawnersAmount();
                if (spanwers > 0)
                {
                    note += "  SPA x" + spanwers;
                }

                int crates = level.GetCratesAmount();
                if (crates > 0)
                {
                    note += "  CRA x" + crates;
                }

                noteProperty.stringValue = note;
            }
        }
    }
}

// -----------------
// 2d grid level editor V1.2.1
// -----------------

// Changelog
// v 1.2.1
// • Some small fixes after update
// v 1.2
// • Reordered some methods
// v 1.1
// • Added global validation
// • Added validation example
// • Fixed mouse click bug
// v 1 basic version works