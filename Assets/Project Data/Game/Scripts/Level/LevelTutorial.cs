using UnityEngine;

namespace Watermelon.BusStop
{
    public class LevelTutorial
    {
        public int stepIndex;
        public TutorialStepCase[] steps;

        public void Initialise(TutorialStep[] tutorialSteps)
        {
            steps = new TutorialStepCase[tutorialSteps.Length];
            stepIndex = 0;

            if (!tutorialSteps.IsNullOrEmpty())
            {
                for (int i = 0; i < tutorialSteps.Length; i++)
                {
                    LevelElementBehavior levelElementBehavior = LevelController.GetLevelElement(tutorialSteps[i].ElementPosition);
                    if (levelElementBehavior != null)
                    {
                        TutorialStepCase tutorialStep = new TutorialStepCase();
                        tutorialStep.LevelElementBehavior = levelElementBehavior;

                        steps[i] = tutorialStep;
                    }
                }
            }

            if (steps.IsInRange(stepIndex))
            {
                if (steps[stepIndex].LevelElementBehavior.IsHighlighted)
                {
                    Vector3 position = steps[stepIndex].LevelElementBehavior.transform.position;
                    position.z = 1.0f;
                    position.y -= 0.4f;

                    TutorialCanvasController.ActivatePointer(position, TutorialCanvasController.POINTER_DEFAULT);

                    steps[stepIndex].IsActive = true;
                }
            }
        }

        public void Unload()
        {
            TutorialCanvasController.ResetPointer();
        }

        public void OnElementClicked(BaseCharacterBehavior levelElementBehavior, ElementPosition elementPosition)
        {
            if (!steps.IsNullOrEmpty())
            {
                for (int i = 0; i < steps.Length; i++)
                {
                    if (steps[i].LevelElementBehavior == levelElementBehavior && !steps[i].IsPicked)
                    {
                        steps[i].IsPicked = true;
                    }
                }

                if (steps.IsInRange(stepIndex))
                {
                    if (steps[stepIndex].LevelElementBehavior == levelElementBehavior && steps[stepIndex].IsActive)
                    {
                        TutorialCanvasController.ResetPointer();

                        stepIndex++;
                    }
                }
            }
        }

        public void OnElementSubmitted(BaseCharacterBehavior levelElementBehavior, ElementPosition elementPosition)
        {
            if (steps.IsInRange(stepIndex))
            {
                int stepOffset = 0;
                for (int i = stepIndex; i < steps.Length; i++)
                {
                    if (steps[stepIndex + stepOffset].LevelElementBehavior.IsHighlighted && !steps[stepIndex + stepOffset].IsPicked)
                    {
                        Vector3 position = steps[stepIndex + stepOffset].LevelElementBehavior.transform.position;
                        position.z = 1.0f;
                        position.y -= 0.4f;

                        TutorialCanvasController.ActivatePointer(position, TutorialCanvasController.POINTER_DEFAULT);

                        steps[stepIndex + stepOffset].IsActive = true;

                        stepIndex += stepOffset;

                        break;
                    }

                    stepOffset++;
                }
            }
        }
    }
}
