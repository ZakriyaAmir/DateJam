using UnityEngine;

namespace Watermelon
{
    public abstract class PUSettings : ScriptableObject
    {
        [SerializeField] PUType type;
        public PUType Type => type;

        [Group("Refs")]
        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [Group("Refs")]
        [SerializeField] GameObject behaviorPrefab;
        public GameObject BehaviorPrefab => behaviorPrefab;

        [Group("Variables")]
        [SerializeField] int defaultAmount;
        public int DefaultAmount => defaultAmount;

        [Group("Variables")]
        [SerializeField] string description;
        public string Description => description;

        [Group("Variables")]
        [SerializeField] int requiredLevel;
        public int RequiredLevel => requiredLevel;

        [LineSpacer("UI")]
        [Group("UI")]
        [SerializeField] bool visualiseActiveState = false;
        public bool VisualiseActiveState => visualiseActiveState;

        [Group("UI")]
        [SerializeField] Color backgroundColor = Color.white;
        public Color BackgroundColor => backgroundColor;

        [LineSpacer("Purchase")]
        [Group("Purchase")]
        [SerializeField] CurrencyType currencyType;
        public CurrencyType CurrencyType => currencyType;

        [Group("Purchase")]
        [SerializeField] int price;
        public int Price => price;

        [Group("Purchase")]
        [SerializeField] int purchaseAmount;
        public int PurchaseAmount => purchaseAmount;

        [LineSpacer("Floating Text")]
        [SerializeField] string floatingMessage;
        public string FloatingMessage => floatingMessage;

        [System.NonSerialized]
        private PUSave save;
        public PUSave Save => save;

        public void InitialiseSave()
        {
            save = SaveController.GetSaveObject<PUSave>(string.Format("powerUp_{0}", type));

            // Set default amount if amount is equal -1
            if (save.Amount == -1)
                save.Amount = defaultAmount;
        }

        public abstract void Initialise();

        public bool HasEnoughCurrency()
        {
            return CurrenciesController.HasAmount(currencyType, price);
        }
    }
}
