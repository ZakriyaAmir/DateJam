using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.IAPStore
{
    public class NoAdsOffer : IAPStoreOffer
    {
        protected override void Awake()
        {
            base.Awake();

            IAPManager.OnPurchaseComplete += OnPurchaseComplete;
        }

        protected override void ApplyOffer()
        {
            AdsManager.DisableForcedAd();
        }

        protected override void ReapplyOffer()
        {
            AdsManager.DisableForcedAd();
        }

        private void OnPurchaseComplete(ProductKeyType productKeyType)
        {
            if(productKeyType == ProductKeyType.StarterPack) gameObject.SetActive(false);
        }
    }
}