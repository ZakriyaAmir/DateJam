using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Prefs Settings", Core = true)]
    public class GameLoadingInitModule : InitModule
    {
        [Tooltip("If manual mode is enabled, the loading screen will be active until GameLoading.MarkAsReadyToHide method has been called.")]
        [SerializeField] bool manualControlMode;
        [SerializeField] GameObject loadingCanvasObject;

        public override void CreateComponent(Initialiser Initialiser)
        {
            if(loadingCanvasObject != null)
            {
                GameObject tempObject = Instantiate(loadingCanvasObject);
                tempObject.transform.ResetGlobal();

                LoadingGraphics loadingGraphics = tempObject.GetComponent<LoadingGraphics>();
                loadingGraphics.Initialise();
            }

            if (manualControlMode)
                GameLoading.EnableManualControlMode();
        }

        public GameLoadingInitModule()
        {
            moduleName = "Game Loading Settings";
        }
    }
}
