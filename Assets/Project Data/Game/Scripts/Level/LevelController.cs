using System.Collections.Generic;
using UnityEngine;
using Watermelon.SkinStore;

namespace Watermelon.BusStop
{
    public class LevelController : MonoBehaviour
    {
        private static LevelController instance;

        [SerializeField] public LevelDatabase database;
        [SerializeField] public int levelIndex;
        [SerializeField] public FemaleTileManager _femaleTileManager;
        [SerializeField] public GameObject _femaleTilePrefab;

        [Space]
        [SerializeField] TileManager tileManager;


        public static EnvironmentBehavior Environment { get; private set; }

        [Space]
        [SerializeField] float levelElementSize = 1.0f;

        [Space]
        [SerializeField] float startZ = 4.5f;
        public static float StartZ => instance.startZ;
        [SerializeField] float posY = 0f;
        public static float PosY => instance.posY;
                      
        private static bool isStageLoaded;

        private static UIGame gameUI;

        private static LevelData loadedLevelData;
        public static LevelData LoadedStageData => loadedLevelData;

        private static LevelSave levelSave;

        public static int DisplayLevelNumber => levelSave.DisplayLevelNumber;
        public static int RealLevelNumber => levelSave.RealLevelNumber;

        public static int CurrentReward => loadedLevelData != null ? loadedLevelData.CoinsReward : -1;

        private static ElementTypeMap levelMap;
        public static ElementTypeMap LevelMap => levelMap;

        private static int levelWidth;
        private static int levelHeight;

        private static LevelTutorial levelTutorial;

        public static List<LevelElementBehavior> levelElements;
        private static List<LevelElementBehavior> highlightedElements;

        private static Dictionary<LevelData.SpecialEffectType, ElementSpecialEffect> levelEffectsLink;
        private static Dictionary<LevelElement.Type, LevelElement> levelElementsLink;

        public static DockBehavior Dock => Environment.Dock;

        public void Initialise()
        {
            instance = this;

            levelSave = SaveController.GetSaveObject<LevelSave>("level");
            gameUI = UIController.GetPage<UIGame>();
            levelTutorial = new LevelTutorial();

            database.Initialise();

            levelElementsLink = new Dictionary<LevelElement.Type, LevelElement>();
            LevelElement[] availableLevelElements = database.LevelElements;
            for (int i = 0; i < availableLevelElements.Length; i++)
            {
                if (levelElementsLink.ContainsKey(availableLevelElements[i].ElementType))
                {
                    Debug.LogError(string.Format("Level element with type {0} has duplicates in the database!", availableLevelElements[i].ElementType));

                    continue;
                }

                availableLevelElements[i].Initialise();

                levelElementsLink.Add(availableLevelElements[i].ElementType, availableLevelElements[i]);
            }

            levelEffectsLink = new Dictionary<LevelData.SpecialEffectType, ElementSpecialEffect>();
            ElementSpecialEffect[] availableSpecialEffects = database.SpecialEffects;
            for (int i = 0; i < availableSpecialEffects.Length; i++)
            {
                if (levelEffectsLink.ContainsKey(availableSpecialEffects[i].EffectType))
                {
                    Debug.LogError(string.Format("Element effect with type {0} has duplicates in the database!", availableSpecialEffects[i].EffectType));

                    continue;
                }

                levelEffectsLink.Add(availableSpecialEffects[i].EffectType, availableSpecialEffects[i]);
            }

            SkinStoreController.OnProductSelected += ChangeEnvironment;

            Environment = Instantiate(SkinStoreController.GetSelectedProductData(SkinTab.World).Prefab).GetComponent<EnvironmentBehavior>();
            EnvironmentBehavior.InitialiseEnvironment();

            Dock.Initialise(this);

            tileManager.Init(Environment.EnvironmentTileData);
        }

