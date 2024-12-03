using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.IAPStore
{
    [System.Serializable]
    public class IAPStoreOfferData
    {
        [SerializeField] ProductKeyType productKey;
        [SerializeField] GameObject prefab;
    }
}
