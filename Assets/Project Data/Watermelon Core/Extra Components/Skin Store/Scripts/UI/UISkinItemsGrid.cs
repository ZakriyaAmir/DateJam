using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SkinStore
{
    public class UISkinItemsGrid : MonoBehaviour
    {
        [SerializeField] GridLayoutGroup gridLayourGroup;

        private List<UISkinItem> storeItemsList = new List<UISkinItem>();
        public List<UISkinItem> StoreItemsList => storeItemsList;

        private Pool storeItemPool;

        public void Init(List<SkinData> products, string selectedProductId)
        {
            UISkinStore uiStore = UIController.GetPage<UISkinStore>();

            storeItemPool = PoolManager.GetPoolByName(uiStore.STORE_ITEM_POOL_NAME);
            storeItemsList.Clear();

            gridLayourGroup.enabled = true;

            for (int i = 0; i < products.Count; i++)
            {
                UISkinItem item = storeItemPool.GetPooledObject(new PooledObjectSettings().SetParrent(transform)).GetComponent<UISkinItem>();
                storeItemsList.Add(item);

                item.transform.localScale = Vector3.one;
                item.transform.SetParent(transform);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;

                item.Init(products[i], products[i].UniqueId == selectedProductId);
            }

            var height = 280 * (products.Count / 3 + 1) + 40 * (products.Count / 3f);

            var rect = GetComponent<RectTransform>();

            rect.sizeDelta = rect.sizeDelta.SetY(height);

            Tween.DelayedCall(0.1f, () => gridLayourGroup.enabled = false);
        }

        public void UpdateItems(string selectedProductId)
        {
            for (int i = 0; i < storeItemsList.Count; i++)
            {
                storeItemsList[i].SetSelectedStatus(storeItemsList[i].Data.UniqueId == selectedProductId);
                storeItemsList[i].UpdatePriceText();
            }
        }
    }
}