        private void ChangeEnvironment(SkinTab tab, SkinData product)
        {
            Destroy(Environment.gameObject);

            Environment = Instantiate(SkinStoreController.GetSelectedProductData(SkinTab.World).Prefab).GetComponent<EnvironmentBehavior>();
            Dock.Initialise(this);

            tileManager.Init(Environment.EnvironmentTileData);

            tileManager.GenerateTileMap(levelMap);
        }

        public void LoadLevel(SimpleCallback onLoaded = null)
        {
            levelIndex = database.GetRandomLevelIndex(levelSave.DisplayLevelNumber, levelSave.RealLevelNumber, levelSave.ReplayingLevelAgain);
            loadedLevelData = database.GetLevel(levelIndex);
            levelSave.RealLevelNumber = levelIndex;

            if (isStageLoaded)
                UnloadStage();

            levelElements = new List<LevelElementBehavior>();
            highlightedElements = new List<LevelElementBehavior>();

            levelWidth = loadedLevelData.Width;
            levelHeight = loadedLevelData.Height;

            // Create empty map
            levelMap = new ElementTypeMap(levelWidth, levelHeight);

            // Mark all other elements as empty objects
            for (int x = 0; x < levelWidth; x++)
            {
                for (int y = 1; y < levelHeight; y++)
                {
                    levelMap[x, y] = LevelElement.Type.Empty;
                }
            }

            // Spawn level objects
            ElementData[] levelData = loadedLevelData.ElementsData;
            for (int i = 0; i < levelData.Length; i++)
            {
                SpawnLevelObject(levelData[i], false, false);
            }

            OnMapChanged(true);

            tileManager.GenerateTileMap(levelMap);
            //EnvironmentBehavior.SetBusQueue(loadedLevelData.BusSpawnQueue);
            //EnvironmentBehavior.StartSpawningBusses();

            levelTutorial.Initialise(loadedLevelData.TutorialSteps);

            PUController.PowerUpsUIController.OnLevelStarted(levelIndex);

            isStageLoaded = true;

            RaycastController.Enable();

            onLoaded?.Invoke();

            InitialzeFemaleTiles();
        }

        public void InitialzeFemaleTiles() 
        {
            Debug.Log("Level Initialized");
            if (_femaleTileManager != null) 
            {
                Destroy(_femaleTileManager.gameObject);
            }
            _femaleTileManager = Instantiate(_femaleTilePrefab).GetComponent<FemaleTileManager>();
            GetComponent<GameController>()._femaleTileManager = _femaleTileManager;
            _femaleTileManager.gameObject.SetActive(true);
        }

        private void LoadLevelStage(SimpleCallback onLevelLoaded = null)
        {

        }

        public void AdjustLevelNumber()
        {
            levelSave.DisplayLevelNumber++;

            SaveController.MarkAsSaveIsRequired();
        }

        public static void OnMapChanged(bool firstSpawn = false)
        {
            List<ElementPosition> tempElements = MapUtils.GetAvailableElementsNew(levelMap);
            List<LevelElementBehavior> newHighlightedElements = new List<LevelElementBehavior>();
            for (int i = 0; i < tempElements.Count; i++)
            {
                LevelElementBehavior levelElementBehavior = GetLevelElement(tempElements[i]);
                if (levelElementBehavior != null)
                {
                    int highlightedElementID = highlightedElements.FindIndex(x => x == levelElementBehavior);
                    if (highlightedElementID == -1)
                    {
                        levelElementBehavior.Highlight(firstSpawn);
                    }
                    else
                    {
                        highlightedElements.RemoveAt(highlightedElementID);
                    }

                    newHighlightedElements.Add(levelElementBehavior);
                }
            }

            if (highlightedElements.Count > 0)
            {
                for (int i = 0; i < highlightedElements.Count; i++)
                {
                    highlightedElements[i].Unhighlight();
                }
            }

            highlightedElements = newHighlightedElements;

            for (int i = 0; i < levelElements.Count; i++)
            {
                if (!levelElements[i].IsSubmitted)
                {
                    levelElements[i].OnMapUpdated();
                }
            }
        }

