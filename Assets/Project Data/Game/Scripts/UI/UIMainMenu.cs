using TMPro;
using UnityEngine;
using Watermelon.BusStop;
using Watermelon.IAPStore;
using Watermelon.SkinStore;

namespace Watermelon
{
    public class UIMainMenu : UIPage
    {
        public readonly float STORE_AD_RIGHT_OFFSET_X = 300F;

        [Space]
        [SerializeField] RectTransform safeZoneTransform;
        [SerializeField] UIScaleAnimation coinsLabelScalable;
        [SerializeField] CurrencyUIPanelSimple coinsPanel;
        [SerializeField] TextMeshProUGUI levelText;
        private UIScaleAnimation levelTextScaleAnimation;

        [Space]
        [SerializeField] UIMainMenuButton iapStoreButton;
        [SerializeField] UIMainMenuButton noAdsButton;
        [SerializeField] UIMainMenuButton skinsButton;

        [Space]
        [SerializeField] UINoAdsPopUp noAdsPopUp;
        public UINoAdsPopUp NoAdsPopUp => noAdsPopUp;

        [Space]
        [SerializeField] AddLivesPanel livesPanel;

        private TweenCase showHideStoreAdButtonDelayTweenCase;
        private TweenCase hideTween;

        private void OnEnable()
        {
            AdsManager.ForcedAdDisabled += ForceAdPurchased;
        }

        private void OnDisable()
        {
            AdsManager.ForcedAdDisabled -= ForceAdPurchased;
        }

        public override void Initialise()
        {
            levelTextScaleAnimation = new UIScaleAnimation(levelText.rectTransform);

            coinsPanel.Initialise();
            coinsPanel.AddButton.onClick.AddListener(IAPStoreButton);

            iapStoreButton.Init(STORE_AD_RIGHT_OFFSET_X);
            noAdsButton.Init(STORE_AD_RIGHT_OFFSET_X);
            skinsButton.Init(STORE_AD_RIGHT_OFFSET_X);

            iapStoreButton.Button.onClick.AddListener(IAPStoreButton);
            noAdsButton.Button.onClick.AddListener(NoAdButton);
            skinsButton.Button.onClick.AddListener(SkinsButton);

            noAdsPopUp.Initialise();

            NotchSaveArea.RegisterRectTransform(safeZoneTransform);
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            UpdateLevelNumber();

            showHideStoreAdButtonDelayTweenCase.KillActive();
            hideTween.KillActive();

            HideAdButton(true);
            iapStoreButton.Hide(true);
            skinsButton.Hide(true);

            levelTextScaleAnimation.Show(scaleMultiplier: 1.05f, immediately: false);
            coinsLabelScalable.Show(immediately: true);

            showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.05f, delegate
            {
                ShowAdButton();
                iapStoreButton.Show();
                skinsButton.Show();
            });

            SettingsPanel.ShowPanel();

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            showHideStoreAdButtonDelayTweenCase.KillActive();
            hideTween.KillActive();

            coinsLabelScalable.Hide(immediately: true);
            levelTextScaleAnimation.Hide(scaleMultiplier: 1.05f, immediately: true);

            HideAdButton();

            showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.1f, delegate
            {
                iapStoreButton.Hide();
                skinsButton.Hide();
            });

            SettingsPanel.HidePanel();

            hideTween = Tween.DelayedCall(0.5f, delegate
            {
                UIController.OnPageClosed(this);
            });
        }

        public void ShowAddLivesPanel()
        {
            livesPanel.Show();
        }

        private void UpdateLevelNumber()
        {
            levelText.text = string.Format("LEVEL {0}", LevelController.DisplayLevelNumber + 1);
        }

        #endregion

        #region Ad Button Label

        private void ShowAdButton(bool immediately = false)
        {
            if (AdsManager.IsForcedAdEnabled())
            {
                noAdsButton.Show(immediately);
            }
            else
            {
                noAdsButton.Hide(immediately: true);
            }
        }

        private void HideAdButton(bool immediately = false)
        {
            noAdsButton.Hide(immediately);
        }

        private void ForceAdPurchased()
        {
            HideAdButton(immediately: true);
        }

        #endregion

        #region Buttons

        public void TapToPlayButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            GameController.StartGame();
        }

        public void IAPStoreButton()
        {
            if (UIController.GetPage<UIIAPStore>().IsPageDisplayed)
                return;

            UIController.HidePage<UIMainMenu>();
            UIController.ShowPage<UIIAPStore>();

            // reopening main menu only after store page was opened throug main menu
            UIController.OnPageClosedEvent += OnIapOrSkinsStoreClosed;


            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }

        public void SkinsButton()
        {
            if (UIController.GetPage<UISkinStore>().IsPageDisplayed)
                return;

            UIController.HidePage<UIMainMenu>();
            SkinStoreController.OpenStore();

            // reopening main menu only after store page was opened throug main menu
            UIController.OnPageClosedEvent += OnIapOrSkinsStoreClosed;

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }

        private void OnIapOrSkinsStoreClosed(UIPage page, System.Type pageType)
        {
            if (pageType.Equals(typeof(UIIAPStore)) || pageType.Equals(typeof(UISkinStore)))
            {
                UIController.OnPageClosedEvent -= OnIapOrSkinsStoreClosed;

                UIController.ShowPage<UIMainMenu>();
            }
        }

        public void NoAdButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            noAdsPopUp.Show();
        }

        #endregion
    }


}
