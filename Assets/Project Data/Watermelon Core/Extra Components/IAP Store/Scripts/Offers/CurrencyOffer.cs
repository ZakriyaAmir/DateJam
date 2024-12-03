using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon.IAPStore
{
    public class CurrencyOffer : IAPStoreOffer
    {
        [SerializeField] int coinsAmount;
        [SerializeField] TMP_Text currencyAmountText;

        [Space]
        [SerializeField] RectTransform cloudSpawnRectTransform;
        [SerializeField] int floatingElementsAmount = 10;

        protected override void Awake()
        {
            base.Awake();
            currencyAmountText.text = $"x{coinsAmount}";
        }

        protected override void ApplyOffer()
        {
            UIIAPStore iapStore = UIController.GetPage<UIIAPStore>();
            iapStore.SpawnCurrencyCloud(cloudSpawnRectTransform, CurrencyType.Coins, floatingElementsAmount, () =>
            {
                CurrenciesController.Add(CurrencyType.Coins, coinsAmount);
            });
        }

        protected override void ReapplyOffer()
        {
            
        }
    }
}