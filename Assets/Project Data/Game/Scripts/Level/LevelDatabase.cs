using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Watermelon.BusStop
{
    [CreateAssetMenu(fileName = "Level Database", menuName = "Content/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField]public LevelData[] levels;
        public LevelData[] Levels => levels;

        [SerializeField] LevelElement[] levelElements;
        public LevelElement[] LevelElements => levelElements;

        [SerializeField] ElementSpecialEffect[] specialEffects;
        public ElementSpecialEffect[] SpecialEffects => specialEffects;

        [SerializeField] EditorTypeColor[] editorTypeColors;
        [SerializeField] Texture2D editorSpawnerTexure;

        public void Initialise()
        {
            for(int i = 0; i < levelElements.Length; i++)
            {
                levelElements[i].Initialise();
            }
        }

        public void Unload()
        {
            for (int i = 0; i < levelElements.Length; i++)
            {
                levelElements[i].Unload();
            }
        }

        public int GetRandomLevelIndex(int displayLevelNumber, int lastPlayedLevelNumber, bool replayingLevel)
        {
            if (levels.IsInRange(displayLevelNumber))
            {
                return displayLevelNumber;
            }

            if(replayingLevel)
            {
                return lastPlayedLevelNumber;
            }

            int randomLevelIndex;

            do
            {
                randomLevelIndex = Random.Range(0, levels.Length);
            }
            while (!levels[randomLevelIndex].UseInRandomizer && randomLevelIndex != lastPlayedLevelNumber);

            return randomLevelIndex;
        }

        public LevelData GetLevel(int levelIndex)
        {
            levelIndex = Mathf.Clamp(levelIndex, 0, levels.Length - 1);

            return levels[levelIndex];
        }

        [System.Serializable]
        public class EditorTypeColor
        {
            [SerializeField] LevelElement.Type elementType;
            [SerializeField] Color editorColor;
        }

        #region Development 

#if UNITY_EDITOR

        // you can use this method to apply any changes to all levels at once
        [Button]
        public void EditorDevAction()
        {
            int largestHeight = 0;
            int level = -1;

            for (int i = 0; i < levels.Length; i++)
            {
                // inject value
                //ReflectionUtils.InjectInstanceComponent(levels[i], "coinsReward", 20);

                if(levels[i].Height > largestHeight)
                {
                    largestHeight = levels[i].Height;
                    level = i;
                }


                // mark as dirty - so changes will be visible and saved
                EditorUtility.SetDirty(levels[i]);
            }
                Debug.Log("lar: " + largestHeight + " " + level);
        }

#endif
#endregion
    }
}
