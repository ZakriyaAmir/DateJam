using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.IAPStore
{
    public class UIIAPStore : UIPage
    {
        [SerializeField] VerticalLayoutGroup layout;
        [SerializeField] RectTransform safeAreaTransform;
        [SerializeField] RectTransform content;
        [SerializeField] Button closeButton;
        [SerializeField] CurrencyUIPanelSimple coinsUI;

        private TweenCase[] appearTweenCases;

        public static bool IsStoreAvailable { get; private set; } = false;

        private List<IIAPStoreOffer> offers = new List<IIAPStoreOffer>();

        private void Awake()
        {
            content.GetComponentsInChildren(offers);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        public override void Initialise()
        {
            IAPManager.SubscribeOnPurchaseModuleInitted(InitOffers);

            NotchSaveArea.RegisterRectTransform(safeAreaTransform);

            coinsUI.Initialise();
        }

        private void InitOffers()
        {
            foreach (var offer in offers)
            {
                offer.Init();
            }

            IsStoreAvailable = true;

            Tween.NextFrame(() => {

                float height = layout.padding.top + layout.padding.bottom;

                for (int i = 0; i < offers.Count; i++)
                {
                    var offer = offers[i];
                    if (offer.GameObject.activeSelf)
                    {
                        height += offer.Height;
                        if(i != offers.Count - 1) height += layout.spacing;
                    }
                }

                content.sizeDelta = content.sizeDelta.SetY(height + 200);
            });
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            appearTweenCases.KillActive();

            appearTweenCases = new TweenCase[offers.Count];
            for (int i = 0; i < offers.Count; i++)
            {
                Transform offerTransform = offers[i].GameObject.transform;
                offerTransform.transform.localScale = Vector3.zero;
                appearTweenCases[i] = offerTransform.transform.DOScale(1.0f, 0.3f, i * 0.05f).SetEasing(Ease.Type.CircOut);
            }

            closeButton.transform.localScale = Vector3.zero;
            closeButton.transform.DOScale(1.0f, 0.3f, 0.2f).SetEasing(Ease.Type.BackOut);

            content.anchoredPosition = Vector2.zero;

            appearTweenCases[^1].OnComplete(() =>
            {
                UIController.OnPageOpened(this);
            });
        }

        public void Hide()
        {
            appearTweenCases.KillActive();

            UIController.HidePage<UIIAPStore>();
        }

        private void OnCloseButtonClicked()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            UIController.HidePage<UIIAPStore>();
        }

        public void SpawnCurrencyCloud(RectTransform spawnRectTransform, CurrencyType currencyType, int amount, SimpleCallback completeCallback = null)
        {
            FloatingCloud.SpawnCurrency(currencyType.ToString(), spawnRectTransform, coinsUI.RectTransform, amount, null, completeCallback);
        }
    }
}

// -----------------
// IAP Store v1.2
// -----------------

// Changelog
// v 1.2
// • Added mobile notch offset support
// • Added free timer money offer
// • Added ad money offer
// v 1.1
// • Added offers interface
// • Offers prefabs renamed
// v 1.0
// • Basic logic