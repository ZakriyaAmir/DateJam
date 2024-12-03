using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon.BusStop
{
    public class SpawnerElementBehavior : LevelElementBehavior
    {
        [SerializeField] Transform graphicsTransform;

        [SerializeField] TextMeshPro amountText;

        [SerializeField] JuicyBounce juicyBounce;

        private List<LevelElement.Type> spawnQueue;
        private int currentElementIndex;
        private ElementPosition spawnerOffset;

        private TweenCase delayTweenCase;

        public override void Initialise(LevelElement levelElement, ElementPosition elementPosition)
        {
            base.Initialise(levelElement, elementPosition);

            juicyBounce.Initialise(transform);
        }

        public override void SetExtraData(object extraData)
        {
            SpawnerData spawnerData = (SpawnerData)extraData;

            graphicsTransform.rotation = Quaternion.Euler(0, 90 + 90 * (int)spawnerData.Direction, 0);

            spawnerOffset = elementPosition + ElementPosition.GetOffset(spawnerData.Direction);

            spawnQueue = new List<LevelElement.Type>(spawnerData.SpawnQueue);
            currentElementIndex = spawnQueue.Count - 1;

            amountText.text = spawnQueue.Count.ToString();
        }

        public override void OnMapUpdated()
        {
            delayTweenCase = Tween.DelayedCall(0.1f, () =>
            {
                TryToSpawnElement();
            });
        }

        private void TryToSpawnElement()
        {
            if (currentElementIndex < 0)
                return;

            if (LevelController.GetElementType(spawnerOffset) != LevelElement.Type.Empty)
                return;

            LevelElement.Type nextElementType = spawnQueue[currentElementIndex];

            LevelElementBehavior spawnedElement = LevelController.SpawnLevelObject(nextElementType, spawnerOffset, false, true);
            spawnedElement.PlaySpawnAnimation();

            amountText.text = currentElementIndex.ToString();

            currentElementIndex--;

            juicyBounce.Bounce();
        }

        public bool TryToRemoveElement(LevelElement.Type type)
        {
            if(currentElementIndex >= 0)
            {
                for(int i = 0; i <= currentElementIndex; i++)
                {
                    if(spawnQueue[i] == type)
                    {
                        spawnQueue.RemoveAt(i);

                        amountText.text = currentElementIndex.ToString();

                        currentElementIndex--;

                        return true;
                    }
                }
            }

            return false;
        }

        public override void Unload()
        {
            delayTweenCase.KillActive();
        }

        public override bool IsPlayableElement()
        {
            return currentElementIndex > 0;
        }
    }
}
