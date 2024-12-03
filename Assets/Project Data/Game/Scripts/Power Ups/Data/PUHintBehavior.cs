using Watermelon.BusStop;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class PUHintBehavior : PUBehavior
    {
        private PUHintSettings customSettings;

        private TweenCase[] scaleTweenCases;
        private TweenCase disableTweenCase;

        public override void Initialise()
        {
            customSettings = (PUHintSettings)settings;
        }

        public override bool Activate()
        {
            int requiredElementsCount = 3;
            LevelElement.Type characterType = LevelElement.Type.Empty;

            if(!GameController.Data.ActivateVehicles)
            {
                List<BaseCharacterBehavior> dockCharacters = DockBehavior.GetHintCharacters();
                if (dockCharacters.IsNullOrEmpty())
                {
                    List<LevelElementBehavior> activeCharacters = LevelController.GetActiveBlocks(true);
                    if (!activeCharacters.IsNullOrEmpty())
                    {
                        characterType = activeCharacters[Random.Range(0, activeCharacters.Count - 1)].LevelElement.ElementType;
                    }
                }
                else
                {
                    characterType = dockCharacters[0].LevelElement.ElementType;

                    requiredElementsCount -= dockCharacters.Count;
                }
            }
            else
            {
                MaleBehavior busBehavior = EnvironmentBehavior.CollectingBus;
                characterType = busBehavior.Type;

                requiredElementsCount -= busBehavior.PassengersCount;
            }

            if(characterType != LevelElement.Type.Empty)
            {
                if (!GameController.Data.ActivateVehicles)
                {
                    if(DockBehavior.GetSlotsAvailable() < requiredElementsCount)
                    {
                        return false;
                    }
                }
                else
                {
                    if(!EnvironmentBehavior.CollectingBus.IsAvailableToEnter)
                    {
                        return false;
                    }
                }

                IsBusy = true;

                List<BaseCharacterBehavior> targetCharacters = new List<BaseCharacterBehavior>();

                List<LevelElementBehavior> levelElements = LevelController.GetLevelElementsByType(characterType);
                for (int i = 0; i < levelElements.Count; i++)
                {
                    targetCharacters.Add((BaseCharacterBehavior)levelElements[i]);

                    requiredElementsCount--;

                    if (requiredElementsCount <= 0)
                        break;
                }

                if (requiredElementsCount > 0)
                {
                    List<LevelElementBehavior> spawnerElements = LevelController.GetLevelElementsByType(LevelElement.Type.Spawner);
                    for (int i = 0; i < spawnerElements.Count; i++)
                    {
                        SpawnerElementBehavior spawner = (SpawnerElementBehavior)spawnerElements[i];
                        while (spawner.TryToRemoveElement(characterType))
                        {
                            BaseCharacterBehavior spawnedCharacter = (BaseCharacterBehavior)LevelController.SpawnLevelObject(characterType, Vector3.zero);
                            spawnedCharacter.transform.localScale = Vector3.zero;

                            targetCharacters.Add(spawnedCharacter);

                            requiredElementsCount--;

                            if (requiredElementsCount <= 0)
                                break;
                        }

                        if (requiredElementsCount <= 0)
                            break;
                    }
                }

                scaleTweenCases = new TweenCase[targetCharacters.Count];
                for (int i = 0; i < scaleTweenCases.Length; i++)
                {
                    BaseCharacterBehavior targetCharacter = targetCharacters[i];

                    targetCharacter.DisableEffect();
                    scaleTweenCases[i] = targetCharacter.transform.DOScale(0.0f, 0.3f).SetEasing(Ease.Type.CircOut).OnComplete(() =>
                    {
                        LevelController.RemoveUnplayableElement(targetCharacter);

                        if (!GameController.Data.ActivateVehicles)
                        {
                            LevelController.OnElementSubmittedToSlot(targetCharacter, true);
                        }
                        else
                        {
                            EnvironmentBehavior.CollectingBus.CollectInstant(targetCharacter);
                        }
                    });
                }

                disableTweenCase = Tween.DelayedCall(1.0f, () =>
                {
                    IsBusy = false;
                });

                return true;
            }

            return false;
        }

        public override void ResetBehavior()
        {
            if(!scaleTweenCases.IsNullOrEmpty())
            {
                for (int i = 0; i < scaleTweenCases.Length; i++)
                {
                    scaleTweenCases[i].KillActive();
                }
            }

            disableTweenCase.KillActive();

            IsBusy = false;
        }
    }
}