        public static void OnElementClicked(BaseCharacterBehavior levelElementBehavior, ElementPosition elementPosition)
        {
            levelTutorial.OnElementClicked(levelElementBehavior, elementPosition);
        }

        public static bool SubmitIsAllowed()
        {
            /*if (Dock.IsAlmostFilled)
            {
                if (!Dock.finalSubmitted)
                {
                    Dock.finalSubmitted = true;
                }
                else 
                {
                    return false;
                }
            }*/
            bool filled = true;
            foreach (SlotBehavior slot in Dock.slots2) 
            {
                if (!slot.IsOccupied) 
                {
                    filled = false;
                } 
            }
            //return !Dock.IsFilled;
            return !filled;
        }

        public static void OnElementSubmittedToSlot(BaseCharacterBehavior blockElementBehavior, bool instant)
        {
            if (!Dock.IsFilled)
            {
                levelElements.Remove(blockElementBehavior);
                blockElementBehavior.isDocked = true;
                Dock.SubmitToSlot(blockElementBehavior, false);
            }
        }

        public static void SubmitElement(BaseCharacterBehavior levelElementBehavior, ElementPosition elementPosition)
        {
            LevelElement.Type elementType = levelElementBehavior.LevelElement.ElementType;

            levelElementBehavior.MarkAsSubmitted();

            levelMap[elementPosition.X, elementPosition.Y] = LevelElement.Type.Empty;

            List<LevelElementBehavior> neighborBehaviors = GetNeighborBehaviours(elementPosition);
            if (!neighborBehaviors.IsNullOrEmpty())
            {
                for (int i = 0; i < neighborBehaviors.Count; i++)
                {
                    neighborBehaviors[i].OnNeighborPicked(elementPosition, elementType);
                }
            }

            OnMapChanged();
            levelTutorial.OnElementSubmitted(levelElementBehavior, elementPosition);
        }

        public static void RemoveUnplayableElement(LevelElementBehavior element)
        {
            levelElements.Remove(element);

            if (MapUtils.ValidatePos(element.ElementPosition))
            {
                levelMap[element.ElementPosition] = LevelElement.Type.Empty;

                OnMapChanged();
            }
        }

        public static void PlaceElementOnMap(LevelElementBehavior levelElementBehavior, ElementPosition elementPosition)
        {
            if (elementPosition.X >= levelWidth || elementPosition.Y >= levelHeight)
            {
                Debug.LogError(string.Format("Object's ({0} {1}) position is out of the range of the level.", levelElementBehavior.LevelElement.ElementType, elementPosition));

                return;
            }

            levelMap[elementPosition.X, elementPosition.Y] = levelElementBehavior.LevelElement.ElementType;

            levelElements.Add(levelElementBehavior);

            OnMapChanged();
        }

        public static LevelElementBehavior SpawnLevelObject(LevelElement.Type levelElementType, ElementPosition elementPosition, bool overrideExisting, bool recalculateMap)
        {
            return SpawnLevelObject(new ElementData(levelElementType, elementPosition, LevelData.SpecialEffectType.None), overrideExisting, recalculateMap);
        }

