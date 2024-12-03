using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.Map
{
    public class MapChunkBehavior : MonoBehaviour
    {
        [SerializeField] SpriteRenderer background;
        [SerializeField] GameObject bottom;

        [SerializeField] List<MapLevelBehavior> levels;
        public int LevelsCount => levels.Count;

        public int ChunkId { get; private set; }

        public MapBehavior Map { get; private set; }
        public float Height => background.size.y * transform.localScale.y;
        public float AdjustedHeight => Height / Map.MapVisibleRectHeight;

        public float CurrentLevelPosition { get; private set; }
        public float Position { get; private set; }
        public int StartLevelCount { get; private set; }

        public void SetPosition(float y)
        {
            Position = y;
            transform.SetPositionY(y * Map.MapVisibleRectHeight + Height / 2 + Camera.main.transform.position.y - Map.MapVisibleRectHeight / 2);
        }

        public void SetMap(MapBehavior map)
        {
            Map = map;
        }

        public void Init(int chunkId, int startLevelCount)
        {
            ChunkId = chunkId;
            StartLevelCount = startLevelCount;

            CurrentLevelPosition = -1;

            transform.localScale = Vector3.one * Map.MapVisibleRectWidth / background.size.x;

            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                var levelId = startLevelCount + i;

                level.Init(levelId);

                if(levelId == MapBehavior.MaxLevelReached)
                {
                    CurrentLevelPosition = (level.transform.position.y + Height / 2) / Map.MapVisibleRectHeight;
                }
            }

            background.receiveShadows = true;

            if(bottom != null) bottom.SetActive(startLevelCount == 0);
        }

        private void CalculateNarrowScreenScale()
        {
            transform.localScale = Vector3.one * Map.MapVisibleRectWidth / background.size.x;
        }

        private void CalculateWideScreenScale()
        {

        }
    }
}

