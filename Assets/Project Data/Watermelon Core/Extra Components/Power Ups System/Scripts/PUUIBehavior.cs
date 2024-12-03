using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class PUUIBehavior : MonoBehaviour
    {
        [Group("Refs")]
        [SerializeField] Image backgroundImage;

        [Group("Refs")]
        [SerializeField] Image iconImage;

        [Group("Refs")]
        [SerializeField] GameObject amountContainerObject;

        [Group("Refs")]
        [SerializeField] TextMeshProUGUI amountText;

        [Group("Refs")]
        [SerializeField] GameObject amountPurchaseObject;

        [Group("Refs")]
        [SerializeField] GameObject busyStateVisualsObject;

        [Space]
        [SerializeField] GameObject timerObject;
        [SerializeField] TextMeshProUGUI timerText;
        [SerializeField] Image timerBackground;

        [Space]
        [SerializeField] SimpleBounce bounce;

        protected PUBehavior behavior;
        public PUBehavior Behavior => behavior;

        protected PUSettings settings;
        public PUSettings Settings => settings;

        private Button button;

        private bool isTimerActive;
        private Coroutine timerCoroutine;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => OnButtonClicked());
        }

        public void Initialise(PUBehavior powerUpBehavior)
        {
            behavior = powerUpBehavior;
            settings = powerUpBehavior.Settings;

            ApplyVisuals();

            Redraw();

            bounce.Initialise(transform);

            gameObject.SetActive(false);
        }

        protected virtual void ApplyVisuals()
        {
            iconImage.sprite = settings.Icon;
            iconImage.color = Color.white;

            backgroundImage.color = settings.BackgroundColor;
            timerBackground.color = settings.BackgroundColor.SetAlpha(0.7f);
        }

        public void Activate()
        {
            gameObject.SetActive(true);

            transform.localScale = Vector3.zero;
            transform.DOScale(1.0f, 0.3f).SetEasing(Ease.Type.BackOut);

            Redraw();
        }

        private IEnumerator TimerCoroutine(PUTimer timer)
        {
            isTimerActive = true;

            timerObject.SetActive(true);
            timerBackground.fillAmount = 1.0f;
            timerText.text = timer.Seconds;

            iconImage.color = new Color(1, 1, 1, 0.3f);

            while (timer.IsActive)
            {
                yield return null;
                yield return null;

                timerBackground.fillAmount = 1.0f - timer.State;
                timerText.text = timer.Seconds;

                if (timerBackground.fillAmount <= 0.0f)
                    break;
            }

            timerObject.SetActive(false);
            iconImage.color = Color.white;

            isTimerActive = false;
        }

        public void OnButtonClicked()
        {
            if (settings.Save.Amount > 0)
            {
                if (!behavior.IsBusy)
                {
                    AudioController.PlaySound(AudioController.Sounds.buttonSound);

                    bounce.Bounce();

                    PUController.UsePowerUp(settings.Type);
                }
            }
            else
            {
                AudioController.PlaySound(AudioController.Sounds.buttonSound);

                PUController.PowerUpsUIController.PowerUpPurchasePanel.Show(settings);
            }
        }

        public void Redraw()
        {
            int amount = settings.Save.Amount;
            if (amount > 0)
            {
                amountContainerObject.SetActive(true);
                amountPurchaseObject.SetActive(false);

                amountText.text = amount.ToString();
            }
            else
            {
                amountContainerObject.SetActive(false);
                amountPurchaseObject.SetActive(true);
            }

            PUTimer timer = behavior.GetTimer();
            if (!isTimerActive)
            {
                if (timer != null)
                {
                    timerCoroutine = StartCoroutine(TimerCoroutine(timer));
                }
            }

            if (settings.VisualiseActiveState)
                RedrawBusyVisuals(behavior.IsBusy);

            behavior.OnRedrawn();
        }

        protected virtual void RedrawBusyVisuals(bool state)
        {
            busyStateVisualsObject.SetActive(behavior.IsBusy);
        }
    }
}
