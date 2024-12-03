using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BusStop;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [SerializeField] RectTransform safeZoneTransform;
        [SerializeField] PUUIController powerUpsUIController;
        public PUUIController PowerUpsUIController => powerUpsUIController;

        [SerializeField] Button replayButton;
        [SerializeField] UILevelQuitPopUp exitPopUp;

        [SerializeField] TextMeshProUGUI levelText;
        private UIScaleAnimation levelTextScaleAnimation;

        [Space(5f)]
        [SerializeField] GameObject devOverlay;

        public override void Initialise()
        {
            levelTextScaleAnimation = new UIScaleAnimation(levelText.rectTransform);

            devOverlay.SetActive(DevPanelEnabler.IsDevPanelDisplayed());

            replayButton.onClick.AddListener(OnReplayButtonClicked);

            NotchSaveArea.RegisterRectTransform(safeZoneTransform);

            exitPopUp.OnCancelExitEvent += ExitPopCloseButton;
            exitPopUp.OnConfirmExitEvent += ExitPopUpConfirmExitButton;
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            UpdateLevelNumber();

            levelTextScaleAnimation.Show(scaleMultiplier: 1.05f, immediately: true);

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            levelTextScaleAnimation.Hide(scaleMultiplier: 1.05f, immediately: false);

            exitPopUp.Hide();

            UIController.OnPageClosed(this);
        }

        #endregion

        private void OnReplayButtonClicked()
        {
            exitPopUp.Show();
        }

        public void ExitPopCloseButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            exitPopUp.Hide();
        }

        public void ExitPopUpConfirmExitButton()
        {
            exitPopUp.Hide();

            if (LivesManager.IsMaxLives) LivesManager.RemoveLife();

            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            UIController.HidePage<UIGame>();

            GameController.ReplayLevel();
        }

        private void UpdateLevelNumber()
        {
            levelText.text = string.Format("LEVEL {0}", LevelController.DisplayLevelNumber + 1);
        }

        #region Development

        public void ReloadDev()
        {
            ReflectionUtils.InjectInstanceComponent<GameController>("isGameActive", false, ReflectionUtils.FLAGS_STATIC_PRIVATE);

            GameController.RefreshLevelDev();
        }

        public void HideDev()
        {
            devOverlay.SetActive(false);
        }

        public void OnLevelInputUpdatedDev(string newLevel)
        {
            int level = -1;

            if (int.TryParse(newLevel, out level))
            {
                ReflectionUtils.InjectInstanceComponent<GameController>("isGameActive", false, ReflectionUtils.FLAGS_STATIC_PRIVATE);

                LevelSave levelSave = SaveController.GetSaveObject<LevelSave>("level");
                levelSave.DisplayLevelNumber = Mathf.Clamp((level - 1), 0, int.MaxValue);
                levelSave.RealLevelNumber = levelSave.DisplayLevelNumber;

                GameController.RefreshLevelDev();
            }
        }

        public void PrevLevelDev()
        {
            ReflectionUtils.InjectInstanceComponent<GameController>("isGameActive", false, ReflectionUtils.FLAGS_STATIC_PRIVATE);

            LevelSave levelSave = SaveController.GetSaveObject<LevelSave>("level");
            levelSave.DisplayLevelNumber = Mathf.Clamp(levelSave.DisplayLevelNumber - 1, 0, int.MaxValue);
            levelSave.RealLevelNumber = levelSave.DisplayLevelNumber;

            GameController.RefreshLevelDev();
        }

        public void NextLevelDev()
        {
            ReflectionUtils.InjectInstanceComponent<GameController>("isGameActive", false, ReflectionUtils.FLAGS_STATIC_PRIVATE);

            LevelSave levelSave = SaveController.GetSaveObject<LevelSave>("level");
            levelSave.DisplayLevelNumber = levelSave.DisplayLevelNumber + 1;
            levelSave.RealLevelNumber = levelSave.DisplayLevelNumber;

            GameController.RefreshLevelDev();
        }

        #endregion
    }
}
