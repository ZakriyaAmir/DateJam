using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon.IAPStore
{
    public class StartedPackOffer : IAPStoreOffer
    {
        [SerializeField, Tooltip("In hours")] int infiniteLifeDuration;

        [Space]

#if MODULE_POWERUPS
        [SerializeField] List<PUType> powerUps;
        [SerializeField] int powerUpsAmount;
#endif

        [SerializeField] int coinsAmount;

        [Space]

#if MODULE_POWERUPS
        [SerializeField] TMP_Text powerUpsText;
#endif

        [SerializeField] TMP_Text livesText;
        [SerializeField] TMP_Text coinsText;

        protected override void Awake()
        {
            base.Awake();

#if MODULE_POWERUPS
            powerUpsText.text = $"x{powerUpsAmount}";
#endif

            coinsText.text = $"x{coinsAmount}";
            livesText.text = $"{infiniteLifeDuration}hrs";
        }

        protected override void ApplyOffer()
        {
            LivesManager.StartInfiniteLives(infiniteLifeDuration * 60 * 60);

#if MODULE_POWERUPS
            for (int i = 0; i < powerUps.Count; i++)
            {
                if(System.Enum.IsDefined(typeof(PUType), powerUps[i]))
                {
                    var type = powerUps[i];

                    PUController.AddPowerUp(type, powerUpsAmount);
                }
            }
#endif

            UIIAPStore iapStore = UIController.GetPage<UIIAPStore>();
            iapStore.SpawnCurrencyCloud((RectTransform)transform, CurrencyType.Coins, 15, () =>
            {
                CurrenciesController.Add(CurrencyType.Coins, coinsAmount);
            });

            AdsManager.DisableForcedAd();
        }

        protected override void ReapplyOffer()
        {
            AdsManager.DisableForcedAd();
        }
    }
}