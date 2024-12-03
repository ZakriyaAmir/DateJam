using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SkinStore
{
    public class UISkinItem : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Image productImage;
        [SerializeField] Image productMaskImage;

        [Space]
        [SerializeField] Image costOutline;
        [SerializeField] Image costBackground;
        [SerializeField] Image currencyImage;
        [SerializeField] TextMeshProUGUI costText;
        

        [Space]
        [SerializeField] Color inGamePurchaseTypeBackColor;
        [SerializeField] Color rewardedPurchaseTypeBackColor;

        [SerializeField] Color availableCostColor;
        [SerializeField] Color notAvailableCostColor;

        [Space]
        [SerializeField] Image selectionOutlineImage;

        [Space]
        [SerializeField] SimpleBounce bounce;

        public SkinData Data { get; private set; }

        public bool IsSelected { get; private set; }

        public void Init(SkinData data, bool selected)
        {
            Data = data;

            bounce.Initialise(transform);

            if (data.IsDummy)
            {
                productImage.sprite = data.LockedSprite;
                costOutline.gameObject.SetActive(false);
            }
            else
            {
                if (data.IsUnlocked)
                {
                    productImage.sprite = data.OpenedSprite;
                    costOutline.gameObject.SetActive(false);
                    button.enabled = true;
                }
                else
                {
                    productImage.sprite = data.LockedSprite;
                    costOutline.gameObject.SetActive(true);


                    if (data.PurchType == SkinData.PurchaseType.InGameCurrency)
                    {
                        costBackground.color = inGamePurchaseTypeBackColor;
                        currencyImage.sprite = CurrenciesController.GetCurrency(data.Currency).Icon;
                        costText.text = data.Cost.ToString();
                    }
                    else
                    {
                        costBackground.color = rewardedPurchaseTypeBackColor;
                        currencyImage.sprite = UIController.GetPage<UISkinStore>().AdsIcon;
                        costText.text = data.RewardedVideoWatchedAmount + "/" + data.Cost.ToString();

                        button.enabled = true;
                    }

                    UpdatePriceText();
                }
            }

            productMaskImage.color = SkinStoreController.SelectedTabData.ProductBackgroundColor;

            SetSelectedStatus(selected);
        }

        public void SetSelectedStatus(bool isSelected)
        {
            IsSelected = isSelected;

            if (Data.IsDummy || isSelected)
            {
                button.enabled = false;
            }
            else if (Data.IsUnlocked)
            {
                button.enabled = true;
            }
            else if (Data.PurchType == SkinData.PurchaseType.RewardedVideo)
            {
                button.enabled = true;
            }
            else
            {
                button.enabled = CurrenciesController.HasAmount(Data.Currency, Data.Cost);
            }

            selectionOutlineImage.gameObject.SetActive(isSelected);
        }

        public void UpdatePriceText()
        {
            if (Data.PurchType == SkinData.PurchaseType.InGameCurrency)
            {
                costText.color = CurrenciesController.HasAmount(Data.Currency, Data.Cost) ? availableCostColor : notAvailableCostColor;
            }
            else
            {
                costText.color = availableCostColor;
                costText.text = Data.RewardedVideoWatchedAmount + "/" + Data.Cost;
            }    
        }

        public void OnButtonClicked()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            bounce.Bounce();

            if (Data.IsUnlocked)
            {
                SkinStoreController.SelectProduct(Data);
            }
            else
            {
                SkinStoreController.BuyProduct(Data);
            }
        }
    }
}