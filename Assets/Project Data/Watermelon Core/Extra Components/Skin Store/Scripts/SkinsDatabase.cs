using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SkinStore
{
    [CreateAssetMenu(fileName = "Skins Database", menuName = "Content/Data/Skins Database")]
    public class SkinsDatabase : ScriptableObject
    {
        [SerializeField] TabData[] tabs;
        public TabData[] Tabs => tabs;

        [SerializeField] SkinData[] products;

        [SerializeField] int coinsForAdsAmount;
        [SerializeField] CurrencyType currencyForAds;

        private Dictionary<SkinTab, SimpleStringSave> selectedProducts;

        public SkinTab[] TabTypes;

        public int CoinsForAds => coinsForAdsAmount;
        public CurrencyType CurrencyForAds => currencyForAds;

        public string this[SkinTab type]
        {
            get => selectedProducts[type].Value;
            set => selectedProducts[type].Value = value;
        }

        public Dictionary<TabData, List<SkinData>> Init()
        {
            var sortedProducts = new Dictionary<TabData, List<SkinData>>();

            for(int i = 0; i < products.Length; i++)
            {
                var product = products[i];
                var tab = tabs[product.TabId];

                if (sortedProducts.ContainsKey(tab))
                {
                    sortedProducts[tab].Add(product); ;
                }
                else
                {
                    sortedProducts.Add(tab, new List<SkinData> { product });
                }

                product.Init();
            }

            selectedProducts = new Dictionary<SkinTab, SimpleStringSave>();

            TabTypes = (SkinTab[]) Enum.GetValues(typeof(SkinTab));

            for(int i = 0; i < TabTypes.Length; i++)
            {
                var type = (SkinTab)TabTypes.GetValue(i);
                var save = SaveController.GetSaveObject<SimpleStringSave>($"Skin Store Type: {type}");
                selectedProducts.Add(type, save);
            }

            return sortedProducts;
        }
    }

    [System.Serializable]
    public class TabData
    {
        [SerializeField] string uniqueId;
        public string UniqueId => uniqueId;

        [SerializeField] string name;
        public string Name => name;

        [SerializeField] bool isActive = true;
        public bool IsActive => isActive;

        [SerializeField] SkinTab type;
        public SkinTab Type => type;

        [Space]
        [SerializeField] PreviewType previewType;
        public PreviewType PreviewType => previewType;

        [Space]
        [SerializeField] GameObject previewPrefab;
        public GameObject PreviewPrefab => previewPrefab;

        [SerializeField] Sprite backgroundImage;
        public Sprite BackgroundImage => backgroundImage;

        [SerializeField] Color backgroundColor = Color.white;
        public Color BackgroundColor => backgroundColor;

        [SerializeField] Color panelColor = Color.white;
        public Color PanelColor => panelColor;

        [SerializeField] Color tabActiveColor = Color.white;
        public Color TabActiveColor => tabActiveColor;

        [SerializeField] Color tabDisabledColor = Color.white;
        public Color TabDisabledColor => tabDisabledColor;

        [SerializeField] Color productBackgroundColor = Color.white;
        public Color ProductBackgroundColor => productBackgroundColor;

        public delegate void SimpleTabDelegate(TabData tab);
    }

    public enum PreviewType
    {
        Preview_2D,
        Preview_3D
    }
}