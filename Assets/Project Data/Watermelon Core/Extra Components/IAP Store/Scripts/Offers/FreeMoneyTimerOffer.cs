using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Watermelon.Currency;

namespace Watermelon.IAPStore
{
    public class FreeMoneyTimerOffer : MonoBehaviour, IIAPStoreOffer
    {
        [SerializeField] int coinsAmount;
        [SerializeField] TMP_Text coinsAmountText;

        [Space]
        [SerializeField] TMP_Text timerText;
        [SerializeField] int timerDurationInMinutes;

        [Space]
        [SerializeField] Button button;

        [Space]
        [SerializeField] RectTransform cloudSpawnRectTransform;
        [SerializeField] int floatingElementsAmount = 10;

        public GameObject GameObject => gameObject;

        private RectTransform rect;
        public float Height => rect.sizeDelta.y;

        SimpleLongSave save;
        DateTime timerStartTime;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public void Init()
        {
            save = SaveController.GetSaveObject<SimpleLongSave>("Free Money Timer");

            timerStartTime = DateTime.FromBinary(save.Value);

            button.onClick.AddListener(OnAdButtonClicked);
            coinsAmountText.text = $"x{coinsAmount}";
        }

        private void Update()
        {
            var timer = DateTime.Now - timerStartTime;
            var duration = TimeSpan.FromMinutes(timerDurationInMinutes);
            if (timer > duration)
            {
                button.enabled = true;

                timerText.text = "Get!";
            } else
            {
                button.enabled = false;

                var timeLeft = duration - timer;

                if (timeLeft.Hours > 0)
                {
                    timerText.text = string.Format("{0:hh\\:mm\\:ss}", timeLeft);
                } else
                {
                    timerText.text = string.Format("{0:mm\\:ss}", timeLeft);
                }

                var prefferedWidth = timerText.preferredWidth;
                if (prefferedWidth < 270) prefferedWidth = 270;

                timerText.rectTransform.sizeDelta = timerText.rectTransform.sizeDelta.SetX(prefferedWidth + 5);
                button.image.rectTransform.sizeDelta = button.image.rectTransform.sizeDelta.SetX(prefferedWidth + 10);
            }
        }

        private void OnAdButtonClicked()
        {
            save.Value = DateTime.Now.ToBinary();
            timerStartTime = DateTime.Now;

            UIIAPStore iapStore = UIController.GetPage<UIIAPStore>();
            iapStore.SpawnCurrencyCloud(cloudSpawnRectTransform, CurrencyType.Coins, floatingElementsAmount, () =>
            {
                CurrenciesController.Add(CurrencyType.Coins, coinsAmount);
            });
        }
    }
}