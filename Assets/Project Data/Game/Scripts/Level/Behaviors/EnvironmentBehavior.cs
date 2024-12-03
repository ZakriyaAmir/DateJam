using System.Collections.Generic;
using UnityEngine;
using Watermelon.BusStop;
using Watermelon.SkinStore;

namespace Watermelon
{
    public class EnvironmentBehavior : MonoBehaviour
    {
        [SerializeField] TileData environmentTileData;
        public TileData EnvironmentTileData => environmentTileData;

        [SerializeField] DockBehavior dock;
        public DockBehavior Dock => dock;

        [SerializeField] Transform busSpawnPos;
        [SerializeField] Transform busWaitPos;
        [SerializeField] Transform busCollectPos;
        [SerializeField] Transform busExitPos;

        public Vector3 BusSpawnPos => busSpawnPos.position;
        public Vector3 BusWaitPos => busWaitPos.position;
        public Vector3 BusCollectPos => busCollectPos.position;
        public Vector3 BusExitPos => busExitPos.position;

        private static Dictionary<LevelElement.Type, PoolGeneric<MaleBehavior>> busTypesPoolsDictionary;

        private static List<LevelElement.Type> busTypeQueue = new List<LevelElement.Type>();

        public static MaleBehavior WaitingBus { get; private set; }
        public static MaleBehavior CollectingBus { get; private set; }

        public static bool IsWaitingPlaceAvailable => WaitingBus == null;
        public static bool IsCollectingPlaceAvailable => CollectingBus == null;

        private static TweenCase secondBusSpawnCase;

        #region Initialisation 

        /// <summary>
        /// Creates pools and subscribes to events. Should be called once at the beggining of game session
        /// </summary>
        public static void InitialiseEnvironment()
        {
            if (GameController.Data.ActivateVehicles)
            {
                PopulateBusTypePoolDictionary();

                SkinStoreController.OnProductSelected += OnSkinsStoreProductChanged;
            }
        }

        public static void SetBusQueue(LevelElement.Type[] busQueue)
        {
            busTypeQueue = new List<LevelElement.Type>(busQueue);
        }

        public static void OnSkinsStoreProductChanged(SkinTab tab, SkinData product)
        {
            if (tab == SkinTab.Bus)
            {
                UnloadSpawnedBusses();

                foreach (var pool in busTypesPoolsDictionary.Values) pool.Clear();

                PopulateBusTypePoolDictionary();
            }

            busTypeQueue = new List<LevelElement.Type>(LevelController.LoadedStageData.BusSpawnQueue);

            SpawnNextBusFromQueue();
            secondBusSpawnCase = Tween.DelayedCall(0.9f, SpawnNextBusFromQueue);
        }

        private static void PopulateBusTypePoolDictionary()
        {
            busTypesPoolsDictionary = new Dictionary<LevelElement.Type, PoolGeneric<MaleBehavior>>();

            var selectedBusData = SkinStoreController.GetSelectedPrefab(SkinTab.Bus).GetComponent<BusData>();

            for (int i = 0; i < selectedBusData.busData.Count; i++)
            {
                var pool = new PoolGeneric<MaleBehavior>(new PoolSettings(selectedBusData.busData[i].prefab, 2, true));

                busTypesPoolsDictionary.Add(selectedBusData.busData[i].type, pool);
            }
        }

        #endregion

        #region Gameplay

        public static void StartSpawningBusses()
        {
            if (!GameController.Data.ActivateVehicles) return;

            SpawnNextBusFromQueue();
            secondBusSpawnCase = Tween.DelayedCall(0.9f, SpawnNextBusFromQueue);
        }

        public static void SpawnNextBusFromQueue()
        {
            if (!GameController.Data.ActivateVehicles) return;

            if (busTypeQueue.Count > 0)
            {
                var type = busTypeQueue[0];
                busTypeQueue.RemoveAt(0);

                //busTypesPoolsDictionary[type].GetPooledComponent().SetType(type);

            }
        }

        public static void AssignWaitingBus(MaleBehavior bus)
        {
            WaitingBus = bus;
        }

        public static void AssignCollectingBus(MaleBehavior bus)
        {
            CollectingBus = bus;
        }

        /// <summary>
        /// Aslo spawns next bus if there are busses in the queue left
        /// </summary>
        public static void RemoveWaitingBus()
        {
            WaitingBus = null;

            if(busTypeQueue.Count > 0) SpawnNextBusFromQueue();
        }

        public static void RemoveCollectingBus()
        {
            CollectingBus = null;
        }

        #endregion

        #region Cleanup

        public static void UnloadSpawnedBusses()
        {
            if (!GameController.Data.ActivateVehicles) return;

            if (WaitingBus != null) WaitingBus.Clear();
            if (CollectingBus != null) CollectingBus.Clear();

            WaitingBus = null;
            CollectingBus = null;

            busTypesPoolsDictionary.ForEachValue((pool) => pool.ReturnToPoolEverything());

            secondBusSpawnCase.KillActive();
        }

        #endregion
    }
}