        public static LevelElementBehavior SpawnLevelObject(ElementData objectData, bool overrideExisting, bool recalculateMap)
        {
            ElementPosition objectPosition = objectData.ElementPosition;

            if (objectPosition.X >= levelWidth || objectPosition.Y >= levelHeight)
            {
                Debug.LogError(string.Format("Object's ({0} {1}) position is out of the range of the level.", objectData.ElementType, objectPosition));

                return null;
            }

            if (overrideExisting && levelMap[objectPosition.X, objectPosition.Y] != LevelElement.Type.Empty)
            {
                // TODO: Add override logic
            }

            LevelElement levelElement = null;
            LevelElementBehavior elementBehavior = null;

            if (levelElementsLink.ContainsKey(objectData.ElementType))
                levelElement = levelElementsLink[objectData.ElementType];

            levelMap[objectPosition.X, objectPosition.Y] = objectData.ElementType;

            if (levelElement != null)
            {
                GameObject elementObject = levelElement.Pool.GetPooledObject();
                elementObject.transform.position = GetPosition(objectPosition);
                elementObject.transform.rotation = Quaternion.Euler(0, 0, 0);

                elementBehavior = elementObject.GetComponent<LevelElementBehavior>();
                elementBehavior.Initialise(levelElement, objectPosition);

                object extraData = loadedLevelData.GetExtraData(objectPosition, levelElement);
                if (extraData != null)
                {
                    elementBehavior.SetExtraData(extraData);
                }

                if (objectData.SpecialEffectType != LevelData.SpecialEffectType.None)
                {
                    ElementSpecialEffect specialEffectPrefab = GetSpecialEffect(objectData.SpecialEffectType);
                    if (specialEffectPrefab != null)
                    {
                        specialEffectPrefab.ApplyEffect(elementBehavior);
                    }
                }

                levelElements.Add(elementBehavior);
            }

            if (recalculateMap)
                OnMapChanged();

            return elementBehavior;
        }

        public static LevelElementBehavior SpawnLevelObject(LevelElement.Type levelElementType, Vector3 position)
        {
            LevelElement levelElement = null;
            LevelElementBehavior elementBehavior = null;

            if (levelElementsLink.ContainsKey(levelElementType))
                levelElement = levelElementsLink[levelElementType];

            if (levelElement != null)
            {
                GameObject elementObject = levelElement.Pool.GetPooledObject();
                elementObject.transform.position = position;
                elementObject.transform.rotation = Quaternion.Euler(0, 0, 0);

                elementBehavior = elementObject.GetComponent<LevelElementBehavior>();
                elementBehavior.Initialise(levelElement, new ElementPosition(-1, -1));

                levelElements.Add(elementBehavior);
            }

            return elementBehavior;
        }

        public void UnloadStage()
        {
            if (!isStageLoaded)
                return;
            
            isStageLoaded = false;

            for (int i = 0; i < levelElements.Count; i++)
            {
                levelElements[i].Unload();
                levelElements[i].DisableEffect();
                levelElements[i].Unhighlight();
                levelElements[i].gameObject.SetActive(false);
            }

            highlightedElements.Clear();
            levelElements.Clear();

            Dock.DisposeQuickly();

            EnvironmentBehavior.UnloadSpawnedBusses();

            PUController.ResetBehaviors();
        }

        public static void OnMatchComplete()
        {
            instance.OnMatchCompleted();
        }

        public void OnMatchCompleted()
        {
            Vibration.Vibrate(VibrationIntensity.Light);

            AudioController.PlaySound(AudioController.Sounds.matchSound);

            if (!Dock.IsEmpty)
                return;

            bool isActiveObjectExists = false;
            for (int i = 0; i < levelElements.Count; i++)
            {
                if (levelElements[i].IsPlayableElement())
                {
                    isActiveObjectExists = true;

                    break;
                }
            }

            /*if (!isActiveObjectExists)
            {
                GameController.WinGame();
            }*/
        }

        private void LateUpdate()
        {
            Dock.LateUpdate();
        }

        public void OnSlotsFilled()
        {
            Vibration.Vibrate(VibrationIntensity.Hard);

            GameController.LoseGame();
        }

        public static ElementSpecialEffect GetSpecialEffect(LevelData.SpecialEffectType specialEffectType)
        {
            if (levelEffectsLink.ContainsKey(specialEffectType))
                return levelEffectsLink[specialEffectType];

            return null;
        }

