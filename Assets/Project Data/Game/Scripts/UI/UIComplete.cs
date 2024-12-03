using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;
using Watermelon.BusStop;

namespace Watermelon
{
    public class UIComplete : UIPage
    {
        [SerializeField] RectTransform safeZone;
        [SerializeField] UIFadeAnimation backgroundFade;

        [Space]
        [SerializeField] UIScaleAnimation levelCompleteLabel;

        [Space]
        [SerializeField] UIScaleAnimation rewardLabel;
        [SerializeField] Image rewardIconImage;
        [SerializeField] TextMeshProUGUI rewardAmountText;

        [Header("Coins Label")]
        [SerializeField] UIScaleAnimation coinsPanelScalable;
        [SerializeField] CurrencyUIPanelSimple coinsPanelUI;

        [Space]
        [SerializeField] UIFadeAnimation multiplyRewardButtonFade;
        [SerializeField] Button multiplyRewardButton;
        [SerializeField] UIFadeAnimation noThanksButtonFade;
        [SerializeField] Button noThanksButton;
        [SerializeField] TMP_Text noThanksText;

        private TweenCase noThanksAppearTween;
        private int coinsHash = FloatingCloud.StringToHash(CurrencyType.Coins.ToString());

        private readonly string NO_THANKS_TEXT = "NO, THANKS";
        private readonly string CONTINUE_TEXT = "CONTINUE";

        private int currentReward;

        public override void Initialise()
        {
            multiplyRewardButton.onClick.AddListener(MultiplyRewardButton);
            noThanksButton.onClick.AddListener(NoThanksButton);

            coinsPanelUI.Initialise();

            Currency currency = CurrenciesController.GetCurrency(CurrencyType.Coins);
            rewardIconImage.sprite = currency.Icon;

            NotchSaveArea.RegisterRectTransform(safeZone);
        }

        #region Show/Hide
        public override void PlayShowAnimation()
        {
            rewardLabel.Hide(immediately: true);
            multiplyRewardButtonFade.Hide(immediately: true);
            noThanksButtonFade.Hide(immediately: true);
            noThanksButton.interactable = false;
            coinsPanelScalable.Hide(immediately: true);

            noThanksText.text = NO_THANKS_TEXT;

            backgroundFade.Show(duration: 0.3f);
            levelCompleteLabel.Show();

            coinsPanelScalable.Show();

            currentReward = LevelController.CurrentReward;

            ShowRewardLabel(currentReward, false, 0.3f, delegate // update reward here
            {
                rewardLabel.RectTransform.DOPushScale(Vector3.one * 1.1f, Vector3.one, 0.2f, 0.2f).OnComplete(delegate
                {
                    FloatingCloud.SpawnCurrency(coinsHash, rewardLabel.RectTransform, coinsPanelScalable.RectTransform, 10, "", () =>
                    {
                        CurrenciesController.Add(CurrencyType.Coins, currentReward);

                        multiplyRewardButtonFade.Show();
                        multiplyRewardButton.interactable = true;

                        noThanksAppearTween = Tween.DelayedCall(0.5f, delegate
                        {
                            noThanksButtonFade.Show();
                            noThanksButton.interactable = true;
                        });
                    });
                });
            });
        }

        public override void PlayHideAnimation()
        {
            if (!isPageDisplayed)
                return;

            backgroundFade.Hide(0.25f);
            coinsPanelScalable.Hide();

            Tween.DelayedCall(0.25f, delegate
            {
                canvas.enabled = false;
                isPageDisplayed = false;

                UIController.OnPageClosed(this);
            });
        }


        #endregion

        #region RewardLabel

        public void ShowRewardLabel(float rewardAmounts, bool immediately = false, float duration = 0.3f, Action onComplted = null)
        {
            rewardLabel.Show(immediately: immediately);

            if (immediately)
            {
                rewardAmountText.text = "+" + rewardAmounts;
                onComplted?.Invoke();

                return;
            }

            rewardAmountText.text = "+" + 0;

            Tween.DoFloat(0, rewardAmounts, duration, (float value) =>
            {
                rewardAmountText.text = "+" + (int)value;
            }).OnComplete(delegate
            {

                onComplted?.Invoke();
            });
        }

        #endregion

        #region Buttons

        public void MultiplyRewardButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (noThanksAppearTween != null && noThanksAppearTween.IsActive)
            {
                noThanksAppearTween.Kill();
            }

            noThanksButton.interactable = false;
            multiplyRewardButton.interactable = false;

            AdsManager.ShowRewardBasedVideo((bool success) =>
            {
                if (success)
                {
                    int rewardMult = 3;

                    noThanksButtonFade.Hide(immediately: true);
                    multiplyRewardButtonFade.Hide(immediately: true);

                    ShowRewardLabel(currentReward * rewardMult, false, 0.3f, delegate
                    {
                        FloatingCloud.SpawnCurrency(coinsHash, rewardLabel.RectTransform, coinsPanelScalable.RectTransform, 10, "", () =>
                        {
                            CurrenciesController.Add(CurrencyType.Coins, currentReward * rewardMult);

                            noThanksText.text = CONTINUE_TEXT;

                            noThanksButton.interactable = true;
                            noThanksButton.gameObject.SetActive(true);
                            noThanksButtonFade.Show();
                        });
                    });

                    LivesManager.AddLife();
                }
                else
                {
                    NoThanksButton();
                }
            });
        }

        public void NoThanksButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            UIController.HidePage<UIComplete>();

            GameController.LoadNextLevel();

            LivesManager.AddLife();
        }

        public void HomeButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }

        #endregion
    }
}
