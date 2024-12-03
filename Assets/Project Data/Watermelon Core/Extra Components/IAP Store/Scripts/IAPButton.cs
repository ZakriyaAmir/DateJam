using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if MODULE_IAP
using UnityEngine.Purchasing;
#endif

namespace Watermelon.IAPStore
{
    public class IAPButton : MonoBehaviour
    {
        [SerializeField] Image backImage;
        [SerializeField] Button button;
        [SerializeField] TMP_Text priceText;
        [SerializeField] GameObject loadingObject;

        [Space]
        [SerializeField] Sprite activeBackSprite;
        [SerializeField] Sprite unactiveBackSprite;

        private ProductKeyType key;
        private ProductData product;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        public void Init(ProductKeyType key)
        {
            this.key = key;

            product = IAPManager.GetProductData(key);

            UpdateState();
        }

        private void UpdateState()
        {
            if (product != null)
            {
                loadingObject.SetActive(false);
                priceText.gameObject.SetActive(true);

                backImage.sprite = activeBackSprite;

                priceText.text = IAPManager.GetProductLocalPriceString(key);
            }
            else
            {
                SetDisabledState();
            }
        }

        private void SetDisabledState()
        {
            loadingObject.SetActive(true);
            priceText.gameObject.SetActive(false);

            backImage.sprite = unactiveBackSprite;
        }

        private void OnButtonClicked()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            IAPManager.BuyProduct(key);
        }
    }
}