using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.BusStop;
using Watermelon.SkinStore;
using System;

namespace Watermelon
{
    public class GameController : MonoBehaviour
    {
        private static GameController gameController;

        [DrawReference]
        [SerializeField] GameData data;
        [SerializeField] public List<GameObject> allMales;

        [SerializeField] UIController uiController;

        private ParticlesController particlesController;
        private CurrenciesController currenciesController;
        private LevelController levelController;
        private TutorialController tutorialController;
        private PUController powerUpsController;

        private static bool isGameActive;
        public static bool IsGameActive => isGameActive;

        public static event SimpleCallback OnLevelChangedEvent;
        private static LevelSave levelSave;

        public FemaleTileManager _femaleTileManager;

        public static GameData Data => gameController.data;
        public GameObject heartParticle;
        public static Action clearMales;

        private void Awake()
        {
            gameController = this;
            SaveController.Initialise(useAutoSave: false);
            levelSave = SaveController.GetSaveObject<LevelSave>("level");
            clearMales += flushAllMales;
            // Cache components
            CacheComponent(out particlesController);
            CacheComponent(out currenciesController);
            CacheComponent(out levelController);
            CacheComponent(out tutorialController);
            CacheComponent(out powerUpsController);
            
        }

        void flushAllMales() 
        {
            foreach (var m in allMales) 
            {
                if (m == null) continue;
                m.GetComponent<BaseCharacterBehavior>().isDocked = false;
                m.GetComponent<BaseCharacterBehavior>().isVIP = false;
            }
            allMales.Clear();
        }

        private void Start()
        {
            Application.targetFrameRate = 120;
            InitialiseGame();
            _femaleTileManager = FindObjectOfType<FemaleTileManager>();
        }

        public void InitialiseGame()
        {
            uiController.Initialise();

            particlesController.Initialise();
            currenciesController.Initialise();
            tutorialController.Initialise();
            powerUpsController.Initialise();
            
            uiController.InitialisePages();

            // Add raycast controller component
            RaycastController raycastController = gameObject.AddComponent<RaycastController>();
            raycastController.Initialise();

            SkinStoreController.Init();

            levelController.Initialise();

            // Display default page
            UIController.ShowPage<UIMainMenu>();

            LoadLevel(() => GameLoading.MarkAsReadyToHide());
        }

        private static void LoadLevel(System.Action OnComplete = null)
        {
            gameController.levelController.LoadLevel(() =>
            {
                OnComplete?.Invoke();
            });

            OnLevelChangedEvent?.Invoke();
        }

        public static void StartGame()
        {
            // On Level is loaded
            isGameActive = true;

            UIController.HidePage<UIMainMenu>();
            UIController.ShowPage<UIGame>();

            Tween.DelayedCall(2f, LivesManager.RemoveLife);
        }

        public static void LoseGame()
        {
            if (!isGameActive)
                return;

            isGameActive = false;

            RaycastController.Disable();

            UIController.HidePage<UIGame>();
            UIController.ShowPage<UIGameOver>();

            AudioController.PlaySound(AudioController.Sounds.failSound);

            levelSave.ReplayingLevelAgain = true;
        }

        public static void WinGame()
        {
            if (!isGameActive)
                return;

            isGameActive = false;

            RaycastController.Disable();

            levelSave.ReplayingLevelAgain = false;

            LevelData completedLevel = LevelController.LoadedStageData;

            UIController.HidePage<UIGame>();
            UIController.ShowPage<UIComplete>();

            AudioController.PlaySound(AudioController.Sounds.completeSound);
        }

        public static void LoadNextLevel()
        {
            clearMales?.Invoke();
            if (isGameActive)
                return;

            UIController.ShowPage<UIMainMenu>();

            levelSave.ReplayingLevelAgain = false;
            gameController.levelController.AdjustLevelNumber();

            AdsManager.ShowInterstitial(null);

            LoadLevel();
        }

        public void ReplayLevel()
        {
            foreach (GameObject obj in allMales)
            {
                if (obj == null) continue;
                if (obj.GetComponent<MaleBehavior>() == null) continue;
                foreach (GameObject obj2 in obj.transform.GetComponent<MaleBehavior>().passengers) 
                {
                    Debug.Log("zak4 = " + obj2.name);
                    if (obj2 == null) continue;
                    Destroy(obj2);
                }
                obj.transform.GetComponent<MaleBehavior>().passengers.Clear();
            }
            clearMales?.Invoke();
            isGameActive = false;
            
            UIController.ShowPage<UIMainMenu>();

            levelSave.ReplayingLevelAgain = true;

            AdsManager.ShowInterstitial(null);

            LoadLevel();
        }

        public static void RefreshLevelDev()
        {
            UIController.ShowPage<UIGame>();
            levelSave.ReplayingLevelAgain = true;

            LoadLevel();
        }

        private void OnApplicationQuit()
        {
            // to make sure we will load similar level next time game launched (in case we outside level bounds)
            levelSave.ReplayingLevelAgain = true;
        }

        #region Extensions
        public bool CacheComponent<T>(out T component) where T : Component
        {
            Component unboxedComponent = gameObject.GetComponent(typeof(T));

            if (unboxedComponent != null)
            {
                component = (T)unboxedComponent;

                return true;
            }

            Debug.LogError(string.Format("Scripts Holder doesn't have {0} script added to it", typeof(T)));

            component = null;

            return false;
        }
        #endregion
    }
}