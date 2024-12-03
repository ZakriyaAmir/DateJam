using UnityEngine;
using Watermelon.BusStop;
using System.Collections.Generic;

namespace Watermelon
{
    public class PUShuffleBehavior : PUBehavior
    {
        private PUShuffleSettings customSettings;

        private TweenCase delayTweenCase;

        public override void Initialise()
        {
            customSettings = (PUShuffleSettings)settings;
        }

        public override bool Activate()
        {
            IsBusy = true;

            List<LevelElementBehavior> activeBlocks = LevelController.GetActiveBlocks(true);
            if(activeBlocks != null)
            {
                if(activeBlocks.Count > 1)
                {
                    ElementPosition[] shuffleElements = new ElementPosition[activeBlocks.Count];
                    for(int i = 0; i < shuffleElements.Length; i++)
                    {
                        shuffleElements[i] = activeBlocks[i].ElementPosition;
                    }

                    shuffleElements.Shuffle();

                    for(int i = 0; i < shuffleElements.Length; i++)
                    {
                        activeBlocks[i].transform.localScale = Vector3.zero;
                        activeBlocks[i].transform.position = LevelController.GetPosition(shuffleElements[i]);
                        activeBlocks[i].SetPosition(shuffleElements[i]);
                        activeBlocks[i].PlaySpawnAnimation(Random.Range(0.05f, 0.4f));
                    }

                    LevelController.OnMapChanged();

                    delayTweenCase = Tween.DelayedCall(1.0f, () =>
                    {
                        IsBusy = false;
                    });

                    return true;
                }
            }

            return false;
        }

        public override void ResetBehavior()
        {
            IsBusy = false;

            delayTweenCase.KillActive();
        }
    }
}
