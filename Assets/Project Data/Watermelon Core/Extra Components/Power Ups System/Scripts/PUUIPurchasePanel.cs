using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class PUUIPurchasePanel : MonoBehaviour, IPopupWindow
    {
        [SerializeField] GameObject powerUpPurchasePanel;
        [SerializeField] RectTransform safeAreaTransform;

        [Space(5)]
        [SerializeField] Image powerUpPurchasePreview;
        [SerializeField] TMP_Text powerUpPurchaseAmountText;
        [SerializeField] TMP_Text powerUpPurchaseDescriptionText;
        [SerializeField] TMP_Text powerUpPurchasePriceText;
        [SerializeField] Image powerUpPurchaseIcon;

        [Space(5)]
        [SerializeField] Button smallCloseButton;
        [SerializeField] Button bigCloseButton;
        [SerializeField] Button purchaseButton;

        [Space(5)]
        [SerializeField] CurrencyUIPanelSimple currencyPanel;

        private PUSettings settings;

        private bool isOpened;
        public bool IsOpened => isOpened;

        private void Awake()
        {
            smallCloseButton.onClick.AddListener(ClosePurchasePUPanel);
            bigCloseButton.onClick.AddListener(ClosePurchasePUPanel);
            purchaseButton.onClick.AddListener(PurchasePUButton);
        }

        public void Initialise()
        {
            NotchSaveArea.RegisterRectTransform(safeAreaTransform);
        }

        public void Show(PUSettings settings)
        {
            this.settings = settings;

            currencyPanel.Initialise();

            powerUpPurchasePanel.SetActive(true);

            powerUpPurchasePreview.sprite = settings.Icon;
            powerUpPurchaseDescriptionText.text = settings.Description;
            powerUpPurchasePriceText.text = settings.Price.ToString();
            powerUpPurchaseAmountText.text = string.Format("x{0}", settings.PurchaseAmount);

            Currency currency = CurrenciesController.GetCurrency(settings.CurrencyType);
            powerUpPurchaseIcon.sprite = currency.Icon;

            UIController.OnPopupWindowOpened(this);
        }

        public void PurchasePUButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            bool purchaseSuccessful = PUController.PurchasePowerUp(settings.Type);

            if (purchaseSuccessful)
                ClosePurchasePUPanel();
        }

        public void ClosePurchasePUPanel()
        {
            powerUpPurchasePanel.SetActive(false);

            UIController.OnPopupWindowClosed(this);
        }
    }
}