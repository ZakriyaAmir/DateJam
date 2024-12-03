using UnityEngine;

namespace Watermelon.BusStop
{
    [System.Serializable]
    public class LevelData : ScriptableObject
    {
        public const float BLOCK_Z_OFFSET = 3.0f;
        public const float BLOCK_PICK_Z_OFFSET = 1.0f;
        public const float BLOCK_OVERLAY_Z_OFFSET = -0.1f;

        [SerializeField] ElementData[] elementsData;
        public ElementData[] ElementsData => elementsData;

        [SerializeField] SpawnerData[] spawnerData;
        public SpawnerData[] SpawnerData => spawnerData;

        [SerializeField] TutorialStep[] tutorialSteps;
        public TutorialStep[] TutorialSteps => tutorialSteps;

        [SerializeField] LevelElement.Type[] busSpawnQueue;
        public LevelElement.Type[] BusSpawnQueue => busSpawnQueue;

        [SerializeField] int width;
        public int Width => width;

        [SerializeField] int height;
        public int Height => height;

        [SerializeField] int coinsReward = 20;
        public int CoinsReward => coinsReward;

        [SerializeField] bool useInRandomizer = true;
        public bool UseInRandomizer => useInRandomizer;

        [SerializeField] string note;

        public object GetExtraData(ElementPosition objectPosition, LevelElement levelElement)
        {
            if (levelElement.ElementType == LevelElement.Type.Spawner)
            {
                for (int i = 0; i < spawnerData.Length; i++)
                {
                    if (spawnerData[i].ElementPosition.Equals(objectPosition))
                        return spawnerData[i];
                }
            }

            return null;
        }

        public enum SpecialEffectType
        {
            None = 0,
            Hidden = 1,
        }

        public int GetBlocksAmount()
        {
            int blocksCount = 0;

            if (elementsData == null)
            {
                return 0;
            }


            for (int i = 0; i < elementsData.Length; i++)
            {
                if (IsBlock(elementsData[i].ElementType))
                {
                    blocksCount++;
                }
            }

            return blocksCount;
        }

        public int GetSpawnersAmount()
        {
            int spawnersAmount = 0;

            if (elementsData == null)
            {
                return 0;
            }

            for (int i = 0; i < elementsData.Length; i++)
            {
                if (elementsData[i].ElementType == LevelElement.Type.Spawner)
                {
                    spawnersAmount++;
                }
            }

            return spawnersAmount;
        }

        public int GetCratesAmount()
        {
            int cratesCount = 0;

            if (elementsData == null)
            {
                return 0;
            }

            for (int i = 0; i < elementsData.Length; i++)
            {
                if (IsBlock(elementsData[i].ElementType) && elementsData[i].SpecialEffectType == SpecialEffectType.Hidden)
                {
                    cratesCount++;
                }
            }

            return cratesCount;
        }

        public bool IsBlock(LevelElement.Type element)
        {
            if (element == LevelElement.Type.Block_Blue ||
                element == LevelElement.Type.Block_Green ||
                element == LevelElement.Type.Block_Pink ||
                element == LevelElement.Type.Block_Purple ||
                element == LevelElement.Type.Block_Red ||
                element == LevelElement.Type.Block_Teal ||
                element == LevelElement.Type.Block_Yellow)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
