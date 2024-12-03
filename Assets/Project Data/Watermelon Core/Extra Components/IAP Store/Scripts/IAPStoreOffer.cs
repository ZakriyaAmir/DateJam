using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if MODULE_IAP
using UnityEngine.Purchasing;
#endif

namespace Watermelon.IAPStore
{
    public abstract class IAPStoreOffer : MonoBehaviour, IIAPStoreOffer
    {
        [SerializeField] ProductKeyType productKey;

        [Space]
        [SerializeField] IAPButton purchaseButton;

        public GameObject GameObject => gameObject;

        private RectTransform rect;
        public float Height => rect.sizeDelta.y;

        private SimpleBoolSave save;
        protected bool Bought => !save.Value;

        private ProductData product;

        protected virtual void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public void Init()
        {
            save = SaveController.GetSaveObject<SimpleBoolSave>($"Product_{productKey}");

            product = IAPManager.GetProductData(productKey);

            if (product.IsPurchased || product.ProductType == ProductType.NonConsumable && save.Value)
            {
                ReapplyOffer();
                if (product.ProductType == ProductType.NonConsumable)
                    Disable();
                else
                    purchaseButton.Init(productKey);
            }
            else
            {
                purchaseButton.Init(productKey);
                IAPManager.OnPurchaseComplete += OnPurchaseComplete;
            }
        }

        public void Disable()
        {
            IAPManager.OnPurchaseComplete -= OnPurchaseComplete;

            gameObject.SetActive(false);
        }

        private void OnPurchaseComplete(ProductKeyType key)
        {
            if (productKey == key)
            {
                ApplyOffer();

                if (product.ProductType == ProductType.NonConsumable) Disable();

                save.Value = true;
            }
        }

        protected abstract void ApplyOffer();
        protected abstract void ReapplyOffer();
    }
}