using UnityEngine;

namespace Watermelon
{
    [SetupTab("IAP", texture = "icon_iap")]
    [CreateAssetMenu(fileName = "IAP Settings", menuName = "Settings/IAP Settings")]
    [HelpURL("https://docs.google.com/document/d/1GlS55aF4z4Ddn4a1QCu5h0152PoOb29Iy4y9RKZ9Y9Y")]
    public class IAPSettings : ScriptableObject
    {
        [SerializeField] IAPItem[] storeItems;
        public IAPItem[] StoreItems => storeItems;

        [Group("Settings")]
        [SerializeField] bool useTestMode;
        public bool UseTestMode => useTestMode;
    }
}