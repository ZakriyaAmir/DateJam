#pragma warning disable 0067

using UnityEngine;
using Watermelon.IAPStore;
using Watermelon.SkinStore;

namespace Watermelon.BusStop
{
    public class RaycastController : MonoBehaviour
    {
        private UIIAPStore iapStorePage;
        private UIMainMenu mainMenuPage;
        private UISkinStore storePage;

        private static bool isActive;

        public static event SimpleCallback OnInputActivated;
        public static event SimpleCallback OnMovementInputActivated;

        public void Initialise()
        {
            isActive = true;

            iapStorePage = UIController.GetPage<UIIAPStore>();
            mainMenuPage = UIController.GetPage<UIMainMenu>();
            storePage = UIController.GetPage<UISkinStore>();
        }

        private void Update()
        {
            if (!isActive) return;

            if (Input.GetMouseButtonDown(0) && !IsRaycastBlockedByUI())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    IClickableObject clickableObject = hit.transform.GetComponent<IClickableObject>();
                    if (clickableObject != null)
                    {
                        if (!GameController.IsGameActive)
                        {
                            if(LivesManager.Lives > 0)
                            {
                                GameController.StartGame();
                                clickableObject.OnObjectClicked();
                            } else 
                            {
                                mainMenuPage.ShowAddLivesPanel();
                            }

                            
                        } else
                        {
                            clickableObject.OnObjectClicked();
                        }
                    }
                }
            }
        }

        private bool IsRaycastBlockedByUI()
        {
            return mainMenuPage.NoAdsPopUp.IsOpened || iapStorePage.IsPageDisplayed || storePage.IsPageDisplayed|| AddLivesPanel.IsPanelOpened;
        }

        public void ResetControl()
        {

        }

        public static void Enable()
        {
            isActive = true;
            OnInputActivated?.Invoke();
        }

        public static void Disable()
        {
            isActive = false;
        }
    }
}