        public static LevelElementBehavior GetLevelElement(ElementPosition elementPosition)
        {
            for (int i = 0; i < levelElements.Count; i++)
            {
                if (!levelElements[i].IsSubmitted && levelElements[i].ElementPosition.Equals(elementPosition))
                {
                    return levelElements[i];
                }
            }

            return null;
        }

        public static List<LevelElementBehavior> GetLevelElementsByType(LevelElement.Type elementType)
        {
            List<LevelElementBehavior> elements = new List<LevelElementBehavior>();
            for(int i = 0; i < levelElements.Count; i++)
            {
                if (!levelElements[i].IsSubmitted)
                {
                    if (levelElements[i].LevelElement.ElementType == elementType)
                    {
                        elements.Add(levelElements[i]);
                    }
                }
            }

            return elements;
        }

        public static List<LevelElementBehavior> GetActiveBlocks(bool ignoreSpecialEffects)
        {
            List<LevelElementBehavior> elements = new List<LevelElementBehavior>();
            for (int i = 0; i < levelElements.Count; i++)
            {
                if(!levelElements[i].IsSubmitted)
                {
                    if (LevelElement.IsCharacterElement(levelElements[i].LevelElement.ElementType))
                    {
                        if (ignoreSpecialEffects)
                        {
                            if (levelElements[i].SpecialEffect == null)
                            {
                                elements.Add(levelElements[i]);
                            }
                        }
                        else
                        {
                            elements.Add(levelElements[i]);
                        }
                    }
                }
            }

            return elements;
        }

        public static List<LevelElementBehavior> GetNeighborBehaviours(ElementPosition elementPosition)
        {
            List<LevelElementBehavior> levelElements = new List<LevelElementBehavior>();

            ElementPosition[] neighbors = elementPosition.GetNeighbors();
            for (int i = 0; i < neighbors.Length; i++)
            {
                if (MapUtils.ValidatePos(neighbors[i]))
                {
                    LevelElementBehavior tempLevelBehavior = GetLevelElement(neighbors[i]);
                    if (tempLevelBehavior != null)
                        levelElements.Add(tempLevelBehavior);
                }
            }

            return levelElements;
        }

        public static bool IsNeighbor(ElementPosition basePosition, ElementPosition neighborPosition)
        {
            ElementPosition[] neighbors = basePosition.GetNeighbors();
            for (int i = 0; i < neighbors.Length; i++)
            {
                if (neighbors[i].Equals(neighborPosition))
                    return true;
            }

            return false;
        }

        public static bool HasEmptyNeighbor(ElementPosition basePosition)
        {
            ElementPosition[] neighbors = basePosition.GetNeighbors();
            for (int i = 0; i < neighbors.Length; i++)
            {
                if (neighbors[i].X >= 0 && neighbors[i].X < levelWidth && neighbors[i].Y >= 0 && neighbors[i].Y < levelHeight)
                {
                    if (levelMap[neighbors[i].X, neighbors[i].Y] == LevelElement.Type.Empty)
                        return true;
                }
            }

            return false;
        }

        public static LevelElement.Type GetElementType(ElementPosition elementPosition)
        {
            return levelMap[elementPosition.X, elementPosition.Y];
        }

        public static Vector3 GetPosition(ElementPosition elementPosition)
        {
            return GetPosition(elementPosition.X, elementPosition.Y);
        }

        public static Vector3 GetPosition(int x, int y)
        {
            float halfSize = instance.levelElementSize / 2;

            float offsetX = levelWidth * halfSize - halfSize;

            return new Vector3(x * instance.levelElementSize - offsetX, PosY, StartZ - halfSize - y * instance.levelElementSize);
        }

        public static Bounds GetBounds(int levelWidth, int levelHeight)
        {
            return new Bounds(new Vector3(0, PosY, Mathf.Lerp(StartZ, StartZ - levelHeight * instance.levelElementSize, 0.5f)), new Vector3(levelWidth * instance.levelElementSize, levelHeight * instance.levelElementSize, 1));
        }
    }
}
