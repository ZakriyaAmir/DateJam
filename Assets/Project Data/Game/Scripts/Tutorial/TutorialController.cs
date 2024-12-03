using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class TutorialController : MonoBehaviour
    {
        private static TutorialController tutorialController;
        private static List<ITutorial> registeredTutorials = new List<ITutorial>();

        [SerializeField] TutorialCanvasController tutorialCanvasController;

        private static bool isTutorialSkipped;

        public void Initialise()
        {
            tutorialController = this;

            isTutorialSkipped = TutorialHelper.IsTutorialSkipped();

            tutorialCanvasController.Initialise();
        }

        public static ITutorial GetTutorial(TutorialID tutorialID)
        {
            for(int i = 0; i < registeredTutorials.Count; i++)
            {
                if (registeredTutorials[i].TutorialID == tutorialID)
                {
                    if (!registeredTutorials[i].IsInitialised)
                        registeredTutorials[i].Initialise();

                    if (isTutorialSkipped)
                        registeredTutorials[i].FinishTutorial();

                    return registeredTutorials[i];
                }
            }

            return null;
        }

        public static void ActivateTutorial(ITutorial tutorial)
        {
            if (!tutorial.IsInitialised)
                tutorial.Initialise();

            if (isTutorialSkipped)
                tutorial.FinishTutorial();
        }

        public static void RegisterTutorial(ITutorial tutorial)
        {
            if (registeredTutorials.FindIndex(x => x == tutorial) != -1)
                return;

            // Add tutorial to list
            registeredTutorials.Add(tutorial);
        }

        public static void RemoveTutorial(ITutorial tutorial)
        {
            int tutorialIndex = registeredTutorials.FindIndex(x => x == tutorial);
            if (tutorialIndex != -1)
            {
                // Remove tutorial from list
                registeredTutorials.RemoveAt(tutorialIndex);
            }
        }
    }
}