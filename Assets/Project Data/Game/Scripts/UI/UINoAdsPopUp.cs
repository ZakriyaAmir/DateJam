using UnityEngine;
using UnityEngine.UI;
using Watermelon.IAPStore;

namespace Watermelon
{
    public class UINoAdsPopUp : MonoBehaviour
    {
        [SerializeField] UIScaleAnimation panelScalable;
        [SerializeField] UIFadeAnimation backFade;
        [SerializeField] Button bigCloseButton;
        [SerializeField] Button smallCloseButton;
        [SerializeField] IAPButton removeAdsButton;

        public bool IsOpened => gameObject.activeSelf;

        public void Initialise()
        {
            bigCloseButton.onClick.AddListener(ClosePanel);
            smallCloseButton.onClick.AddListener(ClosePanel);
            removeAdsButton.Init(ProductKeyType.NoAds);

            backFade.Hide(immediately: true);
            panelScalable.Hide(immediately: true);

            AdsManager.ForcedAdDisabled += OnForcedAdDisabled;
        }

        private void OnForcedAdDisabled()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            bigCloseButton.interactable = true;
            smallCloseButton.interactable = true;

            gameObject.SetActive(true);
            backFade.Show(0.2f, onCompleted: () =>
            {
                panelScalable.Show(immediately: false, duration: 0.3f);
            });
        }

        private void ClosePanel()
        {
            bigCloseButton.interactable = false;
            smallCloseButton.interactable = false;
            
            backFade.Hide(0.2f);
            panelScalable.Hide(immediately: false, duration: 0.4f, onCompleted: () =>
            {
                gameObject.SetActive(false);
            });
        }
    }
}