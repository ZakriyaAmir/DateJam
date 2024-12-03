using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class PUUIController : MonoBehaviour
    {
        [SerializeField] Transform containerTransform;
        [SerializeField] GameObject itemPrefab;

        [Space]
        [SerializeField] RectTransform floatingTextRectTransform;
        [SerializeField] TextMeshProUGUI floatingText;
        [SerializeField] float floatingTextDelay = 1.0f;
        [SerializeField] Ease.Type floatingTextEasing = Ease.Type.QuartOut;

        [Space]
        [SerializeField] PUUIPurchasePanel powerUpPurchasePanel;
        public PUUIPurchasePanel PowerUpPurchasePanel => powerUpPurchasePanel;

        private PUController powerUpController;

        private PUUIBehavior[] uiBehaviors;
        public PUUIBehavior[] UIBehaviors => uiBehaviors;

        private float defaultFloatingTextWidth;
        private Vector2 defaultFloatingTextPosition;
        private TweenCase floatingTextTweenCase;
        private RectTransform rectTransform;

        public void Initialise(PUController powerUpController)
        {
            this.powerUpController = powerUpController;

            rectTransform = (RectTransform)transform;

            defaultFloatingTextPosition = floatingTextRectTransform.anchoredPosition;
            defaultFloatingTextWidth = floatingTextRectTransform.sizeDelta.x;

            // Create UI elements
            PUBehavior[] activePowerUps = PUController.ActivePowerUps;
            uiBehaviors = new PUUIBehavior[activePowerUps.Length];

            for (int i = 0; i < activePowerUps.Length; i++)
            {
                GameObject itemObject = Instantiate(itemPrefab, containerTransform);

                uiBehaviors[i] = itemObject.GetComponent<PUUIBehavior>();
                uiBehaviors[i].Initialise(activePowerUps[i]);
            }

            powerUpPurchasePanel.Initialise();
        }

        private void Update()
        {
            foreach(var uiBehavior in uiBehaviors)
            {
                if (uiBehavior.Behavior.IsDirty)
                {
                    uiBehavior.Redraw();
                }
            }
        }

        public void OnLevelStarted(int levelIndex)
        {
            for (int i = 0; i < uiBehaviors.Length; i++)
            {
                if(uiBehaviors[i].Settings.RequiredLevel <= levelIndex)
                {
                    uiBehaviors[i].Activate();
                }
            }
        }

        public void OnPowerUpUsed(PUBehavior powerUpBehavior)
        {
            RedrawPanels();

            string floatingMessageText = powerUpBehavior.GetFloatingMessage();
            if (!string.IsNullOrEmpty(floatingMessageText))
                SpawnFloatingText(floatingMessageText);
        }

        public void SpawnFloatingText(string text)
        {
            floatingTextTweenCase.KillActive();

            floatingTextRectTransform.gameObject.SetActive(true);

            floatingText.text = text;
            floatingText.ForceMeshUpdate();

            float prefferedHeight = LayoutUtility.GetPreferredHeight(floatingText.rectTransform);

            floatingTextRectTransform.sizeDelta = new Vector2(defaultFloatingTextWidth, prefferedHeight);
            floatingTextRectTransform.anchoredPosition = defaultFloatingTextPosition;

            floatingTextTweenCase = floatingTextRectTransform.DOAnchoredPosition(new Vector2(defaultFloatingTextPosition.x, defaultFloatingTextPosition.y + 50), floatingTextDelay).SetEasing(floatingTextEasing).OnComplete(() =>
            {
                floatingTextRectTransform.gameObject.SetActive(false);
            });
        }

        public void RedrawPanels()
        {
            for (int i = 0; i < uiBehaviors.Length; i++)
            {
                uiBehaviors[i].Redraw();
            }
        }

        public void HidePanels()
        {
            foreach (var uiBehavior in uiBehaviors)
            {
                uiBehavior.gameObject.SetActive(false);
            }
        }

        public void HidePanel(PUType puType)
        {
            foreach (var uiBehavior in uiBehaviors)
            {
                if (uiBehavior.Settings.Type == puType)
                {
                    uiBehavior.gameObject.SetActive(false);

                    break;
                }
            }
        }

        public void ShowPanels()
        {
            foreach (var uiBehavior in uiBehaviors)
            {
                uiBehavior.gameObject.SetActive(true);
            }
        }

        public void ShowPanel(PUType puType)
        {
            foreach (var uiBehavior in uiBehaviors)
            {
                if (uiBehavior.Settings.Type == puType)
                {
                    uiBehavior.gameObject.SetActive(true);

                    break;
                }
            }
        }

        public PUUIBehavior GetPanel(PUType puType)
        {
            foreach (var uiBehavior in uiBehaviors)
            {
                if (uiBehavior.Settings.Type == puType)
                {
                    return uiBehavior;
                }
            }

            return null;
        }
    }
}
