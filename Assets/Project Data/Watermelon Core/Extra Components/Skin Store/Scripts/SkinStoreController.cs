using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SkinStore
{
    public class SkinStoreController : MonoBehaviour
    {
        private static SkinStoreController instance;

        [SerializeField] SkinsDatabase database;
        public static SkinsDatabase Database => instance.database;

        private static Dictionary<TabData, List<SkinData>> products;
        private static Dictionary<SkinTab, SkinData> selectedProducts;

        public static int TabsCount => Database.Tabs.Length;
        public static int CoinsForAdsAmount => Database.CoinsForAds;
        public static CurrencyType CoinsForAdsCurrency => Database.CurrencyForAds;

        public static TabData SelectedTabData { get; private set; }

        public delegate void ProductDelegate(SkinTab tab, SkinData product);
        public static event ProductDelegate OnProductSelected;
        private static UISkinStore storeUI;

        private void Awake()
        {
            instance = this;
        }

        public static void Init()
        {
            products = Database.Init();

            storeUI = UIController.GetPage<UISkinStore>();
            storeUI.InitTabs(OnTabClicked);

            InitDefaultProducts();
            InitSelectedProducts();

            SelectedTabData = Database.Tabs[0];
        }

        private static void InitDefaultProducts()
        {
            var visitedTypes = new List<SkinTab>();

            foreach (var tab in products.Keys)
            {
                if (visitedTypes.Contains(tab.Type))
                    continue;
                visitedTypes.Add(tab.Type);

                var page = products[tab];

                if (page.Count > 0)
                {
                    var defaultProduct = page[0];
                    defaultProduct.IsUnlocked = true;
                }
            }
        }

        private static void InitSelectedProducts()
        {
            selectedProducts = new Dictionary<SkinTab, SkinData>();

            for (int i = 0; i < Database.TabTypes.Length; i++)
            {
                var type = Database.TabTypes[i];
                var selectedId = Database[type];

                for (int j = 0; j < Database.Tabs.Length; j++)
                {
                    var tab = Database.Tabs[j];

                    if (tab.Type != type)
                        continue;

                    foreach (var product in products[tab])
                    {
                        if (product.UniqueId == selectedId)
                        {
                            product.IsUnlocked = true;
                            selectedProducts.Add(type, product);

                            // I know, blasphemy
                            goto nextType;
                        }
                    }

                }

                // If there is no selected, select first
                for (int j = 0; j < Database.Tabs.Length; j++)
                {
                    var tab = Database.Tabs[j];

                    if (tab.Type != type)
                        continue;

                    var firstProduct = products[tab][0];
                    firstProduct.IsUnlocked = true;

                    selectedProducts.Add(type, firstProduct);

                    break;
                }

                nextType:
                ;
            }
        }

        public static TabData GetTab(int tabId)
        {
            return Database.Tabs[tabId];
        }

        public static List<SkinData> GetProducts(TabData tab)
        {
            return products[tab];
        }

        public static int PagesCount(TabData tab)
        {
            return products[tab].Count;
        }

        public static void OpenStore()
        {
            UIController.ShowPage<UISkinStore>();
        }

        private static void OnTabClicked(TabData data)
        {
            SelectedTabData = data;

            storeUI.SetSelectedTab(data);
        }

        public static bool SelectProduct(SkinData product)
        {
            if (!product.IsUnlocked)
                return false;

            var tab = Database.Tabs[product.TabId];
            Database[tab.Type] = product.UniqueId;

            selectedProducts[tab.Type] = product;

            storeUI.InitStoreUI();

            OnProductSelected?.Invoke(tab.Type, product);

            return true;
        }

        public static bool BuyProduct(SkinData product, bool select = true, bool free = false)
        {
            if (product.IsUnlocked)
                return SelectProduct(product);

            if (free)
            {
                product.IsUnlocked = true;
                if (select)
                    SelectProduct(product);

                return true;
            }
            else if (product.PurchType == SkinData.PurchaseType.InGameCurrency && CurrenciesController.HasAmount(product.Currency, product.Cost))
            {
                CurrenciesController.Substract(product.Currency, product.Cost);

                product.IsUnlocked = true;
                if (select)
                    SelectProduct(product);

                return true;
            }
            // note | this type can't return true or false because result is not defined during execution of this code
            // right now result of this method is not used, but otherwise this logic needs to be improved
            else if(product.PurchType == SkinData.PurchaseType.RewardedVideo)
            {
                AdsManager.ShowRewardBasedVideo((success) =>
                {
                    if(success)
                    {
                        product.RewardedVideoWatchedAmount++;

                        if(product.RewardedVideoWatchedAmount >= product.Cost)
                        {
                            product.IsUnlocked = true;
                            if (select)
                                SelectProduct(product);
                        }

                        storeUI.InitStoreUI();
                    }
                });
            }

            return false;
        }

        public static SkinData GetSelectedProductData(SkinTab tabType)
        {
            return selectedProducts[tabType];
        }

        public static SkinData GetSelectedProductData(TabData tabData)
        {
            return selectedProducts[tabData.Type];
        }

        public static SkinData GetSelectedProductData(int tabId)
        {
            var tabData = Database.Tabs[tabId];
            return selectedProducts[tabData.Type];
        }

        public static GameObject GetSelectedPrefab(SkinTab type)
        {
            return selectedProducts[type].Prefab;
        }

        public static GameObject GetSelectedPrefab(int tabId)
        {
            var tabData = Database.Tabs[tabId];

            return selectedProducts[tabData.Type].Prefab;
        }

        public static GameObject GetSelectedPrefab(TabData tabData)
        {
            return selectedProducts[tabData.Type].Prefab;
        }

        public static SkinData GetRandomLockedProduct()
        {
            SkinData lockedProduct = null;

            Database.Tabs.FindRandomOrder(tab =>
            {
                var product = products[tab].FindRandomOrder(product =>
                {
                    return !product.IsUnlocked && !product.IsDummy;
                });

                if (product != null)
                {
                    lockedProduct = product;
                    return true;
                }

                return false;
            });

            return lockedProduct;
        }

        public static SkinData GetRandomUnlockedProduct(SkinTab tab)
        {
            return products[GetTab((int)tab)].FindRandomOrder(product =>
            {
                return product.IsUnlocked && !product.IsDummy;
            });
        }

        public static SkinData GetRandomProduct(SkinTab tab)
        {
            return products[GetTab((int)tab)].GetRandomItem();
        }
    }
}