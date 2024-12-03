using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UILevelQuitPopUp : MonoBehaviour
    {
        [SerializeField] Button closeSmallButton;
        [SerializeField] Button closeBigButton;
        [SerializeField] Button confirmButton;

        public SimpleCallback OnCancelExitEvent;
        public SimpleCallback OnConfirmExitEvent;

        private void Awake()
        {
            closeSmallButton.onClick.AddListener(ExitPopCloseButton);
            closeBigButton.onClick.AddListener(ExitPopCloseButton);
            confirmButton.onClick.AddListener(ExitPopUpConfirmExitButton);

            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ExitPopCloseButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            OnCancelExitEvent?.Invoke();

            gameObject.SetActive(false);
        }

        public void ExitPopUpConfirmExitButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            OnConfirmExitEvent?.Invoke();

            gameObject.SetActive(false);
        }
    }
}